using UnityEngine;
using UnityEngine.XR.Content.Interaction;

public class LeverController : MonoBehaviour
{
    public XRLever palanca;
    public float Speed;

    void Update()
    {
        if (palanca == null) return;

        float speedMove = Speed * (palanca.value ? 1 : 0);

        Vector3 velocity = new Vector3(0, 0, speedMove);
        transform.Translate(velocity * Time.deltaTime, Space.Self);
    }
}