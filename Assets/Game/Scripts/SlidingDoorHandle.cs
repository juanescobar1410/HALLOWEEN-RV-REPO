using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Daejojeon.VR
{
    /// <summary>Direct VR grab input; the actual panel motion is managed by SlidingDoorGroup.</summary>
    [DisallowMultipleComponent]
    public sealed class SlidingDoorHandle : XRBaseInteractable
    {
        [SerializeField] SlidingDoorGroup door;
        IXRSelectInteractor hand;

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            if (door == null) return;
            var candidate = args.interactorObject;
            var attach = candidate.GetAttachTransform(this);
            if (attach != null && door.BeginDrag(this, attach.position)) hand = candidate;
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            if (hand == args.interactorObject)
            {
                if (door != null) door.EndDrag(this);
                hand = null;
            }
            base.OnSelectExited(args);
        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase phase)
        {
            base.ProcessInteractable(phase);
            if (phase != XRInteractionUpdateOrder.UpdatePhase.Dynamic || hand == null || door == null) return;
            var attach = hand.GetAttachTransform(this);
            if (attach != null) door.ContinueDrag(this, attach.position);
        }

        protected override void OnDisable()
        {
            if (door != null) door.EndDrag(this);
            hand = null;
            base.OnDisable();
        }
    }
}
