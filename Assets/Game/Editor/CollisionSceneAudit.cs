#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using Daejojeon.VR;

namespace Daejojeon.EditorTools
{
    [InitializeOnLoad]
    public static class CollisionSceneAudit
    {
        const string Request = "Library/CollisionSceneAudit.request";
        static double nextPoll;
        static CollisionSceneAudit() { EditorApplication.update += Poll; }
        static void Poll()
        {
            if (EditorApplication.timeSinceStartup < nextPoll) return;
            nextPoll = EditorApplication.timeSinceStartup + 1;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(Request)) return;
            var command = File.ReadAllText(Request).Trim();
            File.Delete(Request);
            try { Run(command); }
            catch (Exception e) { File.WriteAllText("Library/CollisionSceneAudit.error.txt", e.ToString()); Debug.LogException(e); }
        }
        [MenuItem("Tools/Daejojeon/Audit Scene Collisions")]
        public static void Audit() { Run("audit"); }
        [MenuItem("Tools/Daejojeon/Repair Scene Collisions")]
        public static void RepairMenu() { Run("repair"); }
        [MenuItem("Tools/Daejojeon/Validate Scene Collisions")]
        public static void ValidateMenu() { Run("validate"); }
        [Serializable] public class Report
        {
            public string scene, scenePath;
            public bool playing, dirty;
            public int meshes, colliders, enabledSolidColliders, missingMeshes, disabledColliders, triggers, convex, invalidDynamicMeshes;
            public List<string> rigs = new List<string>();
            public List<string> geometry = new List<string>();
            public List<string> colliderDetails = new List<string>();
            public List<string> uncoveredGeometry = new List<string>();
            public List<string> probes = new List<string>();
        }
        static string PathOf(Transform t) { return t.parent == null ? t.name : PathOf(t.parent) + "/" + t.name; }
        static IEnumerable<T> All<T>(Scene scene) where T : Component { return scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)); }
        static void Run(string command)
        {
            var s = SceneManager.GetActiveScene();
            if (command == "convex-all")
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode || s.path != "Assets/Game/Scenes/Scene1.unity")
                    throw new InvalidOperationException("Open Scene1 outside Play mode before setting convex.");
                Directory.CreateDirectory("Library/CollisionReports");
                var backupPath = "Library/CollisionReports/Scene1-before-convex.unity";
                if (!File.Exists(backupPath)) File.Copy(s.path, backupPath);
                var meshes = All<MeshCollider>(s).ToArray();
                var cookingMessages = new List<string>();
                Application.LogCallback capture = (message, stack, type) =>
                {
                    if (type != LogType.Log && (message.IndexOf("convex", StringComparison.OrdinalIgnoreCase) >= 0 || message.IndexOf("hull", StringComparison.OrdinalIgnoreCase) >= 0 || message.IndexOf("collider", StringComparison.OrdinalIgnoreCase) >= 0))
                        cookingMessages.Add(type + ": " + message);
                };
                int changed = 0;
                Undo.IncrementCurrentGroup();
                var undoGroup = Undo.GetCurrentGroup();
                Undo.SetCurrentGroupName("Enable Convex on all scene MeshColliders");
                Application.logMessageReceived += capture;
                try
                {
                    foreach (var mesh in meshes)
                    {
                        if (mesh.convex) continue;
                        Undo.RecordObject(mesh, "Enable Convex");
                        mesh.convex = true;
                        PrefabUtility.RecordPrefabInstancePropertyModifications(mesh);
                        changed++;
                    }
                    Physics.SyncTransforms();
                }
                finally { Application.logMessageReceived -= capture; }
                Undo.CollapseUndoOperations(undoGroup);
                EditorSceneManager.MarkSceneDirty(s);
                if (!EditorSceneManager.SaveScene(s)) throw new IOException("Could not save Scene1.");
                File.WriteAllText("Library/CollisionReports/convex-summary.txt", "MeshColliders: " + meshes.Length + "\nChanged: " + changed + "\nConvex enabled: " + meshes.Count(m => m.convex) + "\nCooking messages: " + cookingMessages.Count);
                File.WriteAllLines("Library/CollisionReports/convex-cooking-messages.txt", cookingMessages);
            }
            if (command == "validate-head") ValidateHeadGuard();
            if (command == "repair") Repair(s);
            if (command == "reinforce")
            {
                if (s.isDirty || EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Save and stop the scene first.");
                var changes = new List<string>();
                ReinforceWalls(s, changes);
                EditorSceneManager.MarkSceneDirty(s);
                EditorSceneManager.SaveScene(s);
                File.AppendAllLines("Library/CollisionReports/repair-changes.txt", changes);
            }
            if (command == "validate") Validate(s);
            var r = new Report { scene = s.name, scenePath = s.path, playing = EditorApplication.isPlaying, dirty = s.isDirty };
            foreach (var c in All<Collider>(s))
            {
                r.colliders++;
                if (c.enabled && !c.isTrigger && c.gameObject.activeInHierarchy) r.enabledSolidColliders++;
                if (!c.enabled) r.disabledColliders++;
                if (c.isTrigger) r.triggers++;
                var m = c as MeshCollider;
                if (m != null)
                {
                    if (m.sharedMesh == null) r.missingMeshes++;
                    if (m.convex) r.convex++;
                    if (!m.convex && m.attachedRigidbody != null && !m.attachedRigidbody.isKinematic) r.invalidDynamicMeshes++;
                }
                r.colliderDetails.Add(PathOf(c.transform) + " | " + c.GetType().Name + " enabled=" + c.enabled + " trigger=" + c.isTrigger + " active=" + c.gameObject.activeInHierarchy + " layer=" + c.gameObject.layer + " mesh=" + (m == null || m.sharedMesh == null ? "-" : m.sharedMesh.name) + " convex=" + (m != null && m.convex) + " bounds=" + c.bounds + " rigidbody=" + (c.attachedRigidbody == null ? "none" : c.attachedRigidbody.name + ":" + c.attachedRigidbody.isKinematic));
                if (m != null && m.sharedMesh != null && (m.name.Contains("Floor") || m.name.Contains("Wall") || m.name.Contains("Panelboard")))
                {
                    var bounds = c.bounds;
                    var top = new Ray(bounds.center + Vector3.up * (bounds.extents.y + .3f), Vector3.down);
                    var bottom = new Ray(bounds.center - Vector3.up * (bounds.extents.y + .3f), Vector3.up);
                    var right = new Ray(bounds.center + Vector3.right * (bounds.extents.x + .3f), Vector3.left);
                    var left = new Ray(bounds.center - Vector3.right * (bounds.extents.x + .3f), Vector3.right);
                    var front = new Ray(bounds.center + Vector3.forward * (bounds.extents.z + .3f), Vector3.back);
                    var back = new Ray(bounds.center - Vector3.forward * (bounds.extents.z + .3f), Vector3.forward);
                    RaycastHit hit;
                    r.probes.Add(PathOf(c.transform) + " | top=" + c.Raycast(top, out hit, bounds.size.y + .6f) + " bottom=" + c.Raycast(bottom, out hit, bounds.size.y + .6f) + " right=" + c.Raycast(right, out hit, bounds.size.x + .6f) + " left=" + c.Raycast(left, out hit, bounds.size.x + .6f) + " front=" + c.Raycast(front, out hit, bounds.size.z + .6f) + " back=" + c.Raycast(back, out hit, bounds.size.z + .6f));
                }
            }
            foreach (var m in All<MeshFilter>(s))
            {
                r.meshes++;
                if (m.sharedMesh == null) continue;
                var rend = m.GetComponent<MeshRenderer>();
                r.geometry.Add(PathOf(m.transform) + " | mesh=" + m.sharedMesh.name + " vertices=" + m.sharedMesh.vertexCount + " localBounds=" + m.sharedMesh.bounds + " worldBounds=" + (rend == null ? "-" : rend.bounds.ToString()) + " scale=" + m.transform.lossyScale + " ownColliders=" + m.GetComponents<Collider>().Length + " active=" + m.gameObject.activeInHierarchy);
                if (!m.GetComponentsInParent<Collider>().Any(c => c.enabled && !c.isTrigger)) r.uncoveredGeometry.Add(PathOf(m.transform));
            }
            foreach (var b in All<MonoBehaviour>(s))
            {
                if (b == null) continue;
                if (b.GetType().Name.Contains("Origin") || b.GetType().Name.Contains("BodyTransformer") || b.GetType().Name.Contains("MoveProvider"))
                    r.rigs.Add(PathOf(b.transform) + " | " + b.GetType().FullName + " " + EditorJsonUtility.ToJson(b));
            }
            foreach (var c in All<CharacterController>(s)) r.rigs.Add(PathOf(c.transform) + " | CharacterController " + EditorJsonUtility.ToJson(c) + " world=" + c.transform.position);
            Directory.CreateDirectory("Library/CollisionReports");
            File.WriteAllText("Library/CollisionReports/" + command + ".json", JsonUtility.ToJson(r, true));
            Debug.Log("Collision audit complete: " + s.name + ", " + r.meshes + " meshes, " + r.colliders + " colliders.");
        }

        static BoxCollider AddBox(GameObject obj, Bounds bounds, bool floor, bool ceiling)
        {
            var box = Undo.AddComponent<BoxCollider>(obj);
            var size = bounds.size;
            var center = bounds.center;
            var scale = obj.transform.lossyScale;
            // Floor top and ceiling underside stay aligned with visible geometry.
            var thickness = .12f / Mathf.Max(Mathf.Abs(scale.y), .001f);
            if ((floor || ceiling) && size.y < thickness)
            {
                size.y = thickness;
                center.y = floor ? bounds.max.y - thickness * .5f : bounds.min.y + thickness * .5f;
            }
            size.x = Mathf.Max(size.x, .04f / Mathf.Max(Mathf.Abs(scale.x), .001f));
            size.y = Mathf.Max(size.y, .04f / Mathf.Max(Mathf.Abs(scale.y), .001f));
            size.z = Mathf.Max(size.z, .04f / Mathf.Max(Mathf.Abs(scale.z), .001f));
            box.center = center;
            box.size = size;
            PrefabUtility.RecordPrefabInstancePropertyModifications(box);
            return box;
        }

        static void Repair(Scene scene)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play mode before repairing.");
            if (scene.path != "Assets/Game/Scenes/Scene1.unity") throw new InvalidOperationException("Open the game's Scene1 before applying this repair.");
            if (scene.isDirty) throw new InvalidOperationException("The scene has unsaved user changes. Save them before applying the repair.");
            Directory.CreateDirectory("Library/CollisionReports");
            var backup = "Library/CollisionReports/Scene1-before-collision-repair.unity";
            if (!File.Exists(backup)) File.Copy(scene.path, backup);
            var changes = new List<string>();
            Undo.IncrementCurrentGroup();
            var group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Repair environment and VR collisions");
            // Scene overrides only: source meshes and vendor prefabs remain intact.
            foreach (var m in All<MeshFilter>(scene).ToArray())
            {
                if (!PathOf(m.transform).StartsWith("Map/") || m.sharedMesh == null || !m.gameObject.activeInHierarchy) continue;
                if (m.GetComponentInParent<Rigidbody>() != null) continue;
                if (m.GetComponentsInParent<Collider>().Any(c => c.enabled && !c.isTrigger)) continue;
                bool floor = m.name.Contains("Floor");
                bool ceiling = m.name.Contains("Ceiling");
                if (m.name.Contains("DoorFrame"))
                {
                    // Preserve the actual doorway; a bounding box would seal it.
                    var c = Undo.AddComponent<MeshCollider>(m.gameObject);
                    c.sharedMesh = m.sharedMesh;
                    c.convex = false;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(c);
                    changes.Add("Added precise doorway collider: " + PathOf(m.transform));
                }
                else
                {
                    AddBox(m.gameObject, m.sharedMesh.bounds, floor, ceiling);
                    changes.Add("Added box: " + PathOf(m.transform));
                }
            }
            foreach (var c in All<MeshCollider>(scene).ToArray())
            {
                if (!PathOf(c.transform).StartsWith("Map/") || !c.name.Contains("Floor") || !c.enabled || c.isTrigger || c.sharedMesh == null || c.attachedRigidbody != null) continue;
                if (c.bounds.size.y >= .06f) continue;
                AddBox(c.gameObject, c.sharedMesh.bounds, true, false);
                Undo.RecordObject(c, "Disable thin floor surface collider");
                c.enabled = false;
                PrefabUtility.RecordPrefabInstancePropertyModifications(c);
                changes.Add("Replaced thin floor surface with solid slab: " + PathOf(c.transform));
            }
            foreach (var origin in All<XROrigin>(scene))
            {
                var obj = origin.Origin != null ? origin.Origin : origin.gameObject;
                var cc = obj.GetComponent<CharacterController>();
                if (cc == null) cc = Undo.AddComponent<CharacterController>(obj);
                Undo.RecordObject(cc, "Configure player collision capsule");
                cc.enabled = true;
                cc.detectCollisions = true;
                cc.radius = .25f;
                cc.skinWidth = .025f;
                cc.minMoveDistance = 0;
                cc.stepOffset = .2f;
                cc.slopeLimit = 45;
                cc.height = Mathf.Max(.5f, origin.CameraInOriginSpaceHeight);
                cc.center = new Vector3(0, cc.height * .5f + cc.skinWidth, 0);
                PrefabUtility.RecordPrefabInstancePropertyModifications(cc);
                foreach (var body in origin.GetComponentsInChildren<XRBodyTransformer>(true))
                {
                    Undo.RecordObject(body, "Use CharacterController for locomotion");
                    body.useCharacterControllerIfExists = true;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(body);
                }
                if (obj.GetComponent<XRHeadCollisionGuard>() == null) Undo.AddComponent<XRHeadCollisionGuard>(obj);
                // The saved rig was high above the roof. Place feet on the real floor below its XZ.
                var floor = All<MeshFilter>(scene).Where(m => PathOf(m.transform).StartsWith("Map/") && m.name.Contains("Floor") && m.GetComponent<Renderer>() != null)
                    .Select(m => m.GetComponent<Renderer>().bounds)
                    .Where(b => obj.transform.position.x >= b.min.x && obj.transform.position.x <= b.max.x && obj.transform.position.z >= b.min.z && obj.transform.position.z <= b.max.z && b.max.y <= obj.transform.position.y + .1f)
                    .OrderByDescending(b => b.max.y).ToArray();
                if (floor.Length > 0 && obj.transform.position.y - floor[0].max.y > 2)
                {
                    Undo.RecordObject(obj.transform, "Place player on walkable floor");
                    var p = obj.transform.position;
                    p.y = floor[0].max.y + .02f;
                    obj.transform.position = p;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(obj.transform);
                    changes.Add("Placed player feet at floor: " + p);
                }
                changes.Add("Configured player capsule, constrained locomotion and room-scale head blackout.");
            }
            ReinforceWalls(scene, changes);
            Physics.SyncTransforms();
            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save repaired scene.");
            File.WriteAllLines("Library/CollisionReports/repair-changes.txt", changes);
        }

        [Serializable] public class Validation
        {
            public int floorTests, wallTests, skippedOpenCenters;
            public List<string> failures = new List<string>();
        }
        static void ReinforceWalls(Scene scene, List<string> changes)
        {
            // These two solid masonry panels have imported hulls that raycast but
            // fail capsule sweeps. Preserve their decorative mesh and reinforce
            // only the central wall body, excluding the projecting roof/eaves.
            foreach (var c in All<MeshCollider>(scene))
            {
                if (c.sharedMesh == null || c.sharedMesh.name != "SM_DG_HG_Wall02D_ConvexHulls" || c.GetComponent<BoxCollider>() != null) continue;
                var renderMesh = c.GetComponentsInChildren<MeshFilter>().First(m => m.sharedMesh != null && m.sharedMesh.name == "SM_DG_HG_Wall02D");
                var mesh = renderMesh.sharedMesh;
                var points = mesh.vertices.Where(v => v.y > mesh.bounds.min.y + .4f && v.y < mesh.bounds.max.y - .7f).ToArray();
                var first = c.transform.InverseTransformPoint(renderMesh.transform.TransformPoint(points[0]));
                var body = new Bounds(first, Vector3.zero);
                foreach (var p in points) body.Encapsulate(c.transform.InverseTransformPoint(renderMesh.transform.TransformPoint(p)));
                body.SetMinMax(new Vector3(body.min.x, c.sharedMesh.bounds.min.y, body.min.z),
                    new Vector3(body.max.x, c.sharedMesh.bounds.max.y - .5f, body.max.z));
                AddBox(c.gameObject, body, false, false);
                changes.Add("Reinforced solid masonry panel: " + PathOf(c.transform));
            }
        }
        static void ValidateHeadGuard()
        {
            var result = new List<string>();
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            var root = new GameObject("Temporary head collision test");
            root.hideFlags = HideFlags.DontSave;
            root.transform.position = new Vector3(10000, 10000, 10000);
            var cameraObject = new GameObject("Test head");
            cameraObject.transform.SetParent(root.transform, false);
            cameraObject.transform.localPosition = Vector3.up * 1.5f;
            var camera = cameraObject.AddComponent<Camera>();
            camera.enabled = false;
            var wall = new GameObject("Temporary test wall");
            wall.hideFlags = HideFlags.DontSave;
            wall.transform.position = root.transform.position + new Vector3(0, 1.5f, 2);
            wall.AddComponent<BoxCollider>().size = new Vector3(4, 3, .1f);
            XRHeadCollisionGuard guard = null;
            try
            {
                var origin = root.AddComponent<XROrigin>();
                origin.Origin = root;
                origin.Camera = camera;
                guard = root.AddComponent<XRHeadCollisionGuard>();
                var type = typeof(XRHeadCollisionGuard);
                type.GetMethod("Awake", flags).Invoke(guard, null);
                var update = type.GetMethod("LateUpdate", flags);
                var image = (UnityEngine.UI.Image)type.GetField("overlay", flags).GetValue(guard);
                Physics.SyncTransforms();
                update.Invoke(guard, null);
                if (image.color.a != 0) throw new Exception("Head outside geometry must remain visible.");
                result.Add("PASS: unobstructed head stays visible.");
                cameraObject.transform.localPosition = new Vector3(0, 1.5f, 2);
                update.Invoke(guard, null);
                if (image.color.a != 1) throw new Exception("Head inside wall must black out.");
                result.Add("PASS: head overlap blacks out.");
                cameraObject.transform.localPosition = new Vector3(0, 1.5f, 3);
                update.Invoke(guard, null);
                if (image.color.a != 1) throw new Exception("Crossing a wall must not uncover the view on its far side.");
                result.Add("PASS: swept head cannot see through the opposite side.");
                cameraObject.transform.localPosition = Vector3.up * 1.5f;
                update.Invoke(guard, null);
                var clear = (Vector3)type.GetField("lastClearPosition", flags).GetValue(guard);
                if (Vector3.Distance(clear, cameraObject.transform.position) > .001f) throw new Exception("Returning to safety must release the blackout.");
                result.Add("PASS: returning to the safe side releases the blackout.");
            }
            finally
            {
                if (guard != null)
                {
                    var image = (UnityEngine.UI.Image)typeof(XRHeadCollisionGuard).GetField("overlay", flags).GetValue(guard);
                    if (image != null) UnityEngine.Object.DestroyImmediate(image.canvas.gameObject);
                }
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(wall);
                Physics.SyncTransforms();
                File.WriteAllLines("Library/CollisionReports/head-validation.txt", result);
            }
        }
        static void Validate(Scene scene)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Run validation outside Play mode.");
            var results = new Validation();
            var targets = All<Collider>(scene).Where(c => PathOf(c.transform).StartsWith("Map/") && c.enabled && !c.isTrigger && c.gameObject.activeInHierarchy && (c.name.Contains("Floor") || c.name.Contains("Wall") || c.name.Contains("Door08E"))
                && !(c is MeshCollider && c.GetComponent<BoxCollider>() != null)).ToArray();
            var testScene = EditorSceneManager.NewPreviewScene();
            try
            {
                foreach (var source in targets)
                {
                    var obj = new GameObject("Test geometry");
                    SceneManager.MoveGameObjectToScene(obj, testScene);
                    obj.transform.SetPositionAndRotation(source.transform.position + new Vector3(10000, 10000, 10000), source.transform.rotation);
                    obj.transform.localScale = source.transform.lossyScale;
                    Collider clone;
                    if (source is BoxCollider box)
                    {
                        var c = obj.AddComponent<BoxCollider>(); c.center = box.center; c.size = box.size; clone = c;
                    }
                    else if (source is MeshCollider mesh)
                    {
                        var c = obj.AddComponent<MeshCollider>(); c.sharedMesh = mesh.sharedMesh; c.convex = mesh.convex; clone = c;
                    }
                    else { UnityEngine.Object.DestroyImmediate(obj); continue; }
                    var player = new GameObject("Test capsule");
                    SceneManager.MoveGameObjectToScene(player, testScene);
                    var cc = player.AddComponent<CharacterController>();
                    cc.height = 1.7f; cc.radius = .25f; cc.center = Vector3.up * .875f; cc.skinWidth = .025f; cc.stepOffset = .2f; cc.minMoveDistance = 0;
                    Physics.SyncTransforms();
                    var b = clone.bounds;
                    if (source.name.Contains("Floor"))
                    {
                        var start = new Vector3(b.center.x, b.max.y + .3f, b.center.z);
                        if (!clone.Raycast(new Ray(start, Vector3.down), out var hit, b.size.y + 1)) results.failures.Add("No floor ray contact: " + PathOf(source.transform));
                        else
                        {
                            cc.enabled = false; player.transform.position = start; cc.enabled = true;
                            Physics.SyncTransforms();
                            for (int i = 0; i < 60; i++) cc.Move(Vector3.down * .1f);
                            if (player.transform.position.y < hit.point.y - .06f) results.failures.Add("Capsule fell through: " + PathOf(source.transform));
                            results.floorTests++;
                        }
                    }
                    else
                    {
                        var axis = b.size.x < b.size.z ? Vector3.right : Vector3.forward;
                        var extent = b.size.x < b.size.z ? b.extents.x : b.extents.z;
                        for (int side = -1; side <= 1; side += 2)
                        {
                            var direction = axis * side;
                            var centerStart = b.center + direction * (extent + .6f);
                            if (!clone.Raycast(new Ray(centerStart, -direction), out _, extent * 2 + 1.2f)) { results.skippedOpenCenters++; continue; }
                            cc.enabled = false; player.transform.position = centerStart - Vector3.up * .875f; cc.enabled = true;
                            Physics.SyncTransforms();
                            int steps = Mathf.CeilToInt((extent * 2 + 1.2f) / .05f);
                            for (int i = 0; i < steps; i++) cc.Move(-direction * .05f);
                            if (Vector3.Dot(player.transform.position - b.center, direction) < 0) results.failures.Add("Capsule crossed wall: " + PathOf(source.transform) + " side=" + side);
                            results.wallTests++;
                        }
                    }
                    UnityEngine.Object.DestroyImmediate(player);
                    UnityEngine.Object.DestroyImmediate(obj);
                }
            }
            finally { EditorSceneManager.ClosePreviewScene(testScene); }
            Directory.CreateDirectory("Library/CollisionReports");
            File.WriteAllText("Library/CollisionReports/validation-results.json", JsonUtility.ToJson(results, true));
            Debug.Log("Collision validation: " + results.floorTests + " floor drops, " + results.wallTests + " wall sweeps, " + results.failures.Count + " failures.");
        }
    }
}
#endif

