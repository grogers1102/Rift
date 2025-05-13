using UnityEngine;

public class WeaponSwitchController : MonoBehaviour
{
    [Header("Current Weapons")]
    public GameObject currentGun;    // Currently equipped gun
    public GameObject currentMelee;  // Currently equipped melee weapon

    [Header("Weapon Anchors")]
    public Transform gunAnchor;      // Where the gun is held
    public Transform meleeAnchor;    // Where melee weapons are held (parented to hand)

    private Camera mainCamera;

    private void Start()
    {
        Debug.Log("WeaponSwitchController starting...");
        
        // Get the main camera
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found! Make sure your camera is tagged as 'MainCamera'");
            return;
        }

        // Initialize weapon anchors if not set
        if (gunAnchor == null)
        {
            gunAnchor = transform.Find("GunAnchor");
            Debug.Log($"Gun anchor found: {gunAnchor != null}");
        }
        if (meleeAnchor == null)
        {
            meleeAnchor = transform.Find("MeleeAnchor");
            Debug.Log($"Melee anchor found: {meleeAnchor != null}");
        }

        // Fallback in case prefabs got stripped in build
        if (currentGun == null)
        {
            Debug.Log("Attempting to load gun from Resources...");
            var gunPrefab = Resources.Load<GameObject>("Weapons/AssaultRifle");
            if (gunPrefab != null)
            {
                currentGun = Instantiate(gunPrefab);
                Debug.Log($"Successfully loaded and instantiated gun: {currentGun.name}");
            }
            else
            {
                Debug.LogError("Failed to load gun prefab from Resources/Weapons/AssaultRifle");
            }
        }

        if (currentMelee == null)
        {
            Debug.Log("Attempting to load melee weapon from Resources...");
            var meleePrefab = Resources.Load<GameObject>("Weapons/FutureBlade");
            if (meleePrefab != null)
            {
                currentMelee = Instantiate(meleePrefab);
                Debug.Log($"Successfully loaded and instantiated melee weapon: {currentMelee.name}");
            }
            else
            {
                Debug.LogError("Failed to load melee prefab from Resources/Weapons/FutureBlade");
            }
        }

        // Equip the weapons
        if (currentGun != null)
        {
            EquipGun(currentGun);
        }
        if (currentMelee != null)
        {
            EquipMelee(currentMelee);
        }
    }

    private void LateUpdate()
    {
        if (mainCamera != null && gunAnchor != null)
        {
            // Position the gun anchor relative to the camera's position and rotation
            gunAnchor.position = mainCamera.transform.position + 
                mainCamera.transform.right * 0.2f + 
                mainCamera.transform.up * -1.4f + 
                mainCamera.transform.forward * 0.5f;
            
            gunAnchor.rotation = mainCamera.transform.rotation;
        }
    }

    private void Update()
    {
        // Switch between gun and melee weapon
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SwitchWeapon();
        }
    }

    public void SwitchWeapon()
    {
        Debug.Log("Switching weapons...");
        
        if (currentGun != null && currentMelee != null)
        {
            bool isGunActive = currentGun.activeSelf;
            currentGun.SetActive(!isGunActive);
            currentMelee.SetActive(isGunActive);
            
            Debug.Log($"Gun active: {currentGun.activeSelf}, Melee active: {currentMelee.activeSelf}");
        }
    }

    public void EquipGun(GameObject newGun)
    {
        Debug.Log($"Equipping gun: {newGun?.name ?? "null"}");
        
        // Unequip current gun if exists
        if (currentGun != null)
        {
            currentGun.SetActive(false);
        }

        // Equip new gun
        currentGun = newGun;
        if (currentGun != null)
        {
            currentGun.transform.SetParent(gunAnchor);
            currentGun.transform.localPosition = Vector3.zero;
            currentGun.transform.localRotation = Quaternion.identity;
            currentGun.SetActive(false); // Start inactive
            Debug.Log($"Gun equipped and active: {currentGun.activeSelf}");
        }
    }

    public void EquipMelee(GameObject newMelee)
    {
        Debug.Log($"Equipping melee: {newMelee?.name ?? "null"}");
        
        // Unequip current melee if exists
        if (currentMelee != null)
        {
            currentMelee.SetActive(false);
        }

        // Equip new melee weapon
        currentMelee = newMelee;
        if (currentMelee != null)
        {
            currentMelee.transform.SetParent(meleeAnchor);
            currentMelee.transform.localPosition = Vector3.zero;
            currentMelee.transform.localRotation = Quaternion.identity;
            currentMelee.SetActive(true); // Start active
            Debug.Log($"Melee equipped and active: {currentMelee.activeSelf}");
        }
    }

    // Call this when starting a new level to equip the level's weapons
    public void EquipLevelWeapons(GameObject levelGun, GameObject levelMelee)
    {
        EquipGun(levelGun);
        EquipMelee(levelMelee);
    }

    public WeaponInfo GetCurrentWeaponInfo()
    {
        if (currentGun != null && currentGun.activeSelf)
        {
            return currentGun.GetComponent<WeaponInfo>();
        }
        else if (currentMelee != null && currentMelee.activeSelf)
        {
            return currentMelee.GetComponent<WeaponInfo>();
        }
        return null;
    }
}
