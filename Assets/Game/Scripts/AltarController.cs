using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;


public class AltarController : MonoBehaviour, IXRSelectFilter
{
    [SerializeField] TagSocket[] sockets;
    [SerializeField] GameObject reina;
    [SerializeField] HingedExitDoors salida;
    [SerializeField] Light luz;
    [SerializeField] AudioSource sonido;

    float tiempoCompleto;
    public bool OfrendaConfirmada { get; private set; }
    public bool canProcess => isActiveAndEnabled;

    void Awake()
    {

        if (luz != null) luz.enabled = false;
    }

    void OnEnable()
    {
        if (sockets == null) return;
        foreach (var socket in sockets)
            if (socket != null) socket.selectExited.AddListener(AlSalirDelSocket);
    }

    void OnDisable()
    {
        if (sockets == null) return;
        foreach (var socket in sockets)
            if (socket != null) socket.selectExited.RemoveListener(AlSalirDelSocket);
    }

    void AlSalirDelSocket(SelectExitEventArgs args)
    {
        if (!OfrendaConfirmada) tiempoCompleto = 0f;
    }

    void Update()
    {
        if (OfrendaConfirmada) return;
        if (!TodasLasPiezasColocadas())
        {
            tiempoCompleto = 0f;
            return;
        }

        tiempoCompleto += Time.deltaTime;
        if (tiempoCompleto < 2f) return;

        OfrendaConfirmada = true;
        foreach (var socket in sockets)
            if (socket.interactablesSelected[0] is XRBaseInteractable pieza)
                pieza.selectFilters.Add(this);
        if (reina != null) reina.SetActive(false);
        if (salida != null) salida.Abrir();
        if (luz != null) StartCoroutine(EncenderLuzGradualmente());
        if (sonido != null && sonido.clip != null) sonido.Play();
    }

    IEnumerator EncenderLuzGradualmente()
    {
        float intensidadFinal = luz.intensity;
        float tiempo = 0f;
        const float duracion = 2f;

        luz.intensity = 0f;
        luz.enabled = true;
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            luz.intensity = Mathf.Lerp(0f, intensidadFinal, tiempo / duracion);
            yield return null;
        }
        luz.intensity = intensidadFinal;
    }
    bool TodasLasPiezasColocadas()
    {
        if (sockets == null || sockets.Length != 3) return false;
        foreach (var socket in sockets)
        {
            if (socket == null || !socket.isActiveAndEnabled || !socket.socketActive || socket.interactablesSelected.Count != 1)
                return false;
            var pieza = socket.interactablesSelected[0];
            if (pieza == null || !pieza.transform.CompareTag(socket.targetTag) ||
                pieza.interactorsSelecting.Count != 1 || !ReferenceEquals(pieza.interactorsSelecting[0], socket))
                return false;
        }
        return sockets[0] != sockets[1] && sockets[0] != sockets[2] && sockets[1] != sockets[2];
    }

    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
    {
        return !OfrendaConfirmada || (interactable.interactorsSelecting.Count == 1 &&
            interactable.interactorsSelecting[0] == interactor);
    }
}



