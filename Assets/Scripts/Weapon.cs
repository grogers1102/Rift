using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponInfo info;
    int currentAmmo;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentAmmo = info.magSize;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
