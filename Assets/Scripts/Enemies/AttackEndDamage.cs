using UnityEngine;

public class AttackEndDamage : StateMachineBehaviour
{
    public string methodName = "DealDamage";
    public string endMethodName = "OnAttackEnd";
    public bool dealDamageOnExit = false;
    public bool dealDamageOnEnter = false;
    [Range(0f, 1f)]
    public float damagePoint = 0.5f; // Normalized time in the animation (0-1)
    private bool hasDealtDamage = false;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        hasDealtDamage = false;
        if (dealDamageOnEnter)
        {
            animator.gameObject.SendMessage(methodName, SendMessageOptions.DontRequireReceiver);
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!hasDealtDamage && !dealDamageOnEnter && !dealDamageOnExit)
        {
            // Check if we've reached the damage point in the animation
            if (stateInfo.normalizedTime >= damagePoint)
            {
                animator.gameObject.SendMessage(methodName, SendMessageOptions.DontRequireReceiver);
                hasDealtDamage = true;
            }
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator == null || animator.gameObject == null)
        {
            Debug.LogError("AttackEndDamage: Animator or GameObject is null");
            return;
        }

        Debug.Log("AttackEndDamage: OnStateExit called");
        
        // Call both the damage method and the end method
        if (!string.IsNullOrEmpty(methodName))
        {
            animator.gameObject.SendMessage(methodName, SendMessageOptions.DontRequireReceiver);
        }
        
        if (!string.IsNullOrEmpty(endMethodName))
        {
            animator.gameObject.SendMessage(endMethodName, SendMessageOptions.DontRequireReceiver);
        }
    }
} 