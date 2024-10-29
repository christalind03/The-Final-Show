using System;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "BaseState/FlyingEnemy/Chasing")]
public class ChasingState : FlyingEnemyState
{
    public void Initialize(FlyingEnemyContext context, FlyingEnemyStateMachine.EEnemyState stateKey)
    {
        Context = context;
        base.Initialize(context, stateKey);
    }

    public override void EnterState()
    {
        Debug.Log("Entering Chasing State");
    }

    public override void ExitState() 
    {
        Debug.Log("Leaving Chasing State");
    }
    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }
    public override void UpdateState() 
    {
        Context.NavMeshAgent.destination = Context.PlayerTransform.position;
    }
}
