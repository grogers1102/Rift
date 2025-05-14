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
        
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found! Make sure your camera is tagged as 'MainCamera'");
            return;
        }

        // Robust anchor finding
        if (gunAnchor == null)
        {
            gunAnchor = transform.Find("GunAnchor");
            if (gunAnchor == null)
            {
                var gunAnchorObj = GameObject.Find("GunAnchor");
                if (gunAnchorObj != null)
                    gunAnchor = gunAnchorObj.transform;
            }
            Debug.Log($"Gun anchor found: {gunAnchor != null}");
        }
        if (meleeAnchor == null)
        {
            meleeAnchor = transform.Find("MeleeAnchor");
            if (meleeAnchor == null)
            {
                var meleeAnchorObj = GameObject.Find("MeleeAnchor");
                if (meleeAnchorObj != null)
                    meleeAnchor = meleeAnchorObj.transform;
            }
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
        // No need to manually set gunAnchor or meleeAnchor position/rotation if they're children of the arm!
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

        if (currentGun != null)
            currentGun.SetActive(false);

        currentGun = newGun;
        if (currentGun != null && gunAnchor != null)
        {
            currentGun.transform.SetParent(gunAnchor, false);
            Debug.Log($"Gun parent after SetParent: {currentGun.transform.parent?.name}");
            currentGun.transform.localPosition = Vector3.zero;
            currentGun.transform.localRotation = Quaternion.identity;
            currentGun.transform.localScale = Vector3.one;
            currentGun.SetActive(false);
            Debug.Log($"Gun equipped and active: {currentGun.activeSelf}, localScale: {currentGun.transform.localScale}, parent: {currentGun.transform.parent?.name}");
        }
        else
        {
            Debug.LogError("Cannot equip gun: gunAnchor is null!");
        }
    }

    public void EquipMelee(GameObject newMelee)
    {
        Debug.Log($"Equipping melee: {newMelee?.name ?? "null"}");

        if (currentMelee != null)
            currentMelee.SetActive(false);

        currentMelee = newMelee;
        if (currentMelee != null && meleeAnchor != null)
        {
            currentMelee.transform.SetParent(meleeAnchor, false);
            currentMelee.transform.localPosition = new Vector3(0.0003f, 0f, 0.0002f);
            currentMelee.transform.localRotation = Quaternion.identity;
            currentMelee.transform.localScale = new Vector3(0.00397f, 0.00397f, 0.00256f);
            currentMelee.SetActive(true);
            Debug.Log($"Melee equipped and active: {currentMelee.activeSelf}, localScale: {currentMelee.transform.localScale}");
        }
        else
        {
            Debug.LogError("Cannot equip melee: meleeAnchor is null!");
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
