
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class BabyFragmentPickup : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private BabyFragmentInventory inventario;
    [SerializeField] private BabyFragmentType tipo;

    private XRGrabInteractable objetoAgarrable;
    private Rigidbody rb;

    private bool encontrado;
    private bool guardarAlSoltar;

    private void Awake()
    {
        objetoAgarrable =
            GetComponent<XRGrabInteractable>();

        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        objetoAgarrable.selectEntered.AddListener(
            AlAgarrar);

        objetoAgarrable.selectExited.AddListener(
            AlSoltar);
    }

    private void OnDisable()
    {
        if (objetoAgarrable != null)
        {
            objetoAgarrable.selectEntered.RemoveListener(
                AlAgarrar);

            objetoAgarrable.selectExited.RemoveListener(
                AlSoltar);
        }
    }

    private void AlAgarrar(SelectEnterEventArgs args)
    {
        // Si ya fue encontrado, no volver a guardarlo.
        if (encontrado)
            return;

        if (inventario == null)
        {
            Debug.LogError(
                "El fragmento no tiene inventario asignado.",
                this);
            return;
        }

        encontrado = true;
        guardarAlSoltar = true;

        inventario.RegistrarFragmento(tipo);
    }

    private void AlSoltar(SelectExitEventArgs args)
    {
        if (guardarAlSoltar)
            StartCoroutine(GuardarDespuesDeSoltar());
    }

    private IEnumerator GuardarDespuesDeSoltar()
    {
        // Esperar a que termine el evento de interaccion.
        yield return null;

        if (objetoAgarrable.isSelected)
            yield break;

        guardarAlSoltar = false;

        // El objeto se conserva, pero sale del escenario.
        gameObject.SetActive(false);
    }

    public void ReaparecerEnAltar(Transform punto)
    {
        if (punto == null)
            return;

        // Marcarlo antes de activarlo para evitar
        // que vuelva a guardarse al soltarlo.
        encontrado = true;
        guardarAlSoltar = false;

        transform.SetPositionAndRotation(
            punto.position,
            punto.rotation);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        gameObject.SetActive(true);

        if (rb != null)
            rb.WakeUp();
    }
}
