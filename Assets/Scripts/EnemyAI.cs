using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.Rendering;

public class EnemyAI : MonoBehaviour
{
    public GameObject playerObj;
    Transform player;
    public float detectionRange;
    public float movementSpeed;
    public float turnSpeed;
    void Start()
    {
        player = playerObj.transform; 
    }

    void Update()
    {
        if(isFront() && inLineOfSight()){
            Vector3 targetDireciton = player.position - transform.position;
            float turnStep = turnSpeed * Time.deltaTime;
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDireciton, turnStep, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDirection);
            float moveStep = movementSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, player.position, moveStep);
            if (Vector3.Distance(transform.position, player.position) < 1f){
                moveStep = 0;
            }
        }
        //isFront();
        //inLineOfSight();
    }
    bool isFront(){
        Vector3 directionOfPlayer = transform.position - player.position;
        float angle = Vector3.Angle(transform.forward, directionOfPlayer);
        if(Mathf.Abs(angle) > 90 && Mathf.Abs(angle) < 270){
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
    }
}
