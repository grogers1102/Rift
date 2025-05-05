using UnityEngine;
using System.Collections;

public class MeleeController : BaseWeapon
{
    [Header("Melee Settings")]
    public LayerMask enemyLayer;
    public float nextTimeToAttack = 0f;
    public float comboWindow = 0.5f; // Time window to perform the next attack in the combo
    private float lastAttackTime = 0f;
    private int comboCount = 0; // Start at 0 for no attack

    [Header("Raycast Settings")]
    public float meleeAttackRadius = 2f; // Reduced from 5f to 2f for more precise hits
    public float meleeAttackAngle = 60f; // Angle of the melee attack cone
    public int meleeRayCount = 10; // Number of rays to cast in the cone
    public float meleeCapsuleRadius = 0.5f; // Radius of the capsule cast

    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CombatFeedback combatFeedback;
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private Transform attackPoint; // Point from where the raycast will be fired

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
        }

        // Find combat feedback if not set in inspector
        if (combatFeedback == null && playerController != null)
        {
            combatFeedback = playerController.GetComponent<CombatFeedback>();
        }

        // Set attack point to camera if not assigned
        if (attackPoint == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                attackPoint = mainCamera.transform;
            }
            else
            {
                attackPoint = transform;
            }
        }

        // Debug layer setup
        Debug.Log($"Melee attack enemy layer: {enemyLayer.value}");
        Debug.Log($"Layer 'Enemies' index: {LayerMask.NameToLayer("Enemies")}");
        Debug.Log($"Layer 'Enemies' mask value: {1 << LayerMask.NameToLayer("Enemies")}");
        
        // Verify the layer mask is set correctly
        if (enemyLayer != (1 << LayerMask.NameToLayer("Enemies")))
        {
            Debug.LogWarning("Enemy layer mask might not be set correctly! Please check the layer mask in the inspector.");
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
            // Perform the raycast attack
            PerformRaycastAttack();
        }
    }

    private void PerformRaycastAttack()
    {        
        Debug.Log("Starting melee attack raycast");
        
        // First, check for enemies in range using OverlapSphere
        Collider[] hitColliders = Physics.OverlapSphere(attackPoint.position, meleeAttackRadius, enemyLayer);
        foreach (Collider hitCollider in hitColliders)
        {
            EnemyHealthController enemy = hitCollider.GetComponent<EnemyHealthController>();
            if (enemy != null)
            {
                Debug.Log($"Found enemy in range: {hitCollider.gameObject.name}");
                enemy.TakeDamage(weaponInfo.meleeDamage);
                
                // Show combat feedback at hit point
                if (combatFeedback != null)
                {
                    combatFeedback.ShowHitMarker(hitCollider.transform.position);
                }

                // Play hit effect
                if (hitEffect != null)
                {
                    hitEffect.Play();
                }
                
                return; // Exit after hitting one enemy
            }
        }
        
        // If no enemies found in range, proceed with raycast as before
        float angleStep = meleeAttackAngle / (meleeRayCount - 1);
        float startAngle = -meleeAttackAngle / 2f;
        
        // Get the forward direction from the attack point
        Vector3 forward = attackPoint.forward;
        
        // Adjust attack point to be in front of the player
        Vector3 attackPosition = attackPoint.position + forward * 1f; // Move 1 unit forward from camera
        Debug.Log($"Attack direction: {forward}, Position: {attackPosition}, Attack radius: {meleeAttackRadius}");
        
        // Cast multiple capsule casts in a cone pattern
        for (int i = 0; i < meleeRayCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * forward;
            
            // Calculate capsule points (slightly above and below the center)
            Vector3 point1 = attackPosition + Vector3.up * meleeCapsuleRadius;
            Vector3 point2 = attackPosition - Vector3.up * meleeCapsuleRadius;
            
            // Cast the capsule
            RaycastHit hit;
            Debug.DrawRay(attackPosition, direction * meleeAttackRadius, Color.yellow, 2f);

            if (Physics.CapsuleCast(point1, point2, meleeCapsuleRadius, direction, out hit, meleeAttackRadius, enemyLayer))
            {
                Debug.DrawRay(attackPosition, direction * hit.distance, Color.red, 2f);
                Debug.Log($"Ray {i} hit something at distance {hit.distance}. Hit object: {hit.collider.gameObject.name}");
                
                EnemyHealthController enemy = hit.collider.GetComponent<EnemyHealthController>();
                if (enemy != null)
                {
                    Debug.Log($"Found EnemyHealthController on hit object. Current health: {enemy.currentHealth}");
                    
                    if (combatFeedback != null)
                    {
                        combatFeedback.ShowHitMarker(hit.point);
                    }

                    enemy.TakeDamage(weaponInfo.meleeDamage);
                    Debug.Log($"Applied {weaponInfo.meleeDamage} damage to enemy. New health: {enemy.currentHealth}");

                    if (hitEffect != null)
                    {
                        hitEffect.Play();
                    }
                    
                    break;
                }
            }
        }
    }

    // Optional: Visualize the melee attack sphere in the editor
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        // Draw the melee attack sphere
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint.position, meleeAttackRadius);

        // Draw the melee attack cone
        Gizmos.color = Color.red;
        float angleStep = meleeAttackAngle / (meleeRayCount - 1);
        float startAngle = -meleeAttackAngle / 2f;
        Vector3 forward = attackPoint.forward;
        
        for (int i = 0; i < meleeRayCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * forward;
            
            Vector3 point1 = attackPoint.position + Vector3.up * meleeCapsuleRadius;
            Vector3 point2 = attackPoint.position - Vector3.up * meleeCapsuleRadius;
            Gizmos.DrawWireSphere(point1, meleeCapsuleRadius);
            Gizmos.DrawWireSphere(point2, meleeCapsuleRadius);
            Gizmos.DrawLine(point1, point2);
            Gizmos.DrawRay(attackPoint.position, direction * meleeAttackRadius);
        }
    }
}
