using UnityEngine;
using UnityEngine.UI;

public class PlayerStaminaController : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegenRate = 10f;
    public float staminaRegenDelay = 2f;

    [Header("Action Costs")]
    public float rollCost = 25f;
    public float runCostPerSecond = 10f;
    public float meleeAttackCost = 15f;
    public float gunShotCost = 10f;

    [Header("UI Elements")]
    public Slider staminaSlider;
    public Slider easeStaminaSlider;
    public float lerpSpeed = 5f;
    public Color normalColor = Color.green;
    public Color lowColor = Color.red;
    public float lowStaminaThreshold = 25f;

    private float lastActionTime;
    private bool isRegenerating = false;
    private PlayerController playerController;

    private void Start()
    {
        // Initialize sliders
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = maxStamina;
        }
        if (easeStaminaSlider != null)
        {
            easeStaminaSlider.maxValue = maxStamina;
            easeStaminaSlider.value = maxStamina;
        }

        currentStamina = maxStamina;
        playerController = GetComponent<PlayerController>();
        UpdateStaminaUI();
    }

    private void Update()
    {
        // Test stamina drain with 'O' key
        if (Input.GetKeyDown(KeyCode.O))
        {
            UseStamina(20f); // Drain 20 stamina when pressing O
        }

        if (currentStamina < maxStamina && Time.time - lastActionTime > staminaRegenDelay)
        {
            isRegenerating = true;
            currentStamina = Mathf.Min(currentStamina + staminaRegenRate * Time.deltaTime, maxStamina);
        }
        else if (currentStamina >= maxStamina)
        {
            isRegenerating = false;
        }

        // Update main stamina bar instantly
        if (staminaSlider != null)
        {
            staminaSlider.value = currentStamina;
        }
        
        // Update ease stamina bar with smooth follow
        if (easeStaminaSlider != null)
        {
            // Use different lerp speeds for draining vs regenerating
            float currentLerpSpeed = isRegenerating ? lerpSpeed * 0.5f : lerpSpeed;
            
            // Only update ease bar if it's not already at the target value
            if (Mathf.Abs(easeStaminaSlider.value - currentStamina) > 0.01f)
            {
                easeStaminaSlider.value = Mathf.Lerp(easeStaminaSlider.value, currentStamina, currentLerpSpeed * Time.deltaTime);
            }
            else
            {
                easeStaminaSlider.value = currentStamina;
            }
        }
    }

    public bool CanPerformAction(float cost)
    {
        return currentStamina >= cost;
    }

    public void UseStamina(float amount)
    {
        currentStamina = Mathf.Max(0, currentStamina - amount);
        lastActionTime = Time.time;
        isRegenerating = false;
        UpdateStaminaUI();
    }

    public void OnRoll()
    {
        if (CanPerformAction(rollCost))
        {
            UseStamina(rollCost);
        }
    }

    public void OnMeleeAttack()
    {
        if (CanPerformAction(meleeAttackCost))
        {
            UseStamina(meleeAttackCost);
        }
    }

    public void OnGunShot()
    {
        if (CanPerformAction(gunShotCost))
        {
            UseStamina(gunShotCost);
        }
    }

    private void UpdateStaminaUI()
    {
        if (staminaSlider != null)
        {
            staminaSlider.value = currentStamina;
        }
        if (easeStaminaSlider != null)
        {
            easeStaminaSlider.value = currentStamina;
        }
    }
} 