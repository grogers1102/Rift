using UnityEngine;

public class WeaponFollowCamera : MonoBehaviour
{
    public Transform target; // Reference to the camera
    public float smoothSpeed = 10f;

    void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, target.position, smoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, smoothSpeed * Time.deltaTime);
    }
}
