using UnityEngine;

public class HingedExitDoors : MonoBehaviour
{
    [SerializeField] Rigidbody puerta1;
    [SerializeField] Rigidbody puerta2;

    Quaternion rotacion1;
    Quaternion rotacion2;
    Collider[] colisiones1;
    Collider[] colisiones2;
    CharacterController jugador;
    float tiempo;
    bool abriendo;

    void Awake()
    {
        if (puerta1 == null || puerta2 == null)
        {
            enabled = false;
            return;
        }
        rotacion1 = puerta1.rotation;
        rotacion2 = puerta2.rotation;
        colisiones1 = puerta1.GetComponentsInChildren<Collider>();
        colisiones2 = puerta2.GetComponentsInChildren<Collider>();
        jugador = FindAnyObjectByType<CharacterController>();
    }

    public void Abrir()
    {
        if (enabled && tiempo < 1.5f) abriendo = true;
    }

    void FixedUpdate()
    {
        if (!abriendo) return;
        float siguiente = Mathf.Min(tiempo + Time.fixedDeltaTime, 1.5f);
        float progreso = Mathf.SmoothStep(0f, 1f, siguiente / 1.5f);
        Quaternion destino1 = Quaternion.AngleAxis(-90f * progreso, Vector3.up) * rotacion1;
        Quaternion destino2 = Quaternion.AngleAxis(90f * progreso, Vector3.up) * rotacion2;
        if (BloqueaJugador(puerta1, colisiones1, destino1) || BloqueaJugador(puerta2, colisiones2, destino2)) return;
        puerta1.MoveRotation(destino1);
        puerta2.MoveRotation(destino2);
        tiempo = siguiente;
        if (tiempo >= 1.5f) abriendo = false;
    }

    bool BloqueaJugador(Rigidbody puerta, Collider[] colisiones, Quaternion destino)
    {
        if (jugador == null) jugador = FindAnyObjectByType<CharacterController>();
        if (jugador == null || !jugador.enabled || !jugador.gameObject.activeInHierarchy) return false;
        Quaternion giro = destino * Quaternion.Inverse(puerta.rotation);
        foreach (var colision in colisiones)
        {
            if (colision == null || !colision.enabled || colision.isTrigger || !colision.gameObject.activeInHierarchy) continue;
            Vector3 posicion = puerta.position + giro * (colision.transform.position - puerta.position);
            Quaternion rotacion = giro * colision.transform.rotation;
            bool futuro = Physics.ComputePenetration(colision, posicion, rotacion,
                jugador, jugador.transform.position, jugador.transform.rotation, out _, out float profundidadFutura);
            if (!futuro) continue;
            bool actual = Physics.ComputePenetration(colision, colision.transform.position, colision.transform.rotation,
                jugador, jugador.transform.position, jugador.transform.rotation, out _, out float profundidadActual);
            if (!actual || profundidadFutura > profundidadActual + 0.001f) return true;
        }
        return false;
    }
}
