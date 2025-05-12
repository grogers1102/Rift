using TMPro;
using UnityEngine;

public class WeaponUIManager : MonoBehaviour
{
    [Header("Ammo UI")]
    public WeaponSwitchController weaponController;
    public TextMeshProUGUI currentMagAmmo;
    public TextMeshProUGUI currentMaxAmmo;
    public TextMeshProUGUI currentWeaponName;
    public WeaponRuntime currentRuntime;

    void Update()
    {
        UpdateCurrentWeapon();
        UpdateAmmoUI();
    }
    void UpdateCurrentWeapon(){
        GunController gun = null;
        MeleeController melee = null;
        if (weaponController.currentGun != null && weaponController.currentGun.activeSelf){
            gun = weaponController.currentGun.GetComponent<GunController>();
        }
        else if (weaponController.currentMelee != null && weaponController.currentMelee.activeSelf){
            melee = weaponController.GetComponent<MeleeController>();
        }
        /*if (currentWeapon != null){
            WeaponRuntime runtime = currentWeapon.GetComponent<WeaponRuntime>();
            if (runtime != currentRuntime){
                currentRuntime = runtime;
            }
        }*/
    }
    void UpdateAmmoUI(){
        WeaponInfo info = weaponController.GetCurrentWeaponInfo();
        currentWeaponName.text = info.weaponName;
        if (currentRuntime != null && weaponController.currentGun.activeSelf){
            currentMagAmmo.text = currentRuntime.currentMagAmmo.ToString();
            currentMaxAmmo.text = currentRuntime.currentTotalAmmo.ToString();
        }
        else{
            currentMagAmmo.text = "--";
            currentMaxAmmo.text = "--";
        }

    }
}
