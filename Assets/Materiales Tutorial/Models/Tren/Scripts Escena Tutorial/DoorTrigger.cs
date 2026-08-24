using UnityEngine;
using System.Collections;

public class DoorTrigger : MonoBehaviour
{
    public Animator doorAnimator;

    public AudioSource openSound;
    public AudioSource closeSound;

    public float closeDelay = 0.7f;

    private bool doorsOpen = false;
    private Coroutine closeCoroutine = null;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Si el jugador vuelve a entrar antes de que termine el retraso,
        // cancelamos el cierre.
        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
            closeCoroutine = null;
        }

        // Solo abrir si las puertas están cerradas.
        if (!doorsOpen)
        {
            doorsOpen = true;

            doorAnimator.Play("Abrir Puertas", 0, 0f);

            if (openSound != null)
            {
                openSound.Play();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Iniciamos el retraso antes de cerrar.
        if (closeCoroutine == null)
        {
            closeCoroutine = StartCoroutine(CloseDoorsAfterDelay());
        }
    }

    private IEnumerator CloseDoorsAfterDelay()
    {
        yield return new WaitForSeconds(closeDelay);

        // Cerramos las puertas.
        doorsOpen = false;

        doorAnimator.Play("Cerrar Puertas", 0, 0f);

        if (closeSound != null)
        {
            closeSound.Play();
        }

        closeCoroutine = null;
    }
}