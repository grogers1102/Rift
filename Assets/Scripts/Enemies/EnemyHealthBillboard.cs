using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBillboard : MonoBehaviour
{
    [Header("References")]
    public Transform cam;
    public Slider healthSlider;
    public Slider easeHealthSlider;
    public EnemyHealthController healthController;

    [Header("Settings")]
    public float heightOffset = 2f; // Height above enemy
    public bool alwaysFaceCamera = true;
    public float lerpSpeed = 5f; // Speed at which ease health bar follows main health bar
    public float rotationLerpSpeed = 10f; // Speed at which health bar rotates to face camera

    private Transform enemyTransform;
    private Quaternion targetRotation;

    private void Start()
    {
        // Find main camera if not assigned
        if (cam == null)
        {
            cam = Camera.main.transform;
        }

        // Get health controller and enemy transform if not assigned
        if (healthController == null)
        {
            healthController = GetComponentInParent<EnemyHealthController>();
        }
        enemyTransform = healthController.transform;

        // Initialize health bars
        if (healthController != null)
        {
            if (healthSlider != null)
            {
                healthSlider.maxValue = healthController.maxHealth;
                healthSlider.value = healthController.currentHealth;
            }
            if (easeHealthSlider != null)
            {
                easeHealthSlider.maxValue = healthController.maxHealth;
                easeHealthSlider.value = healthController.currentHealth;
            }
        }
    }

    private void LateUpdate()
    {
        if (cam == null || enemyTransform == null) return;

        // Set position directly above enemy
        transform.position = enemyTransform.position + Vector3.up * heightOffset;

        // Calculate target rotation to face camera
        if (alwaysFaceCamera)
        {
            targetRotation = Quaternion.LookRotation(transform.position - cam.position);
            // Smoothly rotate to face camera
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationLerpSpeed * Time.deltaTime);
        }

        // Update health bar values
        if (healthController != null)
        {
            // Update main health bar instantly
            if (healthSlider != null)
            {
                healthSlider.value = healthController.currentHealth;
            }

            // Update ease health bar with smooth follow
            if (easeHealthSlider != null)
            {
                // Only update ease bar if it's not already at the target value
                if (Mathf.Abs(easeHealthSlider.value - healthController.currentHealth) > 0.01f)
                {
                    easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, healthController.currentHealth, lerpSpeed * Time.deltaTime);
                }
                else
                {
                    easeHealthSlider.value = healthController.currentHealth;
                }
            }
            // Force UI update after both sliders are set
            Canvas.ForceUpdateCanvases();
        }
    }
}
