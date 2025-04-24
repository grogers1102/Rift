using UnityEngine;

public class EnemyHealthBillboard : MonoBehaviour
{
    public Transform cam;
    // Update is called once per frame
    void Update()
    {
        transform.LookAt(transform.position + cam.forward);
    }
}
