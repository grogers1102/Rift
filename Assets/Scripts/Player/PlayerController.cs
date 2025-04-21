using UnityEngine;
using System.Collections;

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
    public Camera playerCamera;
    public FirstPersonLook cameraController;

    [Header("Attack Settings")]
    public int maxCombo = 2;
    private int currentCombo = 0;
    private float comboResetTime = 1.0f;
    private float comboTimer;

    [Header("Rolling Settings")]
    [SerializeField] private float rollDuration = 0.7f;
    [SerializeField] private int lowerBodyLayerIndex = 1;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float healAmount = 25f;
    public int maxHeals = 3;
    private int remainingHeals;

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
    private WeaponSwitchController weaponSwitchController;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        remainingHeals = maxHeals;
        
        // Camera and cursor setup
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        // Make sure lower body layer starts enabled
        if (animator != null)
        {
            animator.SetLayerWeight(lowerBodyLayerIndex, 1f);
        }
        else
        {
            Debug.LogError("Player Animator not found on Player GameObject!");
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

        weaponSwitchController = GetComponent<WeaponSwitchController>();
    }

    void Update()
    {
        Debug.Log("Update called");
        HandleMovement();
        HandleRolling();
        HandleHealing();
        HandleReloading();
        HandleComboTimer();
        UpdateEquippedWeapon();
    }

    void HandleMovement()
    {
        if (isRolling || isAttacking || isReloading)
        {
            Debug.Log($"Movement blocked - isRolling: {isRolling}, isAttacking: {isAttacking}, isReloading: {isReloading}");
            return;
        }

        // Get input
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(h, 0, v).normalized;

        bool isMoving = input.magnitude > 0f;
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float targetSpeed = isRunning ? runSpeed : walkSpeed;

        Debug.Log($"Input - h: {h}, v: {v}, isMoving: {isMoving}, isRunning: {isRunning}");

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

        // Apply movement
        controller.Move(currentVelocity * Time.deltaTime);

        // Set animator parameters
        if (animator != null)
        {
            float speedPercent = currentVelocity.magnitude / runSpeed;
            animator.SetFloat("Speed", speedPercent);
            animator.SetBool("isWalking", isMoving && !isRunning);
            animator.SetBool("isRunning", isMoving && isRunning);
            
            Debug.Log($"Movement - Speed: {speedPercent}, isWalking: {isMoving && !isRunning}, isRunning: {isMoving && isRunning}");
        }
        else
        {
            Debug.LogWarning("Animator is null in HandleMovement!");
        }
    }

    void HandleRolling()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isRolling && !isAttacking && !isReloading)
        {
            StartCoroutine(HandleDodgeRoll());
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
            Debug.Log($"Healed! Current health: {currentHealth}. Remaining heals: {remainingHeals}");
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        if (animator != null)
        {
            animator.SetTrigger("isReloading");
        }
        // Wait for reload animation to complete
        yield return new WaitForSeconds(2f); // Adjust this time to match your reload animation
        isReloading = false;
    }

    IEnumerator HandleDodgeRoll()
    {
        isRolling = true;
        animator.SetTrigger("isRolling");

        // Disable lower body layer during roll
        animator.SetLayerWeight(lowerBodyLayerIndex, 0f);

        // Wait for roll animation to complete
        yield return new WaitForSeconds(rollDuration);

        // Re-enable lower body layer
        animator.SetLayerWeight(lowerBodyLayerIndex, 1f);
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
                    animator.SetInteger("combo", 0);
                }
            }
        }
    }

    void UpdateEquippedWeapon()
    {
        if (animator == null) return;

        WeaponInfo currentWeapon = weaponSwitchController.GetCurrentWeaponInfo();
        if (currentWeapon == null) return;

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
        if (animator == null) return;

        // Get the current weapon info
        WeaponInfo currentWeapon = weaponSwitchController.GetCurrentWeaponInfo();
        if (currentWeapon == null || !currentWeapon.isMelee) return;

        // Update combo state
        currentCombo = comboCount;
        comboTimer = 0f;

        Debug.Log($"Attempting to play attack animation. Weapon: {currentWeapon.weaponName}, Combo: {comboCount}");

        // Set the combo value
        animator.SetInteger("combo", comboCount);
        
        // Trigger the attack
        animator.ResetTrigger("isAttacking");
        animator.SetTrigger("isAttacking");
        Debug.Log($"Playing attack animation with combo: {comboCount}");

        // Set attacking state
        isAttacking = true;
        Invoke("ResetAttack", 0.5f); // Reset after animation duration
    }

    void ResetAttack()
    {
        isAttacking = false;
    }

    private bool HasAnimation(string animationName)
    {
        if (animator == null) return false;

        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == animationName)
                return true;
        }
        return false;
    }
}
