using UnityEngine;
using UnityEngine.InputSystem;

public class PauseController : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject controlsPanel;

    private bool isPaused = false;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // ABRIR PAUSA
    public void PauseGame()
    {
        pausePanel.SetActive(true);
        controlsPanel.SetActive(false);

        Time.timeScale = 0f;
        isPaused = true;
    }

    // CONTINUAR EL JUEGO
    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        controlsPanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    // ABRIR CONTROLES DESDE PAUSA
    public void OpenControls()
    {
        pausePanel.SetActive(false);
        controlsPanel.SetActive(true);
    }

    // VOLVER A PAUSA DESDE CONTROLES
    public void BackToPause()
    {
        controlsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }
}