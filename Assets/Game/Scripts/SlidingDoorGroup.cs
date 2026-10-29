using UnityEngine;

namespace Daejojeon.VR
{
    /// <summary>A pair of kinematic sliding panels. A short drag completes a full opening or closing.</summary>
    [DisallowMultipleComponent]
    public sealed class SlidingDoorGroup : MonoBehaviour
    {
        [SerializeField] Transform[] panels;
        [SerializeField] Vector3[] openOffsets;
        [SerializeField, Min(.01f)] float pullDistance = .07f;
        [SerializeField, Min(.1f)] float duration = .9f;

        Rigidbody[] bodies;
        Collider[][] panelColliders;
        Vector3[] closedPositions;
        CharacterController player;
        SlidingDoorHandle activeHandle;
        Vector3 grabPosition;
        Vector3 dragAxis;
        float openness;
        float startOpenness;
        float goalOpenness;
        float elapsed;
        bool moving;

        public bool IsOpen => !moving && openness >= .999f;
        public bool IsMoving => moving;
        public bool IsLocked { get; set; }

        void Awake() => Initialize();

        void Initialize()
        {
            if (bodies != null) return;
            if (panels == null || openOffsets == null || panels.Length == 0 || panels.Length != openOffsets.Length)
            {
                Debug.LogError("Sliding door requires matching panels and open offsets.", this);
                enabled = false;
                return;
            }
            bodies = new Rigidbody[panels.Length];
            panelColliders = new Collider[panels.Length][];
            closedPositions = new Vector3[panels.Length];
            for (int i = 0; i < panels.Length; i++)
            {
                if (panels[i] == null || !panels[i].TryGetComponent(out bodies[i]) || !bodies[i].isKinematic)
                {
                    Debug.LogError("Each sliding panel requires a kinematic Rigidbody.", this);
                    enabled = false;
                    return;
                }
                closedPositions[i] = panels[i].localPosition;
                panelColliders[i] = panels[i].GetComponentsInChildren<Collider>();
            }
            player = FindFirstObjectByType<CharacterController>();
        }

        public bool BeginDrag(SlidingDoorHandle handle, Vector3 handPosition)
        {
            Initialize();
            if (!enabled || IsLocked || moving || activeHandle != null) return false;
            var index = System.Array.IndexOf(panels, handle.transform);
            if (index < 0 || openOffsets[index].sqrMagnitude < .000001f) return false;
            var parent = panels[index].parent;
            var worldOffset = parent != null ? parent.TransformVector(openOffsets[index]) : openOffsets[index];
            dragAxis = worldOffset.normalized * (IsOpen ? -1f : 1f);
            grabPosition = handPosition;
            activeHandle = handle;
            return true;
        }

        public void ContinueDrag(SlidingDoorHandle handle, Vector3 handPosition)
        {
            if (activeHandle != handle || IsLocked || moving) return;
            if (Vector3.Dot(handPosition - grabPosition, dragAxis) < pullDistance) return;
            startOpenness = openness;
            goalOpenness = IsOpen ? 0f : 1f;
            elapsed = 0f;
            moving = true;
            activeHandle = null;
        }

        public void Open()
        {
            Initialize();
            if (!enabled || IsLocked || moving || IsOpen) return;
            activeHandle = null;
            startOpenness = openness;
            goalOpenness = 1f;
            elapsed = 0f;
            moving = true;
        }
        public void EndDrag(SlidingDoorHandle handle)
        {
            if (activeHandle == handle) activeHandle = null;
        }

        void FixedUpdate() => Step(Time.fixedDeltaTime);

        // Public for the editor's isolated physics validation and future gameplay systems.
        public void Step(float deltaTime)
        {
            if (!moving || deltaTime <= 0f) return;
            float nextElapsed = Mathf.Min(elapsed + deltaTime, duration);
            float t = Mathf.SmoothStep(0f, 1f, nextElapsed / duration);
            float nextOpenness = Mathf.Lerp(startOpenness, goalOpenness, t);
            if (WouldHitPlayer(nextOpenness)) return;
            elapsed = nextElapsed;
            openness = nextOpenness;
            for (int i = 0; i < panels.Length; i++)
            {
                var local = closedPositions[i] + openOffsets[i] * openness;
                var world = panels[i].parent != null ? panels[i].parent.TransformPoint(local) : local;
                bodies[i].MovePosition(world);
            }
            if (elapsed >= duration) { openness = goalOpenness; moving = false; }
        }

        bool WouldHitPlayer(float nextOpenness)
        {
            if (player == null) player = FindFirstObjectByType<CharacterController>();
            if (player == null || !player.enabled || !player.gameObject.activeInHierarchy) return false;
            for (int i = 0; i < panels.Length; i++)
            {
                var parent = panels[i].parent;
                var delta = openOffsets[i] * (nextOpenness - openness);
                var worldDelta = parent != null ? parent.TransformVector(delta) : delta;
                foreach (var c in panelColliders[i])
                {
                    if (c == null || !c.enabled || c.isTrigger || !c.gameObject.activeInHierarchy) continue;
                    bool future = Physics.ComputePenetration(c, c.transform.position + worldDelta, c.transform.rotation,
                        player, player.transform.position, player.transform.rotation, out _, out float futureDepth);
                    if (!future) continue;
                    bool current = Physics.ComputePenetration(c, c.transform.position, c.transform.rotation,
                        player, player.transform.position, player.transform.rotation, out _, out float currentDepth);
                    if (!current || futureDepth > currentDepth + .001f) return true;
                }
            }
            return false;
        }
    }
}

