using UnityEngine;

public class WeaponInfo : MonoBehaviour
{
    public string weaponName;
    public string weaponArchitype;
    public bool isMelee;
    public float range;
    public int magSize;
    public int maxAmmo;
    public bool isAutomatic;
    public float fireRate;
    public float reloadSpeed;
    public float bulletDamage;
    public float meleeDamage;
    public float attackRate = 1f; // Attacks per second for melee weapons
}
