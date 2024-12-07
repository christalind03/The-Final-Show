using System;
using UnityEngine;

[CreateAssetMenu(menuName = "BaseState/Enemy/Idle")]
public class IdleState : EnemyState
{
    [SerializeField] private float _rotationSpeed = 1.5f;


    private bool _hasTargetRotation;
    private bool _isReset;
    
    public override void Initialize(EnemyContext context, EnemyStateMachine.EEnemyState stateKey)
    {
        Context = context;
        base.Initialize(context, stateKey);

        _isReset = true;
    }

    public override void EnterState()
    {
        Debug.Log("Entering Idle State");
        Context.Material.SetColor("_BaseColor", Color.green);
        bool hasInitialPosition = Context.InitialPosition == Context.Transform.position;
        bool hasInitialRotation = Context.InitialRotation == Context.Transform.rotation;

        _isReset = hasInitialPosition && hasInitialRotation;

        Context.NavMeshAgent.SetDestination(Context.InitialPosition);
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
            if (Vector3.Distance(Context.InitialPosition, Context.Transform.position) < 0.1f)
            {
                Context.Transform.rotation = Quaternion.Slerp(Context.Transform.rotation, Context.InitialRotation, _rotationSpeed * Time.deltaTime);
            }
            
            if (Context.InitialRotation == Context.Transform.rotation)
            {
                _isReset = true;
            }
        }
    }
}
