using UnityEngine;
using UnityEngine.UI;

public class WeaponRuntime : MonoBehaviour
{
    public int currentMagAmmo;
    public int currentTotalAmmo;
    public string weaponName;
    private WeaponInfo info;

    void Start()
    {
        info = GetComponent<WeaponInfo>();
        currentMagAmmo = info.magSize;
        currentTotalAmmo = info.maxAmmo;
        weaponName = info.weaponName;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
