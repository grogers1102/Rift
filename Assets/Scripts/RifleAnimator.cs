using UnityEngine;

public class RifleAnimator : MonoBehaviour
{
    Animator animator;
    public WeaponInfo info;
    float reloadSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        reloadSpeed = info.reloadSpeed;
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && info.isMelee == false){
            //Reload
            //animator.SetBool("isReloading", true);
            animator.SetTrigger("Reload");
            //GetComponent<Animation>().Play("Magazine|MagazineAction_001");
        }
        /*else{
            animator.SetBool("isReloading", false);
        }*/
        if (Input.GetKeyDown(KeyCode.Mouse0) && info.isMelee == true){
            //Firing
        }
        else{

        }
    }
}
