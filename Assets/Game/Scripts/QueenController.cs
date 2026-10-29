
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

[RequireComponent(typeof(NavMeshAgent))]
public class QueenController : MonoBehaviour
{
    [SerializeField] Transform ojosJugador;
    [SerializeField] Transform jugador;
    [SerializeField] Renderer cuerpo;
    [SerializeField] Animator animador;
    [SerializeField] Transform manoDerecha;
    [SerializeField] AltarController altar;
    [SerializeField] GameObject avisoCaptura;
    [SerializeField] float distanciaVision = 12f;
    [SerializeField] float anguloVision = 70f;
    [SerializeField] float tiempoSinMirar = 0.8f;
    [SerializeField] float velocidad = 1.5f;
    [SerializeField] float distanciaCaptura = 1.2f;
    [SerializeField] float tiempoCaptura = 1f;

    // =========================================================
    // NUEVO: SONIDO DE PROXIMIDAD
    // =========================================================

    [Header("Sonido de proximidad")]
    [SerializeField] AudioSource audioSusurro;
    [SerializeField] AudioClip clipSusurro;
    [SerializeField, Range(0f, 1f)] float volumenSusurro = 0.4f;
    [SerializeField] float distanciaSusurro = 8f;
    [SerializeField] float intervaloSusurroMin = 3.5f;
    [SerializeField] float intervaloSusurroMax = 5f;

    // =========================================================
    // NUEVO: SONIDO CUANDO EL JUGADOR VE A LA REINA
    // =========================================================

    [Header("Sonido al ser observada")]
    [SerializeField] AudioSource audioAlSerObservada;
    [SerializeField] AudioClip clipAlSerObservada;
    [SerializeField, Range(0f, 1f)] float volumenAlSerObservada = 0.7f;
    [SerializeField] float intervaloSonidoObservada = 2f;

    // Variables nuevas para controlar los sonidos.
    bool observadaAnteriormente;
    float siguienteSusurro;
    float siguienteSonidoObservada;

    readonly RaycastHit[] impactos = new RaycastHit[64];
    NavMeshPath ruta;
    NavMeshAgent agente;
    readonly List<LocomotionProvider> locomocionDesactivada = new List<LocomotionProvider>();
    bool sujetando;
    float inicioAgarre;
    Vector3 posicionInicialOjos;
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
        if (Capturado || altar.OfrendaConfirmada)
        {
            // NUEVO: detener los efectos cuando termina el encuentro.
            DetenerSonidos();
            return;
        }

        if (!agente.isOnNavMesh)
        {
            if (animador != null) animador.SetBool("Caminando", false);
            return;
        }

        Bounds limites = cuerpo.bounds;
        Vector3 torso = limites.center;
        Vector3 cabeza = limites.center + Vector3.up * limites.extents.y * 0.65f;
        bool observada = PuntoObservado(torso) || PuntoObservado(cabeza);

        // =====================================================
        // NUEVO: DETECTAR CUANDO EL JUGADOR EMPIEZA A MIRARLA
        // =====================================================

        if (observada && !observadaAnteriormente)
        {
            // Interrumpir el susurro si estaba reproduciéndose.
            if (audioSusurro != null && audioSusurro.isPlaying)
                audioSusurro.Stop();

            // Reproducir una vez el sonido de descubrimiento.
            if (audioAlSerObservada != null &&
                clipAlSerObservada != null &&
                !audioAlSerObservada.isPlaying &&
                Time.time >= siguienteSonidoObservada)
            {
                audioAlSerObservada.PlayOneShot(
                    clipAlSerObservada,
                    volumenAlSerObservada
                );

                siguienteSonidoObservada =
                    Time.time + intervaloSonidoObservada;
            }
        }

        // Guardar el estado actual para detectar el siguiente cambio.
        observadaAnteriormente = observada;

        // =====================================================
        // COMPORTAMIENTO ORIGINAL DE LA REINA
        // =====================================================

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

        // =====================================================
        // NUEVO: GESTIONAR EL SUSURRO DE PROXIMIDAD
        // =====================================================

        GestionarSusurroProximidad(observada);

        // =====================================================
        // ANIMACIÓN ORIGINAL
        // =====================================================

        if (animador != null) animador.SetBool("Caminando", !agente.isStopped && agente.velocity.sqrMagnitude > 0.01f);

        // =====================================================
        // LÓGICA ORIGINAL DE CAPTURA
        // =====================================================

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
            DetenerSonidos();

