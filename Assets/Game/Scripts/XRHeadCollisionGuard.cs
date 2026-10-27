using UnityEngine;
using UnityEngine.UI;
using Unity.XR.CoreUtils;

namespace Daejojeon.VR
{
    /// <summary>
    /// Room-scale tracking cannot be blocked physically. Hide the view when the
    /// headset enters/crosses solid geometry, without moving the tracked camera.
    /// Stick locomotion remains the responsibility of XRI's CharacterController.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class XRHeadCollisionGuard : MonoBehaviour
    {
        [SerializeField] XROrigin xrOrigin;
        [SerializeField, Min(.01f)] float headRadius = .08f;
        [SerializeField] LayerMask obstacleLayers = ~0;
        readonly Collider[] overlaps = new Collider[64];
        readonly RaycastHit[] hits = new RaycastHit[64];
        Image overlay;
        Vector3 lastClearPosition;
        bool hasClearPosition;

        void Awake()
        {
            if (xrOrigin == null) xrOrigin = GetComponent<XROrigin>();
            if (xrOrigin == null || xrOrigin.Camera == null) { enabled = false; return; }
            var panel = new GameObject("Head collision fade", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup));
            panel.layer = 2; // Ignore Raycast; this view never participates in interaction.
            panel.transform.SetParent(xrOrigin.Camera.transform, false);
            panel.transform.localPosition = new Vector3(0, 0, xrOrigin.Camera.nearClipPlane + .02f);
            var canvas = panel.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = xrOrigin.Camera;
            canvas.overrideSorting = true;
            canvas.sortingOrder = short.MaxValue;
            var group = panel.GetComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;
            var rect = panel.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(4, 4);
            var imageObject = new GameObject("Blackout", typeof(RectTransform), typeof(Image));
            imageObject.layer = 2;
            imageObject.transform.SetParent(panel.transform, false);
            overlay = imageObject.GetComponent<Image>();
            overlay.raycastTarget = false;
            var imageRect = imageObject.GetComponent<RectTransform>();
            imageRect.anchorMin = Vector2.zero;
            imageRect.anchorMax = Vector2.one;
            imageRect.offsetMin = imageRect.offsetMax = Vector2.zero;
            overlay.color = Color.clear;
        }

        bool IsObstacle(Collider c)
        {
            return c != null && !c.isTrigger && !c.transform.IsChildOf(xrOrigin.transform)
                && !Physics.GetIgnoreLayerCollision(gameObject.layer, c.gameObject.layer);
        }

        void LateUpdate()
        {
            if (overlay == null) return;
            var head = xrOrigin.Camera.transform.position;
            bool blocked = false;
            int count = Physics.OverlapSphereNonAlloc(head, headRadius, overlaps, obstacleLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++) if (IsObstacle(overlaps[i])) { blocked = true; break; }
            if (!blocked && hasClearPosition)
            {
                var delta = head - lastClearPosition;
                if (delta.sqrMagnitude > .000001f)
                {
                    count = Physics.SphereCastNonAlloc(lastClearPosition, headRadius, delta.normalized, hits, delta.magnitude, obstacleLayers, QueryTriggerInteraction.Ignore);
                    for (int i = 0; i < count; i++) if (IsObstacle(hits[i].collider)) { blocked = true; break; }
                }
            }
            // Black out immediately on penetration; uncover gradually when safe.
            var alpha = blocked ? 1 : Mathf.MoveTowards(overlay.color.a, 0, Time.unscaledDeltaTime * 6);
            overlay.color = new Color(0, 0, 0, alpha);
            if (!blocked) { lastClearPosition = head; hasClearPosition = true; }
        }

        void OnDisable()
        {
            if (overlay != null) overlay.color = Color.clear;
            hasClearPosition = false;
        }

        void OnDestroy()
        {
            if (overlay != null) Destroy(overlay.canvas.gameObject);
        }
    }
}
