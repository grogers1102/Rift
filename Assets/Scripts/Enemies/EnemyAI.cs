using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class EnemyAI : MonoBehaviour
{
    [Header("NavMesh")]
    public float distance;
    public float detectionRange = 10f;
    public float meleeRange = 3f;
    public Transform Player;
    public NavMeshAgent navMeshAgent;
    [Header("Patrolling")]
    public Transform[] waypoints;
    private int currentWaypointIndex = 0;
    [Header("Enemy Behavior")]
    public Animator Anim;
    public bool isRange; //Enemy is capable of ranged attacks
    public bool isMelee; //Enemy is capable of melee attackes
    [Header("Model Settings")]
    public Transform modelTransform; // Reference to the model's transform
    public bool isModelFacingBackwards = true; // Set to true if model is facing backwards
    public Vector3 modelScale = Vector3.one; // Scale to apply to the model
    [Header("Ranged Attack Settings")]
    public float shootRange = 15f;
    public float shootCooldown = 2f;
    public float shootDamage = 10f;
    public Transform shootPoint; // Point from where the raycast will be fired
    public LayerMask shootableLayers; // Layers that can be hit by the raycast
    private float lastShootTime;
    [Header("Melee Attack Settings")]
    public float meleeCooldown = 3f;
    public float meleeDamage = 20f;
    public float meleeAttackRadius = 1.5f;
    public float meleeAttackAngle = 60f; // Angle of the melee attack cone
    public int meleeRayCount = 5; // Number of rays to cast in the cone
    public float meleeCapsuleRadius = 0.5f; // Radius of the capsule cast
    private float lastMeleeTime;
    private bool isMeleeAttacking = false;
    private bool isRangedAttacking = false;
    private bool canAttack = true;  // New flag to control attack cooldown

    private Rigidbody rb;
    private bool isGrounded;

    void Start(){
        Anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        
        // Configure Rigidbody
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        // Configure NavMeshAgent
        if (navMeshAgent != null)
        {
            navMeshAgent.updatePosition = true;
            navMeshAgent.updateRotation = true;
            navMeshAgent.updateUpAxis = false;
        }

        // If modelTransform is not assigned, try to find it
        if (modelTransform == null)
        {
            // Look for a child object that might be the model
            foreach (Transform child in transform)
            {
                if (child.GetComponent<Animator>() != null)
                {
                    modelTransform = child;
                    break;
                }
            }
        }

        // Apply scale if modelTransform is found
        if (modelTransform != null)
        {
            modelTransform.localScale = modelScale;
            
            // Fix model rotation if needed
            if (isModelFacingBackwards)
            {
                modelTransform.localRotation = Quaternion.Euler(0, 180, 0);
            }
        }
    }

    // Call this method when the attack animation ends
    public void OnAttackEnd()
    {
        isMeleeAttacking = false;
        isRangedAttacking = false;
        canAttack = true;
    }

    void Update()
    {
        // Check if grounded
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.1f);

        distance = Vector3.Distance(this.transform.position, Player.position);
        if(distance < detectionRange){
            if(isMelee && !isRange){
                if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
                {
                    navMeshAgent.destination = Player.position;
                    if(distance <= meleeRange){
                        Anim.SetBool("isWalking", false);
                        navMeshAgent.isStopped = true;
                        
                        // Face the player before attacking
                        Vector3 directionToPlayer = (Player.position - transform.position).normalized;
                        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
                        
                        // Apply backwards model rotation if needed
                        if (isModelFacingBackwards)
                        {
                            lookRotation *= Quaternion.Euler(0, 180, 0);
                        }
                        
                        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
                        
                        // Only attack if we're facing the player (within a small angle threshold)
                        // For backwards-facing models, we need to check if we're facing away from the player
                        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
                        bool isFacingPlayer = isModelFacingBackwards ? 
                            angleToPlayer > 150f : // For backwards models, we want to be facing away from player
                            angleToPlayer < 30f;   // For normal models, we want to be facing towards player
                        
                        // Check melee cooldown and facing direction
                        if (Time.time >= lastMeleeTime + meleeCooldown && canAttack && isFacingPlayer)
                        {
                            Anim.SetTrigger("melee");
                            lastMeleeTime = Time.time;
                            isMeleeAttacking = true;
                            canAttack = false;
                        }
                    }
                    else{
                        navMeshAgent.isStopped = false;
                        Anim.SetBool("isWalking", true);
                        
                        // Rotate model to face movement direction
                        if (modelTransform != null && navMeshAgent.velocity.magnitude > 0.1f)
                        {
                            Vector3 moveDirection = navMeshAgent.velocity.normalized;
                            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                            if (isModelFacingBackwards)
                            {
                                targetRotation *= Quaternion.Euler(0, 180, 0);
                            }
                            modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, targetRotation, Time.deltaTime * 10f);
                        }
                    }
                }
            }
            else if(isRange){
                if (navMeshAgent != null)
                {
                    navMeshAgent.isStopped = true;
                }
                if(isMelee && distance <= meleeRange){
                    if (Time.time >= lastMeleeTime + meleeCooldown && canAttack)
                    {
                        Anim.SetTrigger("melee");
                        lastMeleeTime = Time.time;
                        isMeleeAttacking = true;
                        canAttack = false;
                    }
                }
                else if(distance <= shootRange){
                    // Ranged enemies stop moving and rotate to face the player
                    Vector3 direction = (Player.position - transform.position).normalized;
                    Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
                    
                    // Ensure model faces the correct direction
                    if (modelTransform != null)
                    {
                        modelTransform.localRotation = isModelFacingBackwards ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
                    }
                    
                    // Check if we can shoot
                    if (Time.time >= lastShootTime + shootCooldown && canAttack)
                    {
                        Anim.SetTrigger("shoot");
                        lastShootTime = Time.time;
                        isRangedAttacking = true;
                        canAttack = false;
                    }
                }
            }
        }
        else{
            if (navMeshAgent != null)
            {
                navMeshAgent.isStopped = false;
            }
            OnPatrol();
        }
    }
    
    private void OnPatrol(){
        // Only patrol if we have waypoints
        if (waypoints == null || waypoints.Length == 0)
        {
            // If no waypoints, just stay in place
            if (navMeshAgent != null)
            {
                navMeshAgent.isStopped = true;
            }
            Anim.SetBool("isWalking", false);
            return;
        }

        if (navMeshAgent != null && !navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f){
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            navMeshAgent.destination = waypoints[currentWaypointIndex].position;
            Anim.SetBool("isWalking", true);
        }
        
        // Rotate model during patrol
        if (modelTransform != null && navMeshAgent != null && navMeshAgent.velocity.magnitude > 0.1f)
        {
            Vector3 moveDirection = navMeshAgent.velocity.normalized;
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            if (isModelFacingBackwards)
            {
                targetRotation *= Quaternion.Euler(0, 180, 0);
            }
            modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Ensure enemy stays grounded
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            if (rb != null)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            }
        }
    }

    // This method will be called by the AttackEndDamage behavior
    public void DealDamage()
    {
        if (isMeleeAttacking)
        {
            // Calculate the angle between each ray
            float angleStep = meleeAttackAngle / (meleeRayCount - 1);
            float startAngle = -meleeAttackAngle / 2f;
            
            // Get the forward direction of the enemy, accounting for backwards model
            Vector3 forward = isModelFacingBackwards ? -transform.forward : transform.forward;
            
            // Cast multiple capsule casts in a cone pattern
            for (int i = 0; i < meleeRayCount; i++)
            {
                // Calculate the angle for this ray
                float currentAngle = startAngle + (angleStep * i);
                
                // Rotate the forward direction by the current angle
                Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * forward;
                
                // Calculate capsule points (slightly above and below the center)
                Vector3 point1 = transform.position + Vector3.up * meleeCapsuleRadius;
                Vector3 point2 = transform.position - Vector3.up * meleeCapsuleRadius;
                
                // Cast the capsule
                RaycastHit hit;
                Debug.DrawRay(transform.position, direction * meleeAttackRadius, Color.yellow, 2f); // Make rays visible longer
                
                if (Physics.CapsuleCast(point1, point2, meleeCapsuleRadius, direction, out hit, meleeAttackRadius, shootableLayers))
                {
                    // Check if we hit the player
                    if (hit.collider.gameObject == Player.gameObject)
                    {
                        // Get the PlayerHealthController from the player
                        PlayerHealthController playerHealth = Player.GetComponent<PlayerHealthController>();
                        if (playerHealth != null)
                        {
                            playerHealth.TakeDamage(meleeDamage);
                            break; // Stop after hitting the player once
                        }
                    }
                }
            }
            
            isMeleeAttacking = false;
        }
        else if (isRangedAttacking)
        {
            if (shootPoint == null)
            {
                shootPoint = transform; // Fallback to enemy position if no shoot point is set
            }

            Vector3 direction = (Player.position - shootPoint.position).normalized;
            RaycastHit hit;
            
            if (Physics.Raycast(shootPoint.position, direction, out hit, shootRange, shootableLayers))
            {
                // Check if we hit the player
                if (hit.collider.gameObject == Player.gameObject)
                {
                    PlayerHealthController playerHealth = Player.GetComponent<PlayerHealthController>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(shootDamage);
                    }
                }
                // Optional: Spawn hit effect at impact point
                if (hit.point != null)
                {
                    Debug.DrawLine(shootPoint.position, hit.point, Color.red, 1f);
                }
            }
            isRangedAttacking = false;
        }
    }

    // Optional: Visualize the melee attack cone in the editor
    private void OnDrawGizmosSelected()
    {
        // Draw the melee attack cone
        Gizmos.color = Color.red;
        float angleStep = meleeAttackAngle / (meleeRayCount - 1);
        float startAngle = -meleeAttackAngle / 2f;
        Vector3 forward = isModelFacingBackwards ? -transform.forward : transform.forward;
        
        for (int i = 0; i < meleeRayCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * forward;
            
            // Draw the capsule cast
            Vector3 point1 = transform.position + Vector3.up * meleeCapsuleRadius;
            Vector3 point2 = transform.position - Vector3.up * meleeCapsuleRadius;
            Gizmos.DrawWireSphere(point1, meleeCapsuleRadius);
            Gizmos.DrawWireSphere(point2, meleeCapsuleRadius);
            Gizmos.DrawLine(point1, point2);
            Gizmos.DrawRay(transform.position, direction * meleeAttackRadius);
        }
    }
}
