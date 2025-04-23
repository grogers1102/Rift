using UnityEngine;

public abstract class BaseWeapon : MonoBehaviour
{
    [Header("Weapon Info")]
    public WeaponInfo weaponInfo;
    protected Animator animator;
    protected bool isReloading = false;
    protected bool isAttacking = false;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        if (weaponInfo == null)
        {
            Debug.LogError("WeaponInfo not assigned to " + gameObject.name);
        }
    }

    public virtual bool CanUse()
    {
        return !isReloading && !isAttacking;
    }

    public virtual void StartReload()
    {
        if (!isReloading)
        {
            isReloading = true;
            animator?.SetTrigger("Reload");
        }
    }

    public virtual void EndReload()
    {
        isReloading = false;
    }

    public virtual void StartAttack()
    {
        if (!isAttacking)
        {
            isAttacking = true;
            animator?.SetTrigger("Attack");
        }
    }

    public virtual void EndAttack()
    {
        isAttacking = false;
    }
} 