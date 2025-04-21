using UnityEngine;

public class WeaponFollowCamera : MonoBehaviour
{
    private Transform targetAnchor;
    public float smoothSpeed = 10f;

    private void Start()
    {
        // Find the gun anchor in the parent hierarchy
        targetAnchor = transform.parent;
        if (targetAnchor == null)
        {
            Debug.LogError("WeaponFollowCamera needs to be a child of the weapon anchor!");
        }
    }

    void LateUpdate()
    {
        if (targetAnchor != null)
        {
            // Follow the anchor's position and rotation
            transform.position = Vector3.Lerp(transform.position, targetAnchor.position, smoothSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetAnchor.rotation, smoothSpeed * Time.deltaTime);
        }
    }
}
