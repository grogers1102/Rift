using UnityEngine;

public class MeleeController : BaseWeapon
{
    [Header("Melee Settings")]
    public Collider attackCollider;
    public LayerMask enemyLayer;
    public float nextTimeToAttack = 0f;
    public Animator playerAnimator; // Reference to the player's animator

    protected override void Start()
    {
        base.Start();
        // Find the player's animator if not set
        if (playerAnimator == null)
        {
            playerAnimator = GetComponentInParent<Animator>();
            if (playerAnimator == null)
            {
                Debug.LogWarning("Player Animator not found for " + gameObject.name);
            }
        }

        // Disable the attack collider by default
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && Time.time >= nextTimeToAttack && CanUse())
        {
            Attack();
        }
    }

    private void Attack()
    {
        nextTimeToAttack = Time.time + 1f / weaponInfo.attackRate;
        
        // Trigger the player's attack animation
        if (playerAnimator != null)
        {
            // Use weapon archetype to determine which attack animation to play
            switch (weaponInfo.weaponArchitype.ToLower())
            {
                case "sword":
                    playerAnimator.SetTrigger("SwordAttack");
                    break;
                case "axe":
                    playerAnimator.SetTrigger("AxeAttack");
                    break;
                case "dagger":
                    playerAnimator.SetTrigger("DaggerAttack");
                    break;
                default:
                    playerAnimator.SetTrigger("MeleeAttack");
                    break;
            }
        }

        // Enable the attack collider
        if (attackCollider != null)
        {
            attackCollider.enabled = true;
            // Disable after a short time (adjust based on your animation length)
            Invoke(nameof(DisableAttackCollider), 0.2f);
        }
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
        // Only check for hits when the collider is enabled (during attack)
        if (attackCollider != null && attackCollider.enabled)
        {
            // Check if the collided object is on the enemy layer
            if (((1 << other.gameObject.layer) & enemyLayer) != 0)
            {
                EnemyHealthController enemyHealth = other.GetComponent<EnemyHealthController>();
                if (enemyHealth != null)
                {
                    enemyHealth.DamageToHealth(weaponInfo.meleeDamage);
                }
            }
        }
    }
} 