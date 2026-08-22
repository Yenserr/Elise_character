using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public Animator doorAnimator;
    private bool doorsOpen = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            doorsOpen = true;
            doorAnimator.Play("Abrir Puertas", 0, 0f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            doorsOpen = false;
            doorAnimator.Play("Cerrar Puertas", 0, 0f);
        }
    }
}