using UnityEngine;
using System.Collections;

public class MeleeController : BaseWeapon
{
    [Header("Melee Settings")]
    public Collider attackCollider;
    public LayerMask enemyLayer;
    public float nextTimeToAttack = 0f;
    public float comboWindow = 0.5f; // Time window to perform the next attack in the combo
    private float lastAttackTime = 0f;
    private int comboCount = 0; // Start at 0 for no attack

    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CombatFeedback combatFeedback;
    [SerializeField] private ParticleSystem hitEffect;

    protected override void Start()
    {
        base.Start();

        // Find the player controller if not set in inspector
        if (playerController == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerController = player.GetComponent<PlayerController>();
            }
            else
            {
                Debug.LogError("Player GameObject not found! Make sure it has the 'Player' tag.");
            }
        }

        // Find combat feedback if not set in inspector
        if (combatFeedback == null && playerController != null)
        {
            combatFeedback = playerController.GetComponent<CombatFeedback>();
        }

        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }
    }

    private void Update()
    {
        // Check if we're in an attack animation
        bool isInAttackAnimation = false;
        if (playerController != null && playerController.animator != null)
        {
            Animator anim = playerController.animator;
            // Check all layers
            for (int i = 0; i < anim.layerCount; i++)
            {
                AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(i);
                string stateName = anim.GetLayerName(i);
                
                if (stateInfo.IsTag("Attack"))
                {
                    isInAttackAnimation = true;
                    break;
                }
            }
            
            // If we're not in an attack animation, reset the combo
            if (!isInAttackAnimation)
            {
                if (comboCount != 0)
                {
                    comboCount = 0;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && Time.time >= nextTimeToAttack && CanUse())
        {
            Attack();
        }
    }

    private void Attack()
    {
        nextTimeToAttack = Time.time + 1f / weaponInfo.attackRate;
        lastAttackTime = Time.time;

        if (playerController != null)
        {
            
            // Only check if we're in an attack animation
            bool isInAttackAnimation = false;
            if (playerController.animator != null)
            {
                Animator animator = playerController.animator;
                for (int i = 0; i < animator.layerCount; i++)
                {
                    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(i);
                    if (stateInfo.IsTag("Attack"))
                    {
                        isInAttackAnimation = true;
                        break;
                    }
                }
            }

            if (isInAttackAnimation)
            {
                comboCount++;
                if (comboCount > 2) // Reset if we exceed max combo
                {
                    comboCount = 0;
                }
                // Set canCombo to true to allow transition to next attack
                playerController.animator.SetBool("canCombo", true);
            }
            else
            {
                comboCount = 1; // Start new combo
                // Set canCombo to false to return to pose
                playerController.animator.SetBool("canCombo", false);
            }
            
            Debug.Log($"After increment - New combo: {comboCount}");

            // Debug animation parameters before attack
            Animator anim = playerController.animator;
        }
        else
        {
            Debug.LogWarning("PlayerController is null!");
        }

        if (attackCollider != null)
        {
            attackCollider.enabled = true;
            StartCoroutine(DisableColliderAfterDelay(0.2f));
        }
    }

    private IEnumerator DisableColliderAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        DisableAttackCollider();
    }

    private void DisableAttackCollider()
    {
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (attackCollider != null && attackCollider.enabled)
        {
            if (((1 << other.gameObject.layer) & enemyLayer) != 0)
            {
                EnemyHealthController enemy = other.GetComponent<EnemyHealthController>();
                if (enemy != null)
                {
                    // Show combat feedback at hit point
                    if (combatFeedback != null)
                    {
                        combatFeedback.ShowHitMarker(other.ClosestPoint(transform.position));
                    }

                    // Apply damage
                    enemy.TakeDamage(weaponInfo.meleeDamage);

                    // Play hit effect
                    if (hitEffect != null)
                    {
                        hitEffect.Play();
                    }
                }
            }
        }
    }
}
