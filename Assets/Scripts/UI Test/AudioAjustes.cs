using UnityEngine;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;

    private void Start()
    {
        volumeSlider.value = AudioListener.volume;
        volumeSlider.onValueChanged.AddListener(CambiarVolumen);
    }

    private void CambiarVolumen(float volumen)
    {
        AudioListener.volume = volumen;
    }
}