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

    private void Start()
    {
        currentHealth = maxHealth;
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
    }

    public void TakeDamage(float damage)
    {
        if (isInvincible || isDead) return;

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

        // Update UI
        UpdateHealthUI();

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
            healthSlider.value = currentHealth / maxHealth;
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
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Notify GameManager
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.PlayerDied();
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
} 