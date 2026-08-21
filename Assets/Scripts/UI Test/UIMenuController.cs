using UnityEngine;
using UnityEngine.SceneManagement;

public class UIMenuController : MonoBehaviour
{
    // Paneles
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject controlsPanel;
    public GameObject creditsPanel;

    // ABRIR AJUSTES
    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    // ABRIR CONTROLES
    public void OpenControls()
    {
        settingsPanel.SetActive(false);
        controlsPanel.SetActive(true);
    }

    // ABRIR CRÉDITOS
    public void OpenCredits()
    {
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    // VOLVER AL INICIO
    public void BackToMainMenu()
    {
        mainMenuPanel.SetActive(true);

        settingsPanel.SetActive(false);
        controlsPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }

    // JUGAR
    public void PlayGame()
    {
        SceneManager.LoadScene("underclass");
    }

    // SALIR
    public void QuitGame()
    {
        Application.Quit();
    }
}