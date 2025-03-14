using UnityEngine;
using UnityEngine.Rendering;

public class EnemyAI : MonoBehaviour
{
    public GameObject playerObj;
    Transform player;
    public float detectionRange;
    void Start()
    {
        player = playerObj.transform; //Update to accurate player name
    }

    void Update()
    {
        //Add unique behavior
        isFront();
        inLineOfSight();
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
