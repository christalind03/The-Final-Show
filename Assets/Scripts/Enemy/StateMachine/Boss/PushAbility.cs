using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Base State/Enemy/Ability/Push")]
public class PushAbilityState : EnemyState
{
    [SerializeField] private float _pushStrength = 1.5f;
    public override void EnterState()
    {
        Debug.Log("Entering Pushing State");
        StateContext.NavMeshAgent.destination = StateContext.Transform.position; // Stop moving to use ability
    }

    public override void ExitState()
    {
        Debug.Log("Leaving Pushing State");
    }

    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }
    public override void UpdateState()
    {
        // go through FOV's detected objects and use their character controller to move them away each frame
        foreach (var obj in StateContext.FieldOfView.DetectedObjects)
        {
            
            CharacterController targetController = obj.GetComponent<CharacterController>();
            if (targetController)
            {
                Vector3 dir = (obj.transform.position - StateContext.Transform.position).normalized;
                targetController.Move(dir * Time.deltaTime * _pushStrength);
            }
        }
    }
}

