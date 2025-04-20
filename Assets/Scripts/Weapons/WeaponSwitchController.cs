using UnityEngine;

public class WeaponSwitchController : MonoBehaviour
{
    [Header("Current Weapons")]
    public GameObject currentGun;    // Currently equipped gun
    public GameObject currentMelee;  // Currently equipped melee weapon

    [Header("Weapon Anchors")]
    public Transform gunAnchor;      // Where the gun is held
    public Transform meleeAnchor;    // Where melee weapons are held (parented to hand)

    private void Start()
    {
        // Initialize weapon anchors if not set
        if (gunAnchor == null)
        {
            gunAnchor = transform.Find("GunAnchor");
        }
        if (meleeAnchor == null)
        {
            meleeAnchor = transform.Find("MeleeAnchor");
        }

        // Initially equip the first weapons if they exist
        if (currentGun != null)
        {
            EquipGun(currentGun);
        }
        if (currentMelee != null)
        {
            EquipMelee(currentMelee);
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
        // Toggle between gun and melee weapon
        if (currentGun != null)
        {
            currentGun.SetActive(!currentGun.activeSelf);
        }
        if (currentMelee != null)
        {
            currentMelee.SetActive(!currentMelee.activeSelf);
        }
    }

    public void EquipGun(GameObject newGun)
    {
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
            currentGun.SetActive(true);
        }
    }

    public void EquipMelee(GameObject newMelee)
    {
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
            currentMelee.SetActive(true);
        }
    }

    // Call this when starting a new level to equip the level's weapons
    public void EquipLevelWeapons(GameObject levelGun, GameObject levelMelee)
    {
        EquipGun(levelGun);
        EquipMelee(levelMelee);
    }
}
