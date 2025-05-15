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
    public float hitDetectionBuffer = 0.1f; // Added buffer for WebGL hit detection
    private Camera fpsCamera;
    private PlayerStaminaController staminaController;
    private CombatFeedback combatFeedback;
    private Transform cameraHolder; // Added reference to camera holder

    protected override void Start()
    {
        base.Start();
        currentMagAmmo = weaponInfo.magSize;
        currentAmmo = weaponInfo.maxAmmo;
        staminaController = GetComponent<PlayerStaminaController>();
        combatFeedback = GetComponent<CombatFeedback>();
        
        // Find the player first
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found! Make sure the player has the 'Player' tag.");
            return;
        }

        // Find the camera holder in the player hierarchy
        cameraHolder = player.transform.Find("CameraHolder");
        if (cameraHolder == null)
        {
            Debug.LogError("CameraHolder not found! Make sure there's a CameraHolder object under the player.");
            return;
        }

        // Get the camera component
        fpsCamera = cameraHolder.GetComponentInChildren<Camera>();
        if (fpsCamera == null)
        {
            Debug.LogError("Camera not found in CameraHolder! Make sure there's a Camera component in the CameraHolder or its children.");
            return;
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

        // Raycast for hit detection with buffer
        if (fpsCamera != null && cameraHolder != null)
        {
            RaycastHit hit;
            Vector3 rayOrigin = cameraHolder.position;
            Vector3 rayDirection = cameraHolder.forward;
            
            // Add a small spread to the ray for better WebGL hit detection
            rayDirection += new Vector3(
                Random.Range(-hitDetectionBuffer, hitDetectionBuffer),
                Random.Range(-hitDetectionBuffer, hitDetectionBuffer),
                Random.Range(-hitDetectionBuffer, hitDetectionBuffer)
            );
            rayDirection.Normalize();
            
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, weaponInfo.range + hitDetectionBuffer))
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
                // Show miss feedback
                if (combatFeedback != null)
                {
                    combatFeedback.ShowHitMarker(rayOrigin + rayDirection * weaponInfo.range);
                }
            }
        }
        else
        {
            Debug.LogError("FPS Camera or CameraHolder is null!");
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
