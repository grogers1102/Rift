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
    void Start(){
        Anim = GetComponent<Animator>();
    }
    void Update()
    {
        distance = Vector3.Distance(this.transform.position, Player.position);
        if(distance < detectionRange){
            if(isMelee && !isRange){
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
            else if(isRange){
                navMeshAgent.isStopped = true;
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
            navMeshAgent.isStopped = false;
            OnPatrol();
        }
    }
    
    private void OnPatrol(){
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f){
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            navMeshAgent.destination = waypoints[currentWaypointIndex].position;
            Anim.SetBool("isWalking", true);
        }
    }
}
