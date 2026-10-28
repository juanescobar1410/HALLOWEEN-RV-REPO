using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class QueenController : MonoBehaviour
{
    [SerializeField] Transform ojosJugador;
    [SerializeField] Transform jugador;
    [SerializeField] Renderer cuerpo;
    [SerializeField] AltarController altar;
    [SerializeField] GameObject avisoCaptura;
    [SerializeField] float distanciaVision = 12f;
    [SerializeField] float anguloVision = 70f;
    [SerializeField] float tiempoSinMirar = 0.8f;
    [SerializeField] float velocidad = 1.5f;
    [SerializeField] float distanciaCaptura = 1.2f;
    [SerializeField] float tiempoCaptura = 1f;

    readonly RaycastHit[] impactos = new RaycastHit[64];
    NavMeshPath ruta;
    NavMeshAgent agente;
    float sinMirar;
    float cerca;
    float siguienteRuta;
    public bool Capturado { get; private set; }

    void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
        ruta = new NavMeshPath();
        if (ojosJugador == null || jugador == null || cuerpo == null || altar == null || avisoCaptura == null)
        {
            Debug.LogError("Faltan referencias para la Reina.", this);
            enabled = false;
            return;
        }
        agente.speed = velocidad;
        avisoCaptura.SetActive(false);
    }

    void Update()
    {
        if (Capturado || altar.OfrendaConfirmada || !agente.isOnNavMesh) return;

        Bounds limites = cuerpo.bounds;
        Vector3 torso = limites.center;
        Vector3 cabeza = limites.center + Vector3.up * limites.extents.y * 0.65f;
        bool observada = PuntoObservado(torso) || PuntoObservado(cabeza);
        if (observada)
        {
            sinMirar = 0f;
            agente.isStopped = true;
        }
        else
        {
            sinMirar += Time.deltaTime;
            if (sinMirar >= tiempoSinMirar && Time.time >= siguienteRuta)
            {
                siguienteRuta = Time.time + 0.3f;
                Avanzar();
            }
        }

        Vector3 diferencia = ojosJugador.position - transform.position;
        float distanciaHorizontal = new Vector2(diferencia.x, diferencia.z).magnitude;
        bool cercaYAlcanzable = distanciaHorizontal <= distanciaCaptura && Mathf.Abs(diferencia.y) < 2.5f &&
            LineaDespejada(torso, ojosJugador.position, true);
        cerca = cercaYAlcanzable ? cerca + Time.deltaTime : 0f;
        if (cerca >= tiempoCaptura && !altar.OfrendaConfirmada)
        {
            Capturado = true;
            agente.ResetPath();
            agente.isStopped = true;
            altar.enabled = false;
            avisoCaptura.SetActive(true);
        }
    }

    bool PuntoObservado(Vector3 punto)
    {
        Vector3 diferencia = punto - ojosJugador.position;
        float distancia = diferencia.magnitude;
        if (distancia > distanciaVision || distancia < 0.001f) return false;
        float limite = Mathf.Cos(anguloVision * 0.5f * Mathf.Deg2Rad);
        return Vector3.Dot(ojosJugador.forward, diferencia / distancia) >= limite &&
            LineaDespejada(ojosJugador.position, punto, false);
    }

    bool LineaDespejada(Vector3 origen, Vector3 destino, bool ignorarPuertas)
    {
        Vector3 diferencia = destino - origen;
        float distancia = diferencia.magnitude;
        if (distancia < 0.001f) return true;
        int cantidad = Physics.RaycastNonAlloc(origen, diferencia / distancia, impactos, distancia, ~0, QueryTriggerInteraction.Ignore);
        if (cantidad == impactos.Length) return false;
        for (int i = 0; i < cantidad; i++)
        {
            Transform objeto = impactos[i].collider.transform;
            if (objeto.IsChildOf(transform) || objeto.IsChildOf(jugador)) continue;
            if (ignorarPuertas && EsPuerta(objeto)) continue;
            return false;
        }
        return true;
    }

    static bool EsPuerta(Transform objeto)
    {
        while (objeto != null)
        {
            if (objeto.name.IndexOf("door", StringComparison.OrdinalIgnoreCase) >= 0 ||
                objeto.name.IndexOf("puerta", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            objeto = objeto.parent;
        }
        return false;
    }

    void Avanzar()
    {
        var filtro = new NavMeshQueryFilter { agentTypeID = agente.agentTypeID, areaMask = agente.areaMask };
        if (!NavMesh.SamplePosition(ojosJugador.position, out NavMeshHit destino, 2.5f, filtro) ||
            new Vector2(destino.position.x - ojosJugador.position.x, destino.position.z - ojosJugador.position.z).magnitude > 0.45f ||
            Mathf.Abs(destino.position.y - ojosJugador.position.y) > 2.5f ||
            !agente.CalculatePath(destino.position, ruta) || ruta.status != NavMeshPathStatus.PathComplete || !agente.SetPath(ruta))
        {
            agente.ResetPath();
            agente.isStopped = true;
            return;
        }
        agente.isStopped = false;
    }
}


