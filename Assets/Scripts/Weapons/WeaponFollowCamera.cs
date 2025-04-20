using UnityEngine;

public class WeaponFollowCamera : MonoBehaviour
{
    private Camera targetCamera;
    public float smoothSpeed = 10f;

    private void Start()
    {
        // Find the main camera at runtime
        targetCamera = Camera.main;
        if (targetCamera == null)
        {
            Debug.LogError("Main camera not found! Make sure your camera is tagged as 'MainCamera'");
        }
    }

    void LateUpdate()
    {
        if (targetCamera != null)
        {
            transform.position = Vector3.Lerp(transform.position, targetCamera.transform.position, smoothSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetCamera.transform.rotation, smoothSpeed * Time.deltaTime);
        }
    }
}
