using System;
using UnityEngine;

[CreateAssetMenu(menuName = "BaseState/FlyingEnemy/Attacking")]
public class AttackingState : FlyingEnemyState
{
    public void Initialize(FlyingEnemyContext context, FlyingEnemyStateMachine.EEnemyState stateKey)
    {
        Context = context;
        base.Initialize(context, stateKey);
    }

    public override void EnterState()
    {
        Debug.Log("Entering Attacking State");
        Context.NavMeshAgent.destination = Context.Transform.position;
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
