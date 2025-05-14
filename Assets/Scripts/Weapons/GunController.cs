using Unity.VisualScripting;
using UnityEngine;
using System.Collections;

public class GunController : BaseWeapon
{
    [Header("Gun Settings")]
    public Transform muzzlePoint;
    public ParticleSystem muzzleFlash;
    public float nextTimeToFire = 0f;
    public int currentAmmo;
    public int currentMagAmmo;
    private Camera fpsCamera;
    private PlayerStaminaController staminaController;
    private CombatFeedback combatFeedback;

    protected override void Start ()
    {
        base.Start();
        currentMagAmmo = weaponInfo.magSize;
        currentAmmo = weaponInfo.maxAmmo;
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
        if (Input.GetKey(KeyCode.Mouse0) && Time.time >= nextTimeToFire && CanUse())
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
        if (currentMagAmmo <= 0)
        {
            StartReload();
            return;
        }

        if (staminaController != null && !staminaController.CanPerformAction(staminaController.gunShotCost))
        {
            return;
        }

        nextTimeToFire = Time.time + 1f / weaponInfo.fireRate;
        currentMagAmmo--;

        if (staminaController != null)
        {
            staminaController.OnGunShot();
        }

        // Raycast for hit detection
        if (fpsCamera != null)
        {
            RaycastHit hit;
            Vector3 rayOrigin = fpsCamera.transform.position;
            Vector3 rayDirection = fpsCamera.transform.forward;
            
            // Debug visualization
            Debug.DrawRay(rayOrigin, rayDirection * weaponInfo.range, Color.red, 1f);
            
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, weaponInfo.range))
            {
                Debug.Log($"Hit object: {hit.collider.gameObject.name} at distance {hit.distance}");
                
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
                    Debug.Log($"Applying {weaponInfo.bulletDamage} damage to enemy");
                    enemy.TakeDamage(weaponInfo.bulletDamage);
                }
            }
            else
            {
                Debug.Log("Shot missed");
                // Show miss feedback (optional)
                if (combatFeedback != null)
                {
                    combatFeedback.ShowHitMarker(rayOrigin + rayDirection * weaponInfo.range);
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
            StartCoroutine(ReloadCoroutine());
        }
    }

    private IEnumerator ReloadCoroutine()
    {
        // Try weapon-specific reload animation first
        if (animator != null)
        {
            string weaponSpecificAnim = $"{weaponInfo.weaponName}Reload";
            if (HasAnimation(weaponSpecificAnim))
            {
                animator.SetTrigger(weaponSpecificAnim);
                // Get the current animation state info
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                // Wait for the animation to complete
                yield return new WaitForSeconds(stateInfo.length);
            }
            // Fallback to archetype-specific animation
            else
            {
                string archetypeAnim = $"{weaponInfo.weaponArchitype}Reload";
                if (HasAnimation(archetypeAnim))
                {
                    animator.SetTrigger(archetypeAnim);
                    // Get the current animation state info
                    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    // Wait for the animation to complete
                    yield return new WaitForSeconds(stateInfo.length);
                }
                // Final fallback to generic reload
                else
                {
                    animator.SetTrigger("Reload");
                    // Get the current animation state info
                    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    // Wait for the animation to complete
                    yield return new WaitForSeconds(stateInfo.length);
                }
            }
        }
        else
        {
            // If no animator, use the weapon's reload speed
            yield return new WaitForSeconds(weaponInfo.reloadSpeed);
        }

        // Update ammo after reload animation completes
        currentMagAmmo = weaponInfo.magSize;
        currentAmmo -= weaponInfo.magSize;
        
        // End reload state
        isReloading = false;
    }

    public override void EndReload()
    {
        base.EndReload();
    }
}
