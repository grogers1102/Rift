using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class EnemyAI : MonoBehaviour
{
    public float distance;
    public Transform Player;
    public NavMeshAgent navMeshAgent;
    public Animator Anim;
    /*public GameObject playerObj;
    Transform player;
    public float detectionRange = 50;
    public float movementSpeed;
    public float turnSpeed;
    private bool isChasing = false;
    public float stopDistance = 1f;
    public float resumeDistance = 1.5f;*/

    void Start()
    {
        
    }
    void Update()
    {
        distance = Vector3.Distance(this.transform.position, Player.position);
        if(distance < 10){
            navMeshAgent.destination = Player.position;
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
