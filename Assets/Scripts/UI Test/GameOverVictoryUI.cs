using UnityEngine;

public class GameOverVictoryUI : MonoBehaviour
{
    [Header("Panel de inicio")]
    [SerializeField] private GameObject inicioPanel;

    // BOTÓN INICIO
    public void IrAlInicio()
    {
        Time.timeScale = 1f;

        gameObject.SetActive(false);
        inicioPanel.SetActive(true);
    }

    // BOTÓN REINTENTAR
    public void Reintentar()
    {
        Time.timeScale = 1f;
    }
}