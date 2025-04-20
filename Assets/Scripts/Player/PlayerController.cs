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
    private Animator animator;
    private Vector3 moveDirection;
    private Vector3 currentVelocity;
    private bool isRolling = false;
    private bool isAttacking = false;
    private bool isReloading = false;

    void Start()
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
    }

    void Update()
    {
        HandleMovement();
        HandleActions();
        HandleComboTimer();
        HandleHealing();
        HandleReloading();
    }

    void HandleMovement()
    {
        if (isRolling || isAttacking || isReloading) return;

        // Get input
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(h, 0, v).normalized;

        bool isMoving = input.magnitude > 0f;
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float targetSpeed = isRunning ? runSpeed : walkSpeed;

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
        }
    }

    void HandleActions()
    {
        if (animator == null) return;

        // Attack input (left click)
        if (Input.GetMouseButtonDown(0) && !isAttacking && !isReloading)
        {
            isAttacking = true;
            currentCombo++;

            if (currentCombo > maxCombo)
                currentCombo = 1;

            animator.SetInteger("combo", currentCombo);
            animator.SetTrigger("isAttacking");
            
            // Reset attack state after animation completes
            Invoke("ResetAttack", 0.5f);
        }

        // Roll input (space bar)
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

    void ResetAttack()
    {
        isAttacking = false;
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
}
