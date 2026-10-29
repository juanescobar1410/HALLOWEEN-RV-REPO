using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class GrabPorMano : XRGrabInteractable
{
    [Header("Attach por mano")]
    [SerializeField] Transform attachIzquierda;
    [SerializeField] Transform attachDerecha;

    public override Transform GetAttachTransform(IXRInteractor interactor)
    {
        if (interactor != null)
        {
            if (interactor.handedness == InteractorHandedness.Left && attachIzquierda != null)
                return attachIzquierda;
            if (interactor.handedness == InteractorHandedness.Right && attachDerecha != null)
                return attachDerecha;
        }
        return base.GetAttachTransform(interactor);
    }
}