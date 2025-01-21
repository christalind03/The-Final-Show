using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Base State/Enemy/Aiming")]
public class AimingState : EnemyState
{
    public override void EnterState()
    {
        Debug.Log("Entering Aiming State");

        // For debugging purposes only.
        StateContext.Material.SetColor("_BaseColor", Color.red);
        
        StateContext.NavMeshAgent.destination = StateContext.Transform.position;
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