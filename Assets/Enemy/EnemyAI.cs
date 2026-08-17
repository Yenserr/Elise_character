using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform player;

    [Header("Distancias")]
    public float detectionRange = 10f;
   public float attackRange = 2.5f;

    [Header("Movimiento")]
   public float stoppingDistance = 1.8f;

    private NavMeshAgent agent;
    private Animator animator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        agent.stoppingDistance = stoppingDistance;
    }

    void Update()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        // Fuera del rango de detección
        if (distance > detectionRange)
        {
            agent.isStopped = true;

            if (animator != null)
                animator.SetFloat("Speed", 0f);

            return;
        }

        // Cerca del jugador
        if (distance <= attackRange)
        {
            agent.isStopped = true;

            if (animator != null)
                animator.SetFloat("Speed", 0f);

            RotateTowardsPlayer();

            return;
        }

        // Persiguiendo al jugador
        agent.isStopped = false;
        agent.SetDestination(player.position);

        // if (animator != null)
        // {
        //     animator.SetFloat("Speed", agent.velocity.magnitude);
        // }
    }

    void RotateTowardsPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                10f * Time.deltaTime
            );
        }
    }
}