using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float rotationSpeed = 720f;

    [Header("Attack Settings")]
    public int maxCombo = 2;
    private int currentCombo = 0;
    private float comboResetTime = 1.0f;
    private float comboTimer;

    [Header("Rolling Settings")]
    [SerializeField] private float rollDuration = 0.7f; // Match your roll animation
    [SerializeField] private int lowerBodyLayerIndex = 1;

    private CharacterController controller;
    private Animator animator;
    private Vector3 moveDirection;
    private bool isRolling = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Make sure lower body layer starts enabled
        animator.SetLayerWeight(lowerBodyLayerIndex, 1f);
    }

    void Update()
    {
        HandleMovement();
        HandleWeaponSwitch();
        HandleActions();
        HandleComboTimer();
    }

    void HandleMovement()
{
    if (isRolling) return; // Prevent movement during roll

    float h = Input.GetAxis("Horizontal");
    float v = Input.GetAxis("Vertical");
    Vector3 input = new Vector3(h, 0, v).normalized;

    bool isMoving = input.magnitude > 0f;
    bool isRunning = Input.GetKey(KeyCode.LeftShift);
    float speed = isRunning ? runSpeed : walkSpeed;

    if (isMoving)
    {
        moveDirection = input * speed;

        // Rotate towards movement direction
        Quaternion toRotation = Quaternion.LookRotation(input, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
    }
    else
    {
        moveDirection = Vector3.zero;
    }

    // Move the character
    controller.SimpleMove(moveDirection);

    // Set animator parameters
    animator.SetFloat("Speed", isMoving ? speed : 0f);
    animator.SetBool("isWalking", isMoving && !isRunning);
    animator.SetBool("isRunning", isMoving && isRunning);
}


    void HandleWeaponSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            animator.SetInteger("equippedWeapon", 3); // Sword
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            animator.SetInteger("equippedWeapon", 2); // Knife
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            animator.SetInteger("equippedWeapon", 1); // Gun
        else if (Input.GetKeyDown(KeyCode.Alpha0))
            animator.SetInteger("equippedWeapon", 0); // None
    }

    void HandleActions()
    {
        // Attack input (left click)
        if (Input.GetMouseButtonDown(0))
        {
            currentCombo++;

            if (currentCombo > maxCombo)
                currentCombo = 1;

            animator.SetTrigger("isAttacking");
            animator.SetInteger("combo", currentCombo);
            comboTimer = comboResetTime;
        }

        // Reload (R)
        if (Input.GetKeyDown(KeyCode.R))
        {
            animator.SetTrigger("isReloading");
        }

        // Heal (H)
        if (Input.GetKeyDown(KeyCode.H))
        {
            animator.SetTrigger("isHealing");
        }

        // Dodge Roll (Space)
        if (Input.GetKeyDown(KeyCode.Space) && !isRolling)
        {
            StartCoroutine(HandleDodgeRoll());
        }
    }

    IEnumerator HandleDodgeRoll()
    {
        isRolling = true;

        animator.SetTrigger("isRolling");

        // Temporarily disable lower body movement layer
        animator.SetLayerWeight(lowerBodyLayerIndex, 0f);

        yield return new WaitForSeconds(rollDuration);

        // Re-enable lower body movement layer
        animator.SetLayerWeight(lowerBodyLayerIndex, 1f);

        isRolling = false;
    }

    void HandleComboTimer()
    {
        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;
        }
        else
        {
            currentCombo = 0;
            animator.SetInteger("combo", 0);
        }
    }
}
