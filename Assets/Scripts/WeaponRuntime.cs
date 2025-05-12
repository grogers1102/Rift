using UnityEngine;
using UnityEngine.UI;

public class WeaponRuntime : MonoBehaviour
{
    public int currentMagAmmo;
    public int currentTotalAmmo;
    public string weaponName;
    public WeaponSwitchController playerWeapons;

    void Start(){
        WeaponInfo info = playerWeapons.GetCurrentWeaponInfo();
        currentMagAmmo = info.magSize;
        currentTotalAmmo = info.maxAmmo;
        weaponName = info.weaponName;
    }
    void Update(){
        GunController gun = playerWeapons.currentGun.GetComponent<GunController>();
        currentMagAmmo = gun.currentMagAmmo;
        currentTotalAmmo = gun.currentAmmo;
    }
}
