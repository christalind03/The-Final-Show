using System;
using UnityEngine;

[CreateAssetMenu(menuName = "BaseState/Enemy/Aiming")]
public class AimingState : EnemyState
{
    public void Initialize(EnemyContext context, EnemyStateMachine.EEnemyState stateKey)
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