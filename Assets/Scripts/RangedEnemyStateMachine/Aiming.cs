using System;
using UnityEngine;

[CreateAssetMenu(menuName = "BaseState/RangedEnemy/Aiming")]
public class AimingState : RangedEnemyState
{
    public void Initialize(RangedEnemyContext context, RangedEnemyStateMachine.EEnemyState stateKey)
    {
        Context = context;
        base.Initialize(context, stateKey);
    }

    public override void EnterState()
    {
        Debug.Log("Entering Aiming State");
        Context.Material.SetColor("_BaseColor", Color.red);
        Context.NavMeshAgent.destination = Context.Transform.position;
    }

    public override void ExitState()
    {
        Debug.Log("Leaving Aiming State");
    }
    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }
    public override void UpdateState() { }
}