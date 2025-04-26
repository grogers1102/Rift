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
    /*public GameObject playerObj;
    Transform player;
    public float detectionRange = 50;
    public float movementSpeed;
    public float turnSpeed;
    private bool isChasing = false;
    public float stopDistance = 1f;
    public float resumeDistance = 1.5f;*/
    void Update()
    {
        distance = Vector3.Distance(this.transform.position, Player.position);
        if(distance < detectionRange){
            if(isMelee && !isRange){
                navMeshAgent.destination = Player.position;
                if(distance <= meleeRange && isMelee){
                    navMeshAgent.isStopped = true;
                    //Trigger Attack Animation
                }
                else{
                    navMeshAgent.isStopped = false;
                    //Continue pursuing player
                }
            }
            else if(isRange){
                // Ranged enemies stop moving and rotate to face the player
                navMeshAgent.isStopped = true;
                Vector3 direction = (Player.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
                if(isMelee && distance <= meleeRange){
                    //Trigger Attack Animation
                }
            }
        }
        else{
            navMeshAgent.isStopped = false;
            OnPatrol();
        }
    }
    
    private void OnPatrol(){
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            navMeshAgent.destination = waypoints[currentWaypointIndex].position;
        }
    }
    /*
    void Start()
    {
        player = playerObj.transform; 
    }

    void Update()
    {
        
            float distanceToPlayer = Vector3.Distance(player.position, transform.position);
             Debug.Log("Distance to Player: " + distanceToPlayer);
    
            // Stop if too close
            if (isFront() && inLineOfSight() && distanceToPlayer < stopDistance)
            {
                if (isChasing)
                {
                Debug.Log("Enemy stopping! Too close to player.");
                }
                isChasing = false;
            }

            // Resume chasing if player moves far enough away (this check runs regardless of isFront() and inLineOfSight())
            if (!isChasing && distanceToPlayer > resumeDistance)
            {
                Debug.Log("Enemy resuming chase!");
                isChasing = true;
            }

            if(isChasing){
                Debug.Log("Moving Towards player");
                Vector3 targetDireciton = player.position - transform.position;
                float turnStep = turnSpeed * Time.deltaTime;
                Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDireciton, turnStep, 0.0f);
                transform.rotation = Quaternion.LookRotation(newDirection);
                float moveStep = movementSpeed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, player.position, moveStep);
            }
    }
    bool isFront(){
        Vector3 directionOfPlayer = transform.position - player.position;
        float angle = Vector3.Angle(transform.forward, directionOfPlayer);
        if(angle < 90){
            Debug.Log("Player is in front of enemy");
            return true;
        }
        return false;
    }
    bool inLineOfSight(){
        RaycastHit _hit;
        Vector3 directionOfPlayer = player.position - transform.position;
        if(Physics.Raycast(transform.position, directionOfPlayer, out _hit, detectionRange)){
            if(_hit.transform.name == playerObj.name){
                Debug.Log("Player is in line of sight");
                return true;
            }
        }
        return false;
    }*/
}
