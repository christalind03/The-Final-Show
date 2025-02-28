using System;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Base State/Enemy/Ability/Teleport")]
public class TeleportAbilityState : EnemyState
{
    [SerializeField]
    [Tooltip("The distance from the player that the boss will teleport to")]
    private float _teleportDistance = 5f;
    public override void EnterState()
    {
        Debug.Log("Entering Teleport State");
        // Get a location behind the current target and warp the enemy there
        Vector3 dest = StateContext.TargetTransform.position - _teleportDistance * StateContext.TargetTransform.forward;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(dest, out hit, 1.0f, NavMesh.AllAreas))
        {
            StateContext.NavMeshAgent.Warp(hit.position);
            StateContext.Transform.LookAt(StateContext.TargetTransform);
        }
        else
        {
            Debug.Log("Ninja teleport sample failed");
        }
    }

    public override void ExitState()
    {
        Debug.Log("Leaving Teleport State");
    }

    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }
    public override void UpdateState() { }
}
