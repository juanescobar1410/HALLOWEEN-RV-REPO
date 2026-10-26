using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager instance { get; private set; }

    public bool panel1, panel2, panel3, panelBotonApagado;

    private void Awake()
    {
        if (instance != this && instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }

        panel1 = false;
        panel2 = false;
        panel3 = false;
        panelBotonApagado = true;
    }

    private void Update()
    {
        isPanelActive();
    }

    public void isPanelActive()
    {
        if ((panel1 && panel2 && panel3) && panelBotonApagado)
        {
            Debug.Log("Paneles encendidos, puedo activar los botones");
            panelBotonApagado = false;
        }

        if ((!panel1 || !panel2 || !panel3) && !panelBotonApagado)
        {
            Debug.Log("Panel Apagado, NO puedo activar los botones");
            panelBotonApagado = true;
        }
    }
}