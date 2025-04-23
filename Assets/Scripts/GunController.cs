using System.Collections;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public WeaponInfo weapon;
    public float nextTimeToFire = 0f;
    int currentAmmo;
    public Camera fpsCamera;
    private WeaponRuntime weaponRuntime;

    //VFX
    public ParticleSystem muzzleFlash;
    //public GameObject impactEffect;

    public Transform firepoint; //End of barrel
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentAmmo = weapon.magSize;
        weaponRuntime = GetComponent<WeaponRuntime>();
    }

    // Update is called once per frame
    void Update()
    {
        //Shooting the weapon
        if(Input.GetKeyDown(KeyCode.Mouse0) && Time.time >= nextTimeToFire && currentAmmo > 0){
            if(muzzleFlash != null){
                muzzleFlash.Play();
            }
            RaycastHit hit;
            Vector3 hitPoint;
            if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, weapon.range)){
                hitPoint = hit.point;
                Debug.Log(hit.transform.name);
                nextTimeToFire = Time.time + (1f / weapon.fireRate);
                weaponRuntime.currentMagAmmo--;
                // Apply damage if the object has a health script
                EnemyHealthController enemy = hit.transform.GetComponent<EnemyHealthController>();
                if (enemy != null)
                {
                    enemy.DamageToHealth(weapon.bulletDamage);
                    Debug.Log(enemy.health);
                }
            }
            else{
                hitPoint = fpsCamera.transform.position + fpsCamera.transform.forward * weapon.range;
            }
            StartCoroutine(SpawnTrail(firepoint.position, hitPoint));
        }
        if(Input.GetKeyDown(KeyCode.R)){
            StartCoroutine(Reload());
        }
        
    }
    IEnumerator Reload(){
        yield return new WaitForSeconds(weapon.reloadSpeed);
        weaponRuntime.currentMagAmmo = weapon.magSize;
        weaponRuntime.currentTotalAmmo -= weapon.magSize;

    }
    IEnumerator SpawnTrail(Vector3 start, Vector3 end){
        GameObject line = new GameObject("BulletLine");
        LineRenderer lr = line.GetComponent<LineRenderer>();
        // Set appearance
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
        lr.startColor = Color.yellow;
        lr.endColor = Color.red;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.02f;
        //make it fade or flicker (optional)
        yield return new WaitForSeconds(0.05f); // Duration visible
        Destroy(line);
    }
}
