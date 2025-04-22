using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("Crosshair")]
    public Image crosshair;
    public float crosshairSize = 20f;
    public Color crosshairColor = Color.white;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (crosshair != null)
        {
            // Set up crosshair
            crosshair.rectTransform.sizeDelta = new Vector2(crosshairSize, crosshairSize);
            crosshair.color = crosshairColor;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
