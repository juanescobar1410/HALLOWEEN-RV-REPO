
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class RandomHorrorSoundTrigger : MonoBehaviour
{
    [Header("Jugador")]
    [SerializeField] private Transform jugador;

    [Header("Sonidos aleatorios")]
    [SerializeField] private AudioSource fuenteAudio;
    [SerializeField] private AudioClip[] sonidos;

    [Header("Comportamiento")]
    [SerializeField] private bool activarSoloUnaVez = true;
    [SerializeField] private bool evitarRepetirMismoSonido = true;
    [SerializeField] private bool esperarSiYaHayAudio = true;
    [SerializeField] private float tiempoEntreActivaciones = 4f;

    private bool yaActivado;
    private int ultimoSonido = -1;
    private float siguienteActivacion;

    private void Reset()
    {
        // Al añadir el componente, configurar el collider como Trigger.
        BoxCollider box = GetComponent<BoxCollider>();
        box.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!EsJugador(other))
            return;

        if (activarSoloUnaVez && yaActivado)
            return;

        if (Time.time < siguienteActivacion)
            return;

        if (fuenteAudio == null)
        {
            Debug.LogWarning(
                "Falta asignar el AudioSource en " + name, this);
            return;
        }

        if (sonidos == null || sonidos.Length == 0)
        {
            Debug.LogWarning(
                "No hay sonidos asignados en " + name, this);
            return;
        }

        if (esperarSiYaHayAudio && fuenteAudio.isPlaying)
            return;

        int indice = ElegirSonido();

        if (indice < 0)
            return;

        fuenteAudio.PlayOneShot(sonidos[indice]);

        ultimoSonido = indice;
        siguienteActivacion =
            Time.time + Mathf.Max(0f, tiempoEntreActivaciones);

        if (activarSoloUnaVez)
            yaActivado = true;
    }

    private bool EsJugador(Collider otro)
    {
        if (jugador == null)
            return false;

        Transform objeto = otro.transform;

        // Aceptar el collider del jugador o cualquiera de sus hijos.
        return objeto == jugador || objeto.IsChildOf(jugador);
    }

    private int ElegirSonido()
    {
        int cantidad = sonidos.Length;

        if (cantidad == 0)
            return -1;

        int indice = Random.Range(0, cantidad);

        // Si hay varios sonidos, evitar repetir el anterior.
        if (evitarRepetirMismoSonido &&
            cantidad > 1 &&
            indice == ultimoSonido)
        {
            indice = (indice + Random.Range(1, cantidad)) % cantidad;
        }

        return sonidos[indice] != null ? indice : BuscarSonidoValido(indice);
    }

    private int BuscarSonidoValido(int inicio)
    {
        for (int i = 0; i < sonidos.Length; i++)
        {
            int indice = (inicio + i) % sonidos.Length;

            if (sonidos[indice] != null)
                return indice;
        }

        Debug.LogWarning(
            "Todos los clips están vacíos en " + name, this);

        return -1;
    }

    private void OnDrawGizmosSelected()
    {
        BoxCollider box = GetComponent<BoxCollider>();

        if (box == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(box.center, box.size);
    }
}
