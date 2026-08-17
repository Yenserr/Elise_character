using UnityEngine;
using System.Collections;

public class Hitbox : MonoBehaviour
{
    private Collider hitboxCollider;
    
    [Header("Efectos Visuales")]
    public GameObject hitVFXPrefab; 

    void Start()
    {
        hitboxCollider = GetComponent<Collider>();
        hitboxCollider.enabled = false; 
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (hitVFXPrefab != null)
            {
                Vector3 contactPoint = other.ClosestPoint(transform.position);
                Instantiate(hitVFXPrefab, contactPoint, Quaternion.identity);
            }

            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(0.15f, 1.5f);
            }

            StartCoroutine(HitStopRoutine(0.05f)); 
            hitboxCollider.enabled = false;
        }
    }

    IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0.05f; 
        yield return new WaitForSecondsRealtime(duration); 
        Time.timeScale = 1f; 
    }

    public void EnableHitbox()
    {
        if(hitboxCollider != null) hitboxCollider.enabled = true;
    }

    public void DisableHitbox()
    {
        if(hitboxCollider != null) hitboxCollider.enabled = false;
    }
}