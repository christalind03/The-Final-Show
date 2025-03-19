using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Base State/Enemy/Aiming")]
public class AimingState : EnemyState
{
    [SerializeField] private float _rotationSpeed = 1.5f;
    
    public override void EnterState()
    {
        Debug.Log("Entering Aiming State");
        StateContext.Animator.SetBool("Is Aiming", true);
    }

    public override void ExitState()
    {
        Debug.Log("Leaving Aiming State");
    }

    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }
    public override void UpdateState()
    {
        // Keep looking at the target
        Vector3 dir = (StateContext.TargetTransform.position - StateContext.Transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(dir);
        StateContext.Transform.rotation = Quaternion.Slerp(StateContext.Transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        StateContext.Animator.SetFloat("Speed", StateContext.NavMeshAgent.velocity.magnitude);
    }
}