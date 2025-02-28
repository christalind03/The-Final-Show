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
        StateContext.Animator.SetBool("Is Pushing", true);
    }

    public override void ExitState()
    {
        StateContext.Animator.SetBool("Is Pushing", false);
        Debug.Log("Leaving Pushing State");
    }

    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }
    public override void UpdateState()
    {
        // go through FOV's detected objects and use their character controller to move them away each frame
        foreach (GameObject obj in StateContext.FieldOfView.DetectedObjects)
        {
            PlayerController playerController = obj.GetComponent<PlayerController>();
            if (playerController)
            {
                Vector3 dir = (obj.transform.position - StateContext.Transform.position).normalized;
                Vector3 vect = dir * Time.deltaTime * _pushStrength;
                playerController.RpcExternalMove(vect);
            }
        }
    }
}

