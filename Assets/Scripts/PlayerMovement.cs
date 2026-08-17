using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

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

    [Header("Attack Lunge Settings")]
    public float lungeSpeed = 3f;       
    public float lungeDuration = 0.15f; 

    [Header("Combat Idle Settings")]
    public float combatModeDuration = 4f; 
    private float combatTimer;
    private bool isInCombat = false;

    [Header("Efectos Visuales")]
    public TrailRenderer[] punchTrails; 
    public ParticleSystem rustDust; // NUEVO: Sistema de partículas de óxido

    [Header("Hitboxes de Combate")]
    public Hitbox leftHandHitbox;
    public Hitbox rightHandHitbox;

    private bool isStationaryAttacking = false;
    private bool isMovingAttacking = false;
    private bool isDodging = false;
    
    private float verticalVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        SetTrails(false);
        SetHitboxes(false);
    }

   void Update()
{
    // INPUT DE MOVIMIENTO   
    Vector2 input = Vector2.zero;

    if (Keyboard.current != null)
    {
        input = new Vector2(
            (Keyboard.current.dKey.isPressed ? 1f : 0f) -
            (Keyboard.current.aKey.isPressed ? 1f : 0f),

            (Keyboard.current.wKey.isPressed ? 1f : 0f) -
            (Keyboard.current.sKey.isPressed ? 1f : 0f)
        );
    }

    float moveX = input.x;
    float moveZ = input.y;

    // DODGE CON K
   
    if (Keyboard.current != null &&
        Keyboard.current.kKey.wasPressedThisFrame &&
        !isStationaryAttacking &&
        !isDodging)
    {
        float x = input.x;
        float z = input.y;

        StartCoroutine(PerformDodge(x, z));
        return;
    }

    if (isStationaryAttacking || isDodging)
        return;

    Vector3 moveDir = new Vector3(moveX, 0f, moveZ).normalized;

    
    // MOVIMIENTO
    
    if (moveDir.magnitude >= 0.1f)
    {
        bool isRunning = Keyboard.current != null &&
                         Keyboard.current.leftShiftKey.isPressed;

        float targetSpeed = isRunning ? runSpeed : walkSpeed;

        Quaternion targetRotation = Quaternion.LookRotation(moveDir);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        controller.Move(
            moveDir * targetSpeed * Time.deltaTime
        );

        animator.SetFloat(
            "Speed",
            isRunning ? 6f : 2f,
            0.1f,
            Time.deltaTime
        );

        
        // POLVO AL CORRER
        
        if (isRunning && rustDust != null)
        {
            if (!rustDust.isPlaying)
                rustDust.Play();
        }
        else if (!isRunning && rustDust != null)
        {
            if (rustDust.isPlaying)
                rustDust.Stop();
        }
    }
    else
    {
        animator.SetFloat(
            "Speed",
            0f,
            0.1f,
            Time.deltaTime
        );

        // Apagar polvo
        if (rustDust != null && rustDust.isPlaying)
        {
            rustDust.Stop();
        }
    }

   
    // GRAVEDAD
    
    ApplyGravity();

    
    // ATAQUE
    // Click izquierdo o J
  
    bool attackPressed = false;

    if (Mouse.current != null &&
        Mouse.current.leftButton.wasPressedThisFrame)
    {
        attackPressed = true;
    }

    if (Keyboard.current != null &&
        Keyboard.current.jKey.wasPressedThisFrame)
    {
        attackPressed = true;
    }

    if (attackPressed)
    {
        ActivateCombatMode();

        if (!isStationaryAttacking && !isMovingAttacking)
        {
            bool isMoving = moveDir.magnitude > 0.1f;

            if (isMoving)
                StartCoroutine(PerformMovingAttack());
            else
                StartCoroutine(PerformStationaryAttack());
        }
    }

   
    // MODO COMBATE
    
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

    void ApplyGravity()
    {
        if (controller.isGrounded) verticalVelocity = -0.5f;
        else verticalVelocity += gravity * Time.deltaTime;
        
        if (!isStationaryAttacking) 
        {
            controller.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
        }
    }

    void ActivateCombatMode()
    {
        isInCombat = true;
        combatTimer = combatModeDuration; 
        animator.SetBool("InCombat", true);
    }

    void SetTrails(bool state)
    {
        foreach (TrailRenderer trail in punchTrails)
        {
            if (trail != null) trail.emitting = state;
        }
    }

    void SetHitboxes(bool state)
    {
        if (state)
        {
            if (leftHandHitbox != null) leftHandHitbox.EnableHitbox();
            if (rightHandHitbox != null) rightHandHitbox.EnableHitbox();
        }
        else
        {
            if (leftHandHitbox != null) leftHandHitbox.DisableHitbox();
            if (rightHandHitbox != null) rightHandHitbox.DisableHitbox();
        }
    }

    IEnumerator PerformStationaryAttack()
    {
        isStationaryAttacking = true;
        animator.SetTrigger("AttackSingle"); 
        
        SetTrails(true);
        SetHitboxes(true); 

        yield return StartCoroutine(AttackLungeRoutine(lungeSpeed));
        
        SetTrails(false); 
        SetHitboxes(false); 

        yield return new WaitForSeconds(0.4f - lungeDuration);
        isStationaryAttacking = false;
    }

    IEnumerator PerformMovingAttack()
    {
        isMovingAttacking = true;
        animator.SetTrigger("AttackCombo"); 

        SetTrails(true); 
        SetHitboxes(true); 
        
        yield return new WaitForSeconds(0.4f); 
        
        SetTrails(false); 
        SetHitboxes(false); 

        yield return new WaitForSeconds(0.2f); 
        isMovingAttacking = false;
    }

    IEnumerator AttackLungeRoutine(float speed)
    {
        float timer = 0f;
        while (timer < lungeDuration)
        {
            Vector3 lungeDir = transform.forward * speed;
            lungeDir.y = verticalVelocity; 

            controller.Move(lungeDir * Time.deltaTime);
            animator.SetFloat("Speed", 6f); 
            
            timer += Time.deltaTime;
            yield return null; 
        }
        animator.SetFloat("Speed", 0f);
    }

    IEnumerator PerformDodge(float x, float z)
    {
        isDodging = true;
        ActivateCombatMode();

        // NUEVO: Hacemos que expulse una ráfaga intensa de 30 partículas de golpe al esquivar
        if (rustDust != null)
        {
            rustDust.Emit(30); 
        }

        string triggerName = "DodgeForward"; 
        if (z < -0.5f) triggerName = "DodgeBack";
        else if (x > 0.5f) triggerName = "DodgeRight";
        else if (x < -0.5f) triggerName = "DodgeLeft";

        animator.SetTrigger(triggerName);
        yield return new WaitForSeconds(0.5f); 
        isDodging = false;
    }
}