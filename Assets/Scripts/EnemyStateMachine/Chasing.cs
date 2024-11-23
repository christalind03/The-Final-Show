using System;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "BaseState/Enemy/Chasing")]
public class ChasingState : EnemyState
{
    public void Initialize(EnemyContext context, EnemyStateMachine.EEnemyState stateKey)
    {
        Context = context;
        base.Initialize(context, stateKey);
    }

    public override void EnterState()
    {
        Debug.Log("Entering Chasing State");
        Context.Material.SetColor("_BaseColor", Color.yellow);
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
        Context.NavMeshAgent.destination = Context.TargetTransform.position;
    }
}
