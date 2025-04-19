using UnityEngine;
//using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float lookSpeed = 1.5f;
    public Camera playerCamera;
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Movement logic
        Vector3 forward = transform.forward;
        forward.y = 0;
        Vector3 right = transform.right;
        right.y = 0;

        Vector3 direction = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            direction += forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            direction += -forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            direction += -right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            direction += right;
        }
        
        transform.position += direction.normalized * speed * Time.deltaTime;
    }
}
