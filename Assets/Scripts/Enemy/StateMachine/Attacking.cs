using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Base State/Enemy/Attacking")]
public class AttackingState : EnemyState
{
    public override void EnterState()
    {
        Debug.Log("Entering Attacking State");

        // For debugging purposes only.
        StateContext.Material.SetColor("_BaseColor", Color.magenta);

        StateContext.Animator.SetBool("Is Attacking", true);
        //StateContext.Animator.SetBool("Is Aiming", false);
        if (StateContext.TargetTransform.root.TryGetComponent(out AbstractHealth targetHealth))
        {
            targetHealth.CmdDamage(StateContext.AttackStats.AttackDamage);
        }
    }

    public override void ExitState() 
    {
        //StateContext.Animator.SetBool("Is Aiming", true);
        StateContext.Animator.SetBool("Is Attacking", false);
        Debug.Log("Leaving Attacking State");
    }

    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }
    public override void UpdateState() { }
}
