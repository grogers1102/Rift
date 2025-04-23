using UnityEngine;

public class WeaponSwitchController : MonoBehaviour
{
    //public GunController currentWeapon; 
    public int selectedWeapon = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //currentWeapon = transform.GetChild(selectedWeapon).gameObject.GetComponent<GunController>;
        int previousWeapon = selectedWeapon;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                selectedWeapon = i;
            }
        }
        // Change weapon only if selection is different
        if (previousWeapon != selectedWeapon)
        {
            SelectWeapon();
        }
    }
    void SelectWeapon()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(i == selectedWeapon);
        }
    }
}
