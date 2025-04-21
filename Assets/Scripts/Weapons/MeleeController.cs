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
    private PlayerController playerController;

    protected override void Start()
    {
        base.Start();

        // Find the player controller
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            if (playerController == null)
            {
                Debug.LogWarning("PlayerController not found on player GameObject!");
            }
        }
        else
        {
            Debug.LogWarning("Player GameObject not found! Make sure it has the 'Player' tag.");
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
            AnimatorStateInfo stateInfo = playerController.animator.GetCurrentAnimatorStateInfo(0);
            isInAttackAnimation = stateInfo.IsName("Attack1") || stateInfo.IsName("Attack2");
            
            // If we're not in an attack animation and enough time has passed, reset the combo
            if (!isInAttackAnimation && Time.time - lastAttackTime > comboWindow)
            {
                if (comboCount != 0)
                {
                    Debug.Log("Combo reset due to time window");
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
            // Only increment combo if we're not at max combo
            if (comboCount < 2)
            {
                comboCount++;
            }
            else
            {
                // If we're at max combo, reset to 1 to start a new combo
                comboCount = 1;
            }
            
            Debug.Log($"Starting attack with combo: {comboCount}");
            playerController.PerformMeleeAttack(comboCount);
        }
        else
        {
            Debug.LogWarning("PlayerController is null!");
        }

        if (attackCollider != null)
        {
            attackCollider.enabled = true;
            StartCoroutine(DisableColliderAfterDelay(0.2f)); // You can tweak timing
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
                EnemyHealthController enemyHealth = other.GetComponent<EnemyHealthController>();
                if (enemyHealth != null)
                {
                    enemyHealth.DamageToHealth(weaponInfo.meleeDamage);
                }
            }
        }
    }
}
