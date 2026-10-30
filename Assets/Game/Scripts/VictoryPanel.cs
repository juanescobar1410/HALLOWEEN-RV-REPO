using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using Unity.XR.CoreUtils;
using Daejojeon.VR;

public class VictoryPanel : MonoBehaviour
{
    [SerializeField] CanvasGroup fondo;
    [SerializeField] GameObject panel;
    [SerializeField] XROrigin jugador;

    public IEnumerator Mostrar()
    {
        foreach (var locomocion in jugador.GetComponentsInChildren<LocomotionProvider>(true))
            locomocion.enabled = false;
        var guard = jugador.GetComponent<XRHeadCollisionGuard>();
        if (guard != null) guard.enabled = false;
        float tiempo = 0f;
        while (tiempo < 1.5f)
        {
            tiempo += Time.unscaledDeltaTime;
            fondo.alpha = Mathf.Clamp01(tiempo / 1.5f);
            yield return null;
        }
        fondo.alpha = 1f;
        panel.SetActive(true);
        fondo.interactable = true;
        fondo.blocksRaycasts = true;
    }

    public void Reiniciar()
    {
        fondo.interactable = false;
        SceneManager.LoadSceneAsync(gameObject.scene.path);
    }
}
