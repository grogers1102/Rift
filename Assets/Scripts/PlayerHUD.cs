using TMPro;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    public GameObject weaponHolder;
    public TextMeshProUGUI weaponNameText;
    public TextMeshProUGUI magText;
    public TextMeshProUGUI totalAmmoText;

    // Update is called once per frame
    void Update()
    {
        GameObject activeWeapon = GetActiveWeapon();
        if (activeWeapon != null)
        {
            WeaponRuntime runtime = activeWeapon.GetComponent<WeaponRuntime>();
            if (runtime != null)
            {
                weaponNameText.text = runtime.weaponName;
                magText.text = runtime.currentMagAmmo.ToString();
                totalAmmoText.text = runtime.currentTotalAmmo.ToString();
            }
        }
    }
    GameObject GetActiveWeapon()
    {
        foreach (Transform child in weaponHolder.transform)
        {
            if (child.gameObject.activeSelf)
            {
                return child.gameObject;
            }
        }
        return null;
    }
}
