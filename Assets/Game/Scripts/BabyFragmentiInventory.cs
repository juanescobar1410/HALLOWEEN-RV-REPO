
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public enum BabyFragmentType
{
    Base,
    Torso,
    Cabeza
}

public class BabyFragmentInventory : MonoBehaviour
{
    [Header("Panel del inventario")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private InputActionReference toggleInventoryAction;

    [Header("Iconos")]
    [SerializeField] private Image baseIcon;
    [SerializeField] private Image torsoIcon;
    [SerializeField] private Image cabezaIcon;

    [Header("Fragmentos originales")]
    [SerializeField] private BabyFragmentPickup fragmentoBase;
    [SerializeField] private BabyFragmentPickup fragmentoTorso;
    [SerializeField] private BabyFragmentPickup fragmentoCabeza;

    [Header("Puntos de reaparicion en el altar")]
    [SerializeField] private Transform puntoBase;
    [SerializeField] private Transform puntoTorso;
    [SerializeField] private Transform puntoCabeza;

    [Header("Colores del inventario")]
    [SerializeField] private Color colorEncontrado = Color.white;
    [SerializeField]
    private Color colorPendiente =
        new Color(0.25f, 0.25f, 0.25f, 0.65f);

    private bool baseEncontrada;
    private bool torsoEncontrado;
    private bool cabezaEncontrada;

    private bool baseRecuperada;
    private bool torsoRecuperado;
    private bool cabezaRecuperada;

    private void Awake()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        ActualizarIconos();
    }

    private void OnEnable()
    {
        if (toggleInventoryAction != null &&
            toggleInventoryAction.action != null)
        {
            toggleInventoryAction.action.performed +=
                AlternarInventario;

            toggleInventoryAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (toggleInventoryAction != null &&
            toggleInventoryAction.action != null)
        {
            toggleInventoryAction.action.performed -=
                AlternarInventario;
        }
    }

    private void AlternarInventario(
        InputAction.CallbackContext context)
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(
                !inventoryPanel.activeSelf);
        }
    }

    public void RegistrarFragmento(BabyFragmentType tipo)
    {
        switch (tipo)
        {
            case BabyFragmentType.Base:
                baseEncontrada = true;
                break;

            case BabyFragmentType.Torso:
                torsoEncontrado = true;
                break;

            case BabyFragmentType.Cabeza:
                cabezaEncontrada = true;
                break;
        }

        ActualizarIconos();
    }

    public void RecuperarFragmentosEnAltar()
    {
        RecuperarUno(
            fragmentoBase, puntoBase,
            baseEncontrada, ref baseRecuperada);

        RecuperarUno(
            fragmentoTorso, puntoTorso,
            torsoEncontrado, ref torsoRecuperado);

        RecuperarUno(
            fragmentoCabeza, puntoCabeza,
            cabezaEncontrada, ref cabezaRecuperada);
    }

    private void RecuperarUno(
        BabyFragmentPickup fragmento,
        Transform punto,
        bool encontrado,
        ref bool recuperado)
    {
        if (!encontrado || recuperado)
            return;

        if (fragmento == null || punto == null)
        {
            Debug.LogWarning(
                "Falta asignar el fragmento o su punto del altar.");
            return;
        }

        fragmento.ReaparecerEnAltar(punto);
        recuperado = true;
    }

    private void ActualizarIconos()
    {
        CambiarColor(baseIcon, baseEncontrada);
        CambiarColor(torsoIcon, torsoEncontrado);
        CambiarColor(cabezaIcon, cabezaEncontrada);
    }

    private void CambiarColor(Image icono, bool encontrado)
    {
        if (icono != null)
        {
            icono.color = encontrado
                ? colorEncontrado
                : colorPendiente;
        }
    }
}