            if (animador != null && animador.runtimeAnimatorController != null && manoDerecha != null)
                StartCoroutine(AnimarCaptura());
            else
                avisoCaptura.SetActive(true);
        }
    }

    // =========================================================
    // NUEVO: SISTEMA DE SUSURROS DE PROXIMIDAD
    // =========================================================

    void GestionarSusurroProximidad(bool observada)
    {
        if (audioSusurro == null || clipSusurro == null)
            return;

        // No susurrar mientras el jugador mira a la Reina,
        // después de la captura o al completar la ofrenda.
        if (observada || Capturado || altar.OfrendaConfirmada)
        {
            if (audioSusurro.isPlaying)
                audioSusurro.Stop();

            return;
        }

        // Calcular la distancia horizontal al jugador.
        Vector3 diferencia = ojosJugador.position - transform.position;

        float distanciaHorizontal =
            new Vector2(diferencia.x, diferencia.z).magnitude;

        // Evitar avisos cuando hay un desnivel excesivo.
        if (Mathf.Abs(diferencia.y) > 2.5f)
        {
            if (audioSusurro.isPlaying)
                audioSusurro.Stop();

            return;
        }

        // Reproducir el aviso únicamente dentro del rango.
        if (distanciaHorizontal > distanciaSusurro)
        {
            if (audioSusurro.isPlaying)
                audioSusurro.Stop();

            return;
        }

        // No repetir el sonido mientras se reproduce
        // ni antes de que termine el intervalo configurado.
        if (audioSusurro.isPlaying ||
            Time.time < siguienteSusurro)
        {
            return;
        }

        audioSusurro.PlayOneShot(
            clipSusurro,
            volumenSusurro
        );

        float intervaloMin = Mathf.Min(
            intervaloSusurroMin,
            intervaloSusurroMax
        );

        float intervaloMax = Mathf.Max(
            intervaloSusurroMin,
            intervaloSusurroMax
        );

        siguienteSusurro = Time.time +
            UnityEngine.Random.Range(intervaloMin, intervaloMax);
    }

    // NUEVO: detener los sonidos de la Reina.
    void DetenerSonidos()
    {
        if (audioSusurro != null && audioSusurro.isPlaying)
            audioSusurro.Stop();

        if (audioAlSerObservada != null &&
            audioAlSerObservada.isPlaying)
            audioAlSerObservada.Stop();
    }

    IEnumerator AnimarCaptura()
    {
        foreach (var proveedor in jugador.GetComponentsInChildren<LocomotionProvider>(true))
        {
            if (!proveedor.enabled) continue;
            proveedor.enabled = false;
            locomocionDesactivada.Add(proveedor);
        }

        Vector3 direccion = ojosJugador.position - transform.position;
        direccion.y = 0f;
        if (direccion.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(direccion);
        animador.SetBool("Caminando", false);
        animador.SetTrigger("Agarrar");

        float espera = 0f;
        while (!animador.GetCurrentAnimatorStateInfo(0).IsName("Grip") && espera < 1f)
        {
            espera += Time.deltaTime;
            yield return null;
        }
        if (!animador.GetCurrentAnimatorStateInfo(0).IsName("Grip"))
        {
            avisoCaptura.SetActive(true);
            yield break;
        }
        while (animador.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.2f)
            yield return null;

        posicionInicialOjos = ojosJugador.position;
        inicioAgarre = Time.time;
        sujetando = true;
        while (animador.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.85f)
            yield return null;
        sujetando = false;
        while (animador.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.98f)
            yield return null;
        avisoCaptura.SetActive(true);
    }

    void LateUpdate()
    {
        if (!sujetando || manoDerecha == null || ojosJugador == null || jugador == null) return;
        float progreso = Mathf.SmoothStep(0f, 1f, (Time.time - inicioAgarre) / 0.35f);
        Vector3 destino = Vector3.Lerp(posicionInicialOjos, manoDerecha.position + Vector3.up * 0.15f, progreso);
        jugador.position += destino - ojosJugador.position;
    }

    void OnDisable()
    {
        StopAllCoroutines();

        // NUEVO: evitar que los efectos sigan sonando
        // si se desactiva el controlador.
        DetenerSonidos();

        sujetando = false;
        foreach (var proveedor in locomocionDesactivada)
            if (proveedor != null) proveedor.enabled = true;
        locomocionDesactivada.Clear();
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
            if (objeto.name.StartsWith("SM_DG_Door08", StringComparison.OrdinalIgnoreCase)) return true;
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
