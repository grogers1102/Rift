using TMPro;
using UnityEngine;

public class WeaponUIManager : MonoBehaviour
{
    [Header("Ammo UI")]
    public WeaponSwitchController weaponController;
    public TextMeshProUGUI currentMagAmmo;
    public TextMeshProUGUI currentMaxAmmo;
    public TextMeshProUGUI currentWeaponName;
    private WeaponRuntime currentRuntime;

    void Update()
    {
        UpdateCurrentWeapon();
        UpdateAmmoUI();
    }
    void UpdateCurrentWeapon(){
        GameObject currentWeapon = null;
        if (weaponController.currentGun != null && weaponController.currentGun.activeSelf){
            currentWeapon = weaponController.currentGun;
        }
        else if (weaponController.currentMelee != null && weaponController.currentMelee.activeSelf){
            currentWeapon = weaponController.currentMelee;
        }
        if (currentWeapon != null){
            WeaponRuntime runtime = currentWeapon.GetComponent<WeaponRuntime>();
            if (runtime != currentRuntime){
                currentRuntime = runtime;
            }
        }
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
