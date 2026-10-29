using UnityEngine;

[RequireComponent(typeof(Collider))]
public class QueenHuntTrigger : MonoBehaviour
{
    [SerializeField] QueenController reina;
    [SerializeField] CharacterController jugador;

    void OnTriggerEnter(Collider other)
    {
        if (other == jugador && reina != null)
            reina.enabled = true;
    }
}
