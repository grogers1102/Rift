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
    private float lastMeleeTime;
    private bool isMeleeAttacking = false;

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

        // Fix model rotation if needed
        if (isModelFacingBackwards && modelTransform != null)
        {
            modelTransform.localRotation = Quaternion.Euler(0, 180, 0);
        }
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
                        
                        // Check melee cooldown
                        if (Time.time >= lastMeleeTime + meleeCooldown && !isMeleeAttacking)
                        {
                            Anim.SetTrigger("melee");
                            lastMeleeTime = Time.time;
                            isMeleeAttacking = true;
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
                    Anim.SetTrigger("melee");
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
                    if (Time.time >= lastShootTime + shootCooldown)
                    {
                        Anim.SetTrigger("shoot");
                        lastShootTime = Time.time;
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
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Terrain"))
        {
            isGrounded = true;
            if (rb != null)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            }
        }
    }

    // This method should be called by the animation event when the shoot animation reaches the firing point
    public void OnShoot()
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
            PlayerHealthController playerHealth = hit.collider.GetComponent<PlayerHealthController>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(shootDamage);
                Debug.Log($"Hit player for {shootDamage} damage!");
            }
            
            // Optional: Spawn hit effect at impact point
            if (hit.point != null)
            {
                // You can spawn a hit effect here if you have one
                Debug.DrawLine(shootPoint.position, hit.point, Color.red, 1f);
            }
        }
    }

    // This method should be called by the animation event when the melee animation reaches the hit point
    public void OnMeleeAttack()
    {
        // Create a sphere around the enemy to detect hits
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, meleeAttackRadius, shootableLayers);
        
        foreach (var hitCollider in hitColliders)
        {
            // Check if we hit the player
            PlayerHealthController playerHealth = hitCollider.GetComponent<PlayerHealthController>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(meleeDamage);
                Debug.Log($"Melee hit player for {meleeDamage} damage!");
            }
        }
    }

    // This method should be called by the animation event when the melee animation ends
    public void OnMeleeAttackEnd()
    {
        isMeleeAttacking = false;
    }

    // Optional: Visualize the melee attack radius in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRadius);
    }
}
