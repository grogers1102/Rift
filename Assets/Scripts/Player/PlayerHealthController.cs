using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthController : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float damageMultiplier = 1f;
    public float invincibilityDuration = 1f;

    [Header("UI Elements")]
    public Slider healthSlider;
    public Slider easeHealthSlider;
    public float lerpSpeed = 5f;
    public Image damageFlash;
    public float flashDuration = 0.2f;
    public Color flashColor = new Color(1f, 0f, 0f, 0.5f);

    [Header("Visual Feedback")]
    public GameObject hitEffect;
    public GameObject deathEffect;

    private bool isInvincible = false;
    private float invincibilityTimer;
    private Color originalFlashColor;
    private float flashTimer;
    private bool isDead = false;
    private PlayerController playerController;

    private void Start()
    {
        // Get reference to PlayerController
        playerController = GetComponent<PlayerController>();
        
        // Initialize sliders
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }
        if (easeHealthSlider != null)
        {
            easeHealthSlider.maxValue = maxHealth;
            easeHealthSlider.value = maxHealth;
        }

        ResetHealth();
        UpdateHealthUI();

        if (damageFlash != null)
        {
            originalFlashColor = damageFlash.color;
            damageFlash.color = Color.clear;
        }
    }

    private void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
            }
        }

        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0)
            {
                if (damageFlash != null)
                {
                    damageFlash.color = Color.clear;
                }
            }
        }

        // Test damage with 'P' key
        if (Input.GetKeyDown(KeyCode.P))
        {
            TakeDamage(10f); // Take 10 damage when pressing P
        }

        // Smooth health bar update
        if (healthSlider != null && easeHealthSlider != null)
        {
            healthSlider.value = currentHealth;
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, currentHealth, lerpSpeed * Time.deltaTime);
        }
        if(currentHealth <= 0){
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameOver");
        }
    }

    public void TakeDamage(float damage)
    {
        // Check both regular invincibility and roll invincibility
        if (isInvincible || isDead || (playerController != null && playerController.IsInvincible)) return;

        // Apply damage
        currentHealth -= damage * damageMultiplier;

        // Visual feedback
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        // Flash screen
        if (damageFlash != null)
        {
            damageFlash.color = flashColor;
            flashTimer = flashDuration;
        }

        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Start invincibility period
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
        }
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
        if (easeHealthSlider != null)
        {
            easeHealthSlider.value = currentHealth;
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Play death effect
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // Disable player controls
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Notify GameManager
        if (GameManager.instance != null)
        {
            GameManager.instance.PlayerDied();
        }

        // Destroy after delay to allow effects to play
        Destroy(gameObject, 2f);
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHealthUI();
    }

    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        isInvincible = false;
        UpdateHealthUI();
    }
} 