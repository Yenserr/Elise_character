using UnityEngine;

public class MustardProjectile : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 8f;
    public float lifetime = 6f;

    [Header("Daño")]
    public float damage = 10f;

    [Header("Empuje")]
    public float knockbackForce = 3f;

    private Transform player;

    void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (player == null)
            return;

        Vector3 direction =
            (player.position - transform.position).normalized;

        transform.position +=
            direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        Debug.Log("La mostaza golpeó");

        
        // DAÑO
        

        EnemyAI playerHealth =
            other.GetComponent<EnemyAI>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }

        
        // EMPUJE
        

        Rigidbody rb =
            other.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 knockbackDirection =
                (other.transform.position - transform.position);

            knockbackDirection.y = 0f;

            if (knockbackDirection.sqrMagnitude > 0.01f)
            {
                knockbackDirection.Normalize();

                rb.AddForce(
                    knockbackDirection * knockbackForce,
                    ForceMode.Impulse
                );
            }
        }

        Destroy(gameObject);
    }
}