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
                        Anim.SetTrigger("melee");
                    }
                    else{
                        navMeshAgent.isStopped = false;
                        Anim.SetBool("isWalking", true);
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
                else{
                    // Ranged enemies stop moving and rotate to face the player
                    Vector3 direction = (Player.position - transform.position).normalized;
                    Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
                    Anim.SetTrigger("shoot");
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
}
