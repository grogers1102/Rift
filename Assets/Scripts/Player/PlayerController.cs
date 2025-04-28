using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float rotationSpeed = 720f;
    public float lookSpeed = 1.5f;
    public float acceleration = 20f;
    public float deceleration = 15f;
    public float gravity = -9.81f;
    public float jumpHeight = 1f;
    public Camera playerCamera;
    public FirstPersonLook cameraController;

    [Header("Animation Settings")]
    [SerializeField] private int upperBodyLayerIndex = 2;
    [SerializeField] private int lowerBodyLayerIndex = 1;

    [Header("Attack Settings")]
    public int maxCombo = 2;
    private int currentCombo = 0;
    private float comboResetTime = 1.0f;
    private float comboTimer;

    [Header("Rolling Settings")]
    [SerializeField] private float rollDuration = 0.7f;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float healAmount = 25f;
    public int maxHeals = 3;
    private int remainingHeals;

    [Header("Stamina Settings")]
    public bool isRunning = false;
    private PlayerStaminaController staminaController;

    [Header("Weapon Settings")]
    public bool hasCrossbow = false;
    private WeaponSwitchController weaponSwitchController;

    public void SetHeals(int amount)
    {
        remainingHeals = Mathf.Clamp(amount, 0, maxHeals);
    }

    private CharacterController controller;
    public Animator animator;
    private Vector3 moveDirection;
    private Vector3 currentVelocity;
    private bool isRolling = false;
    private bool isAttacking = false;
    private bool isReloading = false;
    private float verticalVelocity = 0f;
    private bool isGrounded;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        remainingHeals = maxHeals;
        
        // Reset vertical velocity
        verticalVelocity = 0f;
        
        // Camera and cursor setup
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        // Setup animation layers
        if (animator != null)
        {
            animator.SetLayerWeight(lowerBodyLayerIndex, 1f);
            animator.SetLayerWeight(upperBodyLayerIndex, 1f);
            // Reset all animation parameters
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
            animator.SetBool("hasCrossbow", false);
            animator.SetInteger("equippedWeapon", 0);
            animator.SetInteger("combo", 0);
            animator.SetFloat("Speed", 0f);
        }

        // Setup camera if not assigned
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // Setup camera controller if not assigned
        if (cameraController == null && playerCamera != null)
        {
            cameraController = playerCamera.GetComponent<FirstPersonLook>();
            if (cameraController != null)
            {
                cameraController.playerBody = transform;
            }
        }

        // Setup weapon controller
        weaponSwitchController = GetComponent<WeaponSwitchController>();
        if (weaponSwitchController == null)
        {
            Debug.LogWarning("WeaponSwitchController not found on player. Weapon animations may not work correctly.");
        }

        staminaController = GetComponent<PlayerStaminaController>();
    }

    void Update()
    {
        HandleMovement();
        HandleRolling();
        HandleHealing();
        HandleReloading();
        HandleComboTimer();
        UpdateEquippedWeapon();
    }

    void HandleMovement()
    {
        // Check if grounded
        isGrounded = controller.isGrounded;
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f; // Small negative value to keep grounded
        }

        // Get input
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(h, 0, v).normalized;

        bool isMoving = input.magnitude > 0f;
        isRunning = Input.GetKey(KeyCode.LeftShift);
        
        // Only allow running if we have stamina
        if (isRunning && staminaController != null && !staminaController.CanPerformAction(staminaController.runCostPerSecond * Time.deltaTime))
        {
            isRunning = false;
        }

        float targetSpeed = isRunning ? runSpeed : walkSpeed;

        // If rolling, reduce movement speed
        if (isRolling)
        {
            targetSpeed *= 0.5f; // Reduce speed during roll
        }

        if (isMoving)
        {
            // Get camera's forward and right vectors
            Vector3 forward = playerCamera.transform.forward;
            Vector3 right = playerCamera.transform.right;
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            // Calculate target velocity
            Vector3 targetVelocity = (forward * v + right * h).normalized * targetSpeed;

            // Smoothly interpolate current velocity to target velocity
            currentVelocity = Vector3.MoveTowards(
                currentVelocity,
                targetVelocity,
                acceleration * Time.deltaTime
            );

            // Only rotate if we're not using camera rotation
            if (cameraController == null)
            {
                Quaternion toRotation = Quaternion.LookRotation(input, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            // Smoothly decelerate when no input
            currentVelocity = Vector3.MoveTowards(
                currentVelocity,
                Vector3.zero,
                deceleration * Time.deltaTime
            );
        }

        // Apply gravity
        if (!isGrounded)
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        // Apply movement
        Vector3 movement = currentVelocity;
        movement.y = verticalVelocity;
        controller.Move(movement * Time.deltaTime);

        // Set animator parameters
        if (animator != null)
        {
            float speedPercent = currentVelocity.magnitude / runSpeed;
            animator.SetFloat("Speed", speedPercent);
            animator.SetBool("isWalking", isMoving && !isRunning);
            animator.SetBool("isRunning", isMoving && isRunning);
        }
    }

    void HandleRolling()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isRolling)
        {
            if (staminaController == null || staminaController.CanPerformAction(staminaController.rollCost))
            {
                StartCoroutine(HandleDodgeRoll());
                if (staminaController != null)
                {
                    staminaController.OnRoll();
                }
            }
        }
    }

    void HandleHealing()
    {
        if (Input.GetKeyDown(KeyCode.H) && remainingHeals > 0)
        {
            Heal();
        }
    }

    void HandleReloading()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && !isAttacking && !isRolling)
        {
            StartCoroutine(Reload());
        }
    }

    void Heal()
    {
        if (remainingHeals > 0)
        {
            currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
            remainingHeals--;
            if (animator != null)
            {
                animator.SetTrigger("isHealing");
            }
            
            // Update PlayerHealthController
            PlayerHealthController healthController = GetComponent<PlayerHealthController>();
            if (healthController != null)
            {
                healthController.Heal(healAmount);
            }
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        if (animator != null)
        {
            // Set reload animation on upper body layer
            animator.SetLayerWeight(upperBodyLayerIndex, 1f);
            if (hasCrossbow)
            {
                animator.SetTrigger("isReloadingCrossbow");
            }
            else
            {
                animator.SetTrigger("isReloading");
            }
        }
        // Wait for reload animation to complete
        yield return new WaitForSeconds(hasCrossbow ? 3f : 2f); // Crossbow reload takes longer
        isReloading = false;
    }

    IEnumerator HandleDodgeRoll()
    {
        isRolling = true;
        
        // Store current layer weights
        float lowerBodyWeight = animator.GetLayerWeight(lowerBodyLayerIndex);
        float upperBodyWeight = animator.GetLayerWeight(upperBodyLayerIndex);
        
        // Disable both layers during roll
        animator.SetLayerWeight(lowerBodyLayerIndex, 0f);
        animator.SetLayerWeight(upperBodyLayerIndex, 0f);
        animator.SetTrigger("isRolling");

        // Wait for roll animation to complete
        yield return new WaitForSeconds(rollDuration);

        // Re-enable both layers with their original weights
        animator.SetLayerWeight(lowerBodyLayerIndex, lowerBodyWeight);
        animator.SetLayerWeight(upperBodyLayerIndex, upperBodyWeight);
        isRolling = false;
    }

    void HandleComboTimer()
    {
        if (currentCombo > 0)
        {
            comboTimer += Time.deltaTime;
            if (comboTimer >= comboResetTime)
            {
                currentCombo = 0;
                comboTimer = 0f;
                if (animator != null)
                {
                    animator.SetInteger("combo", currentCombo);
                }
            }
        }
    }

    void UpdateEquippedWeapon()
    {
        if (weaponSwitchController == null)
        {
            // If no weapon controller, set to unequipped state
            if (animator != null)
            {
                animator.SetInteger("equippedWeapon", 0);
            }
            return;
        }

        WeaponInfo currentWeapon = weaponSwitchController.GetCurrentWeaponInfo();
        if (currentWeapon == null)
        {
            // If no weapon is equipped, set to unequipped state
            if (animator != null)
            {
                animator.SetInteger("equippedWeapon", 0);
            }
            return;
        }

        // Set equippedWeapon based on weapon type
        if (!currentWeapon.isMelee)
        {
            // Gun
            animator.SetInteger("equippedWeapon", 1);
        }
        else
        {
            // Melee weapons
            switch (currentWeapon.weaponName.ToLower())
            {
                case "knife":
                    animator.SetInteger("equippedWeapon", 2);
                    break;
                case "sword":
                    animator.SetInteger("equippedWeapon", 3);
                    break;
                default:
                    animator.SetInteger("equippedWeapon", 0); // Default/unequipped
                    break;
            }
        }
    }

    public void PerformMeleeAttack(int comboCount)
    {
        if (isAttacking || isRolling) return;
        if (staminaController != null && !staminaController.CanPerformAction(staminaController.meleeAttackCost)) return;

        isAttacking = true;
        currentCombo = comboCount;  // Use the combo count directly
        comboTimer = 0f;


        if (staminaController != null)
        {
            staminaController.OnMeleeAttack();
        }

        if (animator != null)
        {
            // Set attack animation on upper body layer
            animator.SetLayerWeight(upperBodyLayerIndex, 1f);
            animator.SetInteger("combo", currentCombo);
            animator.SetTrigger("isAttacking");

            // Debug log the current animation state
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"Current animation state: {stateInfo.fullPathHash}, combo parameter: {animator.GetInteger("combo")}");
        }

        // Reset attack state after animation
        StartCoroutine(ResetAttackAfterDelay(0.8f)); // Adjust time based on your attack animation length
    }

    IEnumerator ResetAttackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isAttacking = false;
    }

    private bool HasAnimation(string animationName)
    {
        if (animator == null) return false;
        return animator.HasState(0, Animator.StringToHash(animationName));
    }
}
