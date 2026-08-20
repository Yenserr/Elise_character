using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class EnemyCombat : MonoBehaviour
{
    [Header("Combate")]
    public float attackRange = 2.5f;
    public float attackCooldown = 1.5f;
    public float enragedAttackCooldown = 1.5f;

    [Header("Combos")]
    public int phase1ComboHits = 1;
    public int phase2ComboHits = 2;
    public int phase3ComboHits = 3;

    [Header("Tiempo entre golpes")]
    public float comboDelay = 1.2f;

    [Header("Referencias")]
    public Transform player;
    public Animator animator;
    public Transform mustardSpawnPoint;
    public GameObject mustardProjectilePrefab;

    [Header("Ataque de Mostaza")]
    public float mustardCooldown = 3f;

    private float nextMustardTime = 0f;
    private EnemyAI enemyAI;

    private bool isAttacking = false;
    private bool mustardSpecialActive = false;
    public bool IsPerformingSpecial => isAttacking && mustardSpecialActive;
    private float nextAttackTime = 0f;

    [Header("Poder Especial - Mostaza")]
    public float mustardSpecialCooldown = 6f;
    public float retreatDistance = 2.5f;
    public float mustardPreparationTime = 1.2f;

    private float nextMustardSpecialTime = 0f;
    private NavMeshAgent agent;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        enemyAI = GetComponent<EnemyAI>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (player == null)
            return;    
    

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (enemyAI != null &&
            enemyAI.CurrentPhase == 3 &&
            Time.time >= nextMustardSpecialTime &&
            distance > attackRange &&
            !isAttacking)
        {
            StartCoroutine(MustardSpecialAAttack());
            return;
        }

        // No atacar si está demasiado lejos
        if (distance > attackRange)
            return;       
        

        // Esperar cooldown
        if (Time.time < nextAttackTime)
            return;

        // No atacar mientras ya está atacando
        if (isAttacking)
            return;

        StartCoroutine(AttackCombo());
    }

    IEnumerator AttackCombo()
    {
        isAttacking = true;

        int comboHits = GetComboHits();

        for (int i = 0; i < comboHits; i++)
        {
            RotateTowardsPlayer();

            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            yield return new WaitForSeconds(comboDelay);
        }

        float currentCooldown = GetAttackCooldown();

        nextAttackTime = Time.time + currentCooldown;

        isAttacking = false;
    }

    int GetComboHits()
    {
        if (enemyAI == null)
            return phase1ComboHits;

        switch (enemyAI.CurrentPhase)
        {
            case 3:
                return phase3ComboHits;

            case 2:
                return phase2ComboHits;

            default:
                return phase1ComboHits;
        }
    }

    float GetAttackCooldown()
    {
        if (enemyAI != null && enemyAI.CurrentPhase >= 2)
        {
            return enragedAttackCooldown;
        }

        return attackCooldown;
    }

    void RotateTowardsPlayer()
    {
        Vector3 direction =
            player.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            transform.rotation =
                Quaternion.LookRotation(direction);
        }
    }

    void LaunchMustard()
    {
        if (mustardSpawnPoint == null)
            return;

        if (mustardProjectilePrefab == null)
            return;

        if (Time.time < nextMustardTime)
            return;

        nextMustardTime = Time.time + mustardCooldown;

        GameObject mustard =
            Instantiate(
                mustardProjectilePrefab,
                mustardSpawnPoint.position,
                Quaternion.identity
            );

        Debug.Log("MOSTAZA DORADA");
    }

    IEnumerator MustardSpecialAAttack()
{
    mustardSpecialActive = true;
    isAttacking = true;

    Debug.Log("EL BOSS PREPARA LA MOSTAZA DORADA");

    yield return new WaitForSeconds(0.6f);

    // Detener al jefe
    if (agent != null)
    {
        agent.isStopped = true;
    }

    // Mirar al jugador
    RotateTowardsPlayer();

    // Activar animación especial
    if (animator != null)
    {
        animator.SetTrigger("MustardAttack");
    }

    // Retroceder
    if (agent != null && player != null)
    {
        Vector3 directionAway =
            transform.position - player.position;

        directionAway.y = 0f;

        if (directionAway.sqrMagnitude > 0.01f)
        {
            directionAway.Normalize();

            Vector3 retreatPosition =
                transform.position +
                directionAway * retreatDistance;

            agent.isStopped = false;
            agent.SetDestination(retreatPosition);
        }
    }

    // Esperar mientras se aleja
    yield return new WaitForSeconds(mustardPreparationTime);

    // Detenerse para lanzar
    if (agent != null)
    {
        agent.isStopped = true;
    }

    RotateTowardsPlayer();

    // Lanzar mostaza
    LaunchMustard();

    Debug.Log("MOSTAZA DORADA LANZADA");

    // Pequeña pausa después del poder
    yield return new WaitForSeconds(1.2f);

    if (agent != null)
    {
        agent.isStopped = false;
    }

    nextMustardSpecialTime =
        Time.time + mustardSpecialCooldown;

    isAttacking = false;
    mustardSpecialActive = false;
}
}