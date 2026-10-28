using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class TagSocket : XRSocketInteractor
{
    public string targetTag;

    public override bool CanHover(IXRHoverInteractable interactable)
    {
        return base.CanHover(interactable) && interactable.transform.CompareTag(targetTag);
    }

    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        return base.CanSelect(interactable) && interactable.transform.CompareTag(targetTag);
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        ActivarPanel(args.interactableObject);
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        DesactivarPanel(args.interactableObject);
    }

    public void ActivarPanel(IXRSelectInteractable interactable) => CambiarPanel(interactable, true);
    public void DesactivarPanel(IXRSelectInteractable interactable) => CambiarPanel(interactable, false);

    void CambiarPanel(IXRSelectInteractable interactable, bool activo)
    {
        var manager = InteractionManager.instance;
        if (manager == null) return;

        switch (interactable.transform.tag)
        {
            case "Pila 1":
            case "Parte1":
                manager.panel1 = activo;
                break;
            case "Pila 2":
                manager.panel2 = activo;
                manager.panel3 = activo;
                break;
            case "Parte2":
                manager.panel2 = activo;
                break;
            case "Parte3":
                manager.panel3 = activo;
                break;
        }
    }
}
