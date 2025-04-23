using Unity.VisualScripting;
using UnityEngine;

public class GunController : BaseWeapon
{
    [Header("Gun Settings")]
    public Transform muzzlePoint;
    public ParticleSystem muzzleFlash;
    public float nextTimeToFire = 0f;
    public int currentAmmo;
    private Camera fpsCamera;
    private PlayerStaminaController staminaController;
    private CombatFeedback combatFeedback;

    protected override void Start ()
    {
        base.Start();
        currentAmmo = weaponInfo.magSize;
        staminaController = GetComponent<PlayerStaminaController>();
        combatFeedback = GetComponent<CombatFeedback>();
        
        // Find the main camera at runtime
        fpsCamera = Camera.main;
        if (fpsCamera == null)
        {
            Debug.LogError("Main camera not found! Make sure your camera is tagged as 'MainCamera'");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && Time.time >= nextTimeToFire && CanUse())
        {
            Fire();
        }

        if (Input.GetKeyDown(KeyCode.R) && CanUse())
        {
            StartReload();
        }
    }

    private void Fire()
    {
        if (currentAmmo <= 0)
        {
            StartReload();
            return;
        }

        if (staminaController != null && !staminaController.CanPerformAction(staminaController.gunShotCost))
        {
            return;
        }

        nextTimeToFire = Time.time + 1f / weaponInfo.fireRate;
        currentAmmo--;

        if (staminaController != null)
        {
            staminaController.OnGunShot();
        }

        // Raycast for hit detection
        if (fpsCamera != null)
        {
            RaycastHit hit;
            Vector3 rayOrigin = fpsCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
            if (Physics.Raycast(rayOrigin, fpsCamera.transform.forward, out hit, weaponInfo.range))
            {
                // Show combat feedback
                if (combatFeedback != null)
                {
                    combatFeedback.ShowHitMarker(hit.point);
                }

                // Play muzzle flash effect
                if (muzzleFlash != null)
                {
                    muzzleFlash.Play();
                }

                // Apply damage if the object has a health script
                EnemyHealthController enemy = hit.transform.GetComponent<EnemyHealthController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(weaponInfo.bulletDamage);
                }
            }
            else
            {
                // Show miss feedback (optional)
                if (combatFeedback != null)
                {
                    combatFeedback.ShowHitMarker(rayOrigin + fpsCamera.transform.forward * weaponInfo.range);
                }
            }
        }

        // Play weapon-specific fire animation if it exists
        if (animator != null)
        {
            string weaponSpecificAnim = $"{weaponInfo.weaponName}Fire";
            if (HasAnimation(weaponSpecificAnim))
            {
                animator.SetTrigger(weaponSpecificAnim);
            }
        }
    }

    private bool HasAnimation(string animationName)
    {
        if (animator == null) return false;
        
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == animationName)
                return true;
        }
        return false;
    }

    public override void StartReload()
    {
        if (!isReloading)
        {
            isReloading = true;
            
            // Try weapon-specific reload animation first
            if (animator != null)
            {
                string weaponSpecificAnim = $"{weaponInfo.weaponName}Reload";
                if (HasAnimation(weaponSpecificAnim))
                {
                    animator.SetTrigger(weaponSpecificAnim);
                }
                // Fallback to archetype-specific animation
                else
                {
                    string archetypeAnim = $"{weaponInfo.weaponArchitype}Reload";
                    if (HasAnimation(archetypeAnim))
                    {
                        animator.SetTrigger(archetypeAnim);
                    }
                    // Final fallback to generic reload
                    else
                    {
                        animator.SetTrigger("Reload");
                    }
                }
            }
        }
    }

    public override void EndReload()
    {
        base.EndReload();
        currentAmmo = weaponInfo.magSize;
    }
}
