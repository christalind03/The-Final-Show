using System;
using UnityEngine;

[CreateAssetMenu(menuName = "BaseState/Enemy/Attacking")]
public class AttackingState : EnemyState
{
    public override void EnterState()
    {
        Debug.Log("Entering Attacking State");

        // For debugging purposes only.
        StateContext.Material.SetColor("_BaseColor", Color.magenta);
        
        if (StateContext.TargetTransform.root.TryGetComponent(out AbstractHealth targetHealth))
        {
            targetHealth.CmdDamage(StateContext.AttackStats.AttackDamage);
        }
    }

    public override void ExitState() 
    {
        Debug.Log("Leaving Attacking State");
    }

    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }
    public override void UpdateState() { }
}
