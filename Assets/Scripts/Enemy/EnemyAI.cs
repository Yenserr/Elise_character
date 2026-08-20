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

    [Header("Jefe Final")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("Fases del Jefe")]
    public float phase2HealthPercent = 0.6f;
    public float phase3HealthPercent = 0.3f;

    [Header("Velocidad por Fase")]
    public float phase1Speed = 2.5f;
    public float phase2Speed = 5f;
    public float phase3Speed = 7f;

    private int currentPhase = 1;

    public int CurrentPhase => currentPhase;
    private NavMeshAgent agent;
    private Animator animator;

    private EnemyCombat enemyCombat;

    

    void Start()
    {
        enemyCombat = GetComponent<EnemyCombat>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        currentHealth = maxHealth;

        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (agent != null)
        {
            agent.stoppingDistance = stoppingDistance;
            agent.speed = phase1Speed;
        }
    }

    void Update()
    {
        if (player == null || agent == null)
            return;

        if (enemyCombat != null && enemyCombat.IsPerformingSpecial)
        {
            return;
        }
        CheckPhase();      

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        // FUERA DEL RANGO DE DETECCIÓN
        if (distance > detectionRange)
        {
            StopMoving();
            return;
        }

        // CERCA DEL JUGADOR
        if (distance <= attackRange)
        {
            StopMoving();
            RotateTowardsPlayer();
            return;
        }

        // PERSIGUIENDO AL JUGADOR
        agent.isStopped = false;
        agent.SetDestination(player.position);

        // Animación de caminar
        if (animator != null)
        {
            float speed =
                agent.velocity.magnitude > 0.1f ? 1f : 0f;

            animator.SetFloat("Speed", speed);
        }
    }

    void CheckPhase()
{
    float healthPercent = currentHealth / maxHealth;

    if (healthPercent <= phase3HealthPercent && currentPhase < 3)
    {
        EnterPhase3();
    }
    else if (healthPercent <= phase2HealthPercent && currentPhase < 2)
    {
        EnterPhase2();
    }
}

    void EnterPhase2()
{
    currentPhase = 2;

    if (agent != null)
    {
        agent.speed = phase2Speed;
    }

    if (animator != null)
    {
        animator.SetBool("Enraged", true);
    }

    Debug.Log("JEFE ENTRA EN FASE 2");
}

void EnterPhase3()
{
    currentPhase = 3;

    if (agent != null)
    {
        agent.speed = phase3Speed;
    }

    if (animator != null)
    {
        animator.SetBool("Enraged", true);
    }

    Debug.Log("JEFE ENTRA EN FASE 3 - HOT DOG DORADO");
}

    public void TakeDamage(float damage)
    {
        if (damage <= 0)
            return;

        currentHealth -= damage;

        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log(
            "Vida del jefe: " +
            currentHealth +
            "/" +
            maxHealth
        );

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("JEFE DERROTADO");

        if (agent != null)
        {
            agent.isStopped = true;
        }

        enabled = false;
    }

    void StopMoving()
    {
        agent.isStopped = true;

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }
    }

    void RotateTowardsPlayer()
    {
        Vector3 direction =
            player.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    10f * Time.deltaTime
                );
        }
    }
    [ContextMenu("Prueba Fase 2")]
    void TestPhase2()
    {
        currentHealth = maxHealth * phase2HealthPercent;
        CheckPhase();
    }

    [ContextMenu("Prueba Fase 3")]
    void TestPhase3()
    {
        currentHealth = maxHealth * phase3HealthPercent;
        CheckPhase();
    }
}