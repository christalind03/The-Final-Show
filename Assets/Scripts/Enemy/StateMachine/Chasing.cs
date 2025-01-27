using System;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Base State/Enemy/Chasing")]
public class ChasingState : EnemyState
{
    public override void EnterState()
    {
        Debug.Log("Entering Chasing State");

        // For debugging purposes only.
        StateContext.Material.SetColor("_BaseColor", Color.yellow);
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
        StateContext.NavMeshAgent.destination = StateContext.TargetTransform.position;
    }
}
