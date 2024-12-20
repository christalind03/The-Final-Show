using System;
using UnityEngine;

[CreateAssetMenu(menuName = "BaseState/Enemy/Attacking")]
public class AttackingState : EnemyState
{
    public override void Initialize(EnemyContext context, EnemyStateMachine.EEnemyState stateKey)
    {
        Context = context;
        base.Initialize(context, stateKey);
    }

    public override void EnterState()
    {
        Debug.Log("Entering Attacking State");
        Context.Material.SetColor("_BaseColor", Color.magenta);
        // Deal damage
        if (Context.TargetTransform.root.TryGetComponent(out Health targetHealth))
        {
            targetHealth.CmdRemoveHealth(Context.AttackStats.AttackDamage);
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
