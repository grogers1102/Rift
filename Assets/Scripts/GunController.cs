using UnityEngine;

public class GunController : MonoBehaviour
{
    public WeaponInfo weapon;
    ParticleSystem muzzleFlash;
    public float nextTimeToFire = 0f;

    public Camera fpsCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0) && Time.time >= nextTimeToFire){
            RaycastHit hit;
            if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, weapon.range))
            {
                Debug.Log(hit.transform.name);
                nextTimeToFire += 1.0f / weapon.fireRate;
                // Apply damage if the object has a health script
                EnemyHealthController enemy = hit.transform.GetComponent<EnemyHealthController>();
                if (enemy != null)
                {
                    enemy.DamageToHealth(weapon.bulletDamage);
                }
            }
        }
        
    }
}
