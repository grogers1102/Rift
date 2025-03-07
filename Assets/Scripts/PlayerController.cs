using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float lookSpeed = 1.5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float xMouseMovement = Input.GetAxis("Mouse X");
        float yMouseMovement = Input.GetAxis("Mouse Y");
        transform.eulerAngles += lookSpeed * new Vector3(-yMouseMovement, xMouseMovement, 0);

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
