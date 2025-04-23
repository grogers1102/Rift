using UnityEngine;
using UnityEngine.UI;

public class CombatFeedback : MonoBehaviour
{
    [Header("Hit Marker")]
    public Image hitMarker;
    public Color normalColor = Color.white;
    public Color hitColor = Color.red;
    public float hitMarkerDuration = 0.1f;
    public float hitMarkerScale = 1.2f;
    private float hitMarkerTimer;
    private Vector3 originalScale;

    [Header("Impact Effects")]
    public GameObject hitEffectPrefab;
    public float effectDuration = 1f;

    private void Start()
    {
        if (hitMarker != null)
        {
            originalScale = hitMarker.transform.localScale;
            hitMarker.color = normalColor;
        }
    }

    private void Update()
    {
        if (hitMarkerTimer > 0)
        {
            hitMarkerTimer -= Time.deltaTime;
            if (hitMarkerTimer <= 0)
            {
                ResetHitMarker();
            }
        }
    }

    public void ShowHitMarker(Vector3 hitPosition)
    {
        if (hitMarker != null)
        {
            hitMarker.color = hitColor;
            hitMarker.transform.localScale = originalScale * hitMarkerScale;
            hitMarkerTimer = hitMarkerDuration;
        }

        // Show impact effect at hit position
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, hitPosition, Quaternion.identity);
            Destroy(effect, effectDuration);
        }
    }

    private void ResetHitMarker()
    {
        if (hitMarker != null)
        {
            hitMarker.color = normalColor;
            hitMarker.transform.localScale = originalScale;
        }
    }
} 