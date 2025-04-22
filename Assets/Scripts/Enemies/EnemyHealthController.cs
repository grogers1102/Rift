using UnityEngine;
using UnityEngine.AI;
using Rift.Level;
using System.Collections;

public class EnemyHealthController : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float damageMultiplier = 1f;
    public bool isInvincible = false;
    public float invincibilityDuration = 0.5f;

    [Header("Visual Feedback")]
    public GameObject hitEffect;
    public GameObject deathEffect;
    public float hitEffectDuration = 0.2f;
    public float deathEffectDuration = 2f;
    public float hitFlashDuration = 0.1f;
    public Color hitFlashColor = Color.red;
    private Color originalColor;
    private Renderer enemyRenderer;

    private float invincibilityTimer;
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;
        }
        
        // Register with TransitionManager
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.RegisterEnemy(gameObject);
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
    }

    public void TakeDamage(float damage)
    {
        if (isInvincible || isDead) return;

        // Apply damage
        currentHealth -= damage * damageMultiplier;

        // Visual feedback
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(effect, hitEffectDuration);
        }

        // Flash effect
        if (enemyRenderer != null)
        {
            StartCoroutine(FlashEffect());
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

    private System.Collections.IEnumerator FlashEffect()
    {
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = hitFlashColor;
            yield return new WaitForSeconds(hitFlashDuration);
            enemyRenderer.material.color = originalColor;
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Play death effect
        if (deathEffect != null)
        {
            GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
            Destroy(effect, deathEffectDuration);
        }

        // Notify TransitionManager
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.UnregisterEnemy(gameObject);
        }

        // Disable components
        EnemyAI enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.enabled = false;
        }

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
        }

        // Disable colliders
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }

        // Make enemy invisible but keep effects playing
        if (enemyRenderer != null)
        {
            enemyRenderer.enabled = false;
        }

        // Destroy after delay to allow effects to play
        Destroy(gameObject, deathEffectDuration);
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }
}
