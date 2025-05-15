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
    public int meleeRayCount = 5; // Reduced from 10 to 5 for WebGL performance
    public float meleeCapsuleRadius = 0.5f; // Radius of the capsule cast
    public float hitDetectionBuffer = 0.1f; // Added buffer for WebGL hit detection

    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CombatFeedback combatFeedback;
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private Transform attackPoint; // Point from where the raycast will be fired
    private Transform cameraHolder; // Added reference to camera holder

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

        // Find the camera holder in the player hierarchy
        if (playerController != null)
        {
            Transform playerTransform = playerController.transform;
            while (playerTransform != null)
            {
                if (playerTransform.name == "CameraHolder")
                {
                    cameraHolder = playerTransform;
                    break;
                }
                playerTransform = playerTransform.parent;
            }
        }

        // Set attack point to camera if not assigned
        if (attackPoint == null)
        {
            if (cameraHolder != null)
            {
                attackPoint = cameraHolder;
            }
            else
            {
                attackPoint = transform;
                Debug.LogWarning("CameraHolder not found, using transform as attack point");
            }
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
            PerformMeleeAttack();
        }
    }

    private void PerformMeleeAttack()
    {        
        // First, check for enemies in range using OverlapSphere with buffer
        Collider[] hitColliders = Physics.OverlapSphere(attackPoint.position, meleeAttackRadius + hitDetectionBuffer, enemyLayer);
        
        foreach (Collider hitCollider in hitColliders)
        {
            EnemyHealthController enemy = hitCollider.GetComponent<EnemyHealthController>();
            if (enemy != null)
            {
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
        
        // If no enemies found in range, proceed with simplified raycast
        float angleStep = meleeAttackAngle / (meleeRayCount - 1);
        float startAngle = -meleeAttackAngle / 2f;
        
        // Get the forward direction from the camera
        Vector3 forward = cameraHolder != null ? cameraHolder.forward : attackPoint.forward;
        
        // Adjust attack point to be in front of the player with buffer
        Vector3 attackPosition = attackPoint.position + forward * (1f + hitDetectionBuffer);
        
        // Cast fewer rays in a cone pattern
        for (int i = 0; i < meleeRayCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * forward;
            
            // Calculate capsule points with buffer
            Vector3 point1 = attackPosition + Vector3.up * (meleeCapsuleRadius + hitDetectionBuffer);
            Vector3 point2 = attackPosition - Vector3.up * (meleeCapsuleRadius + hitDetectionBuffer);
            
            // Cast the capsule with buffer
            RaycastHit hit;
            if (Physics.CapsuleCast(point1, point2, meleeCapsuleRadius + hitDetectionBuffer, direction, out hit, meleeAttackRadius + hitDetectionBuffer, enemyLayer))
            {
                EnemyHealthController enemy = hit.collider.GetComponent<EnemyHealthController>();
                if (enemy != null)
                {
                    if (combatFeedback != null)
                    {
                        combatFeedback.ShowHitMarker(hit.point);
                    }

                    enemy.TakeDamage(weaponInfo.meleeDamage);

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
