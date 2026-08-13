using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator;

    [Header("Configuración")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 15f;
    public float gravity = -9.81f;

    [Header("Combo System")]
    public float comboWindow = 0.5f; 
    private int comboStep = 0;
    private float lastClickTime;
    private bool isAttacking = false;

    [Header("Combat Idle Settings")]
    public float combatModeDuration = 4f; 
    private float combatTimer;
    private bool isInCombat = false;

    private float verticalVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K) && !isAttacking)
        {
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");
            StartCoroutine(PerformDodge(x, z));
            return; 
        }

        if (isAttacking) return;

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 moveDir = new Vector3(moveX, 0f, moveZ).normalized;

        if (moveDir.magnitude >= 0.1f)
        {
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            float targetSpeed = isRunning ? runSpeed : walkSpeed;
            
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            controller.Move(moveDir * targetSpeed * Time.deltaTime);
            
            animator.SetFloat("Speed", isRunning ? 6f : 2f, 0.1f, Time.deltaTime);
        }
        else
        {
            animator.SetFloat("Speed", 0f, 0.1f, Time.deltaTime);
        }

        if (controller.isGrounded) verticalVelocity = -0.5f;
        else verticalVelocity += gravity * Time.deltaTime;
        controller.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.J))
        {
            ActivateCombatMode();

            if (!isAttacking)
            {
                comboStep = 1;
                StartCoroutine(PerformCombo());
            }
            else if (comboStep == 1 && (Time.time - lastClickTime) < comboWindow)
            {
                comboStep = 2;
            }
        }

        if (isInCombat)
        {
            combatTimer -= Time.deltaTime;
            if (combatTimer <= 0f)
            {
                isInCombat = false;
                animator.SetBool("InCombat", false);
            }
        }
    }

    void ActivateCombatMode()
    {
        isInCombat = true;
        combatTimer = combatModeDuration; 
        animator.SetBool("InCombat", true);
    }

    IEnumerator PerformCombo()
    {
        isAttacking = true;
        lastClickTime = Time.time;

        animator.SetTrigger("AttackSingle");
        yield return new WaitForSeconds(0.4f);

        if (comboStep == 2)
        {
            animator.SetTrigger("AttackCombo");
            yield return new WaitForSeconds(0.6f); 
        }

        isAttacking = false;
        comboStep = 0;
    }

    IEnumerator PerformDodge(float x, float z)
    {
        isAttacking = true;
        ActivateCombatMode();

        string triggerName = "DodgeForward"; 
        if (z < -0.5f) triggerName = "DodgeBack";
        else if (x > 0.5f) triggerName = "DodgeRight";
        else if (x < -0.5f) triggerName = "DodgeLeft";

        animator.SetTrigger(triggerName);


        yield return new WaitForSeconds(0.5f); 

        isAttacking = false;
    }
}