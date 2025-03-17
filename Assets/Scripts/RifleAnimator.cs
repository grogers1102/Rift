using UnityEngine;

public class RifleAnimator : MonoBehaviour
{
    Animator animator;
    public WeaponInfo info;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)){
            //Reload
        }
        if (Input.GetKeyDown(KeyCode.Mouse0)){
            //Firing
        }
    }
}
