using System;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Base State/Enemy/Idle")]
public class IdleState : EnemyState
{
    [SerializeField] private float _rotationSpeed = 1.5f;

    private bool _hasTargetRotation;
    private bool _isReset;
    private bool hasInitialPosition;
    private bool hasInitialRotation;


    public override void EnterState()
    {
        Debug.Log("Entering Idle State");

        // For debugging purposes only.
        StateContext.Material.SetColor("_BaseColor", Color.green);
        hasInitialPosition = StateContext.InitialPosition == StateContext.Transform.position;
        hasInitialRotation = StateContext.InitialRotation == StateContext.Transform.rotation;

        _isReset = hasInitialPosition && hasInitialRotation;

        StateContext.NavMeshAgent.SetDestination(StateContext.InitialPosition);
        StateContext.NavMeshAgent.stoppingDistance = 0; // Allows the enemy to return to exact start position
    }

    public override void ExitState() 
    {
        Debug.Log("Leaving Idle State");
    }

    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }

    public override void UpdateState() 
    {
        if (!_isReset)
        {
            // Rotate enemy back to original rotation once it's close to initial position
            // TODO: still bugged, won't always rotate back
            if (StateContext.NavMeshAgent.remainingDistance < 0.1f)
            {
                StateContext.Transform.rotation = Quaternion.Slerp(StateContext.Transform.rotation, StateContext.InitialRotation, _rotationSpeed * Time.deltaTime);
            }

            if (StateContext.InitialRotation == StateContext.Transform.rotation)
            {
                _isReset = true;
            }
        }
        StateContext.Animator.SetFloat("Speed", StateContext.NavMeshAgent.velocity.magnitude);
    }
}
