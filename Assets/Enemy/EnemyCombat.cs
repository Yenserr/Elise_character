using UnityEngine;
using System.Collections;

public class EnemyCombat : MonoBehaviour
{
    [Header("Combate")]
    public float attackRange = 2.5f;
    public float attackCooldown = 1.5f;

    [Header("Referencias")]
    public Transform player;
    public Animator animator;

    private bool isAttacking = false;
    private float nextAttackTime = 0f;

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
    }

    void Update()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        // No atacar si está demasiado lejos
        if (distance > attackRange)
            return;

        // Esperar cooldown
        if (Time.time < nextAttackTime)
            return;

        // No atacar mientras ya está atacando
        if (isAttacking)
            return;

        StartCoroutine(Attack());
    }

    IEnumerator Attack()
    {
        isAttacking = true;

        nextAttackTime = Time.time + attackCooldown;

        // Mirar al jugador
        Vector3 direction =
            player.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            transform.rotation =
                Quaternion.LookRotation(direction);
        }

        // Activar animación
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Duración aproximada del ataque
        yield return new WaitForSeconds(0.8f);

        isAttacking = false;
    }
}