using System;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Base State/Enemy/Chasing")]
public class ChasingState : EnemyState
{
    private float movementSpeed = 3.5f;

    public override void EnterState()
    {
        Debug.Log("Entering Chasing State");
        StateContext.NavMeshAgent.speed = movementSpeed; // Update speed
        // We want the enemy to stop once the target is within aim distance
        StateContext.NavMeshAgent.stoppingDistance = StateContext.BehaviorStats.StartAimDist;
        StateContext.Animator.SetBool("Is Aiming", false);
        StateContext.AudioManager?.CmdPlay("Chase");
    }

    public override void ExitState() 
    {
        StateContext.AudioManager?.CmdStop("Chase");
        Debug.Log("Leaving Chasing State");
    }
 
    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }
  
    public override void UpdateState() 
    {
        StateContext.NavMeshAgent.destination = StateContext.TargetTransform.position; // If we need to, we can limit this to every x seconds
        StateContext.Animator.SetFloat("Speed", StateContext.NavMeshAgent.velocity.magnitude);
    }
}
