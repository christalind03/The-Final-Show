using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(FieldOfView), typeof(NavMeshAgent))]
public class FlyingEnemyStateMachine : StateManager<FlyingEnemyStateMachine.EEnemyState>
{
    public enum EEnemyState
    {
        Idle,
        Chasing,
        Aiming,
        Attacking
    }
    
    [SerializeField] private SphereCollider _spherecollider;

    [Header("Attack Parameters")]
    [SerializeField] protected float _attackCooldown;
    [SerializeField] protected float _attackDamage;
    [SerializeField] protected float _attackRange;

    [Header("Behavior Parameters")]
    [Tooltip("Distance at which the enemy will stop chasing the target and return to idle. Should be no less than the object's FieldOfView distance.")]
    [SerializeField] protected float _endChaseDist;
    [Tooltip("Distance at which the enemy will start chasing the target from the idle state. Should be no greater than the object's FieldOfView distance.")]
    [SerializeField] protected float _startChaseDist;
    [Tooltip("Distance at which the enemy will stop aiming at the target and resume chasing. Should be no less than Start Aim Dist")]
    [SerializeField] protected float _endAimDist;
    [Tooltip("Distance at which the enemy will start aiming at the target from the chasing state. Should be no greater than Start Chase Dist.")]
    [SerializeField] protected float _startAimDist;
    

    protected Vector3 _initialPosition;
    protected Quaternion _initialRotation;

    protected NavMeshAgent _navMeshAgent;
    protected FieldOfView _fieldOfView;

    protected Transform _targetTransform;
    protected bool _hasTarget;

    protected FlyingEnemyContext _context;
    protected bool _canAttack;

    
    private void Awake()
    {
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;

        _navMeshAgent = GetComponent<NavMeshAgent>();
        _fieldOfView = GetComponent<FieldOfView>();

        _context = new FlyingEnemyContext(_attackDamage, _startChaseDist, _endChaseDist, _startAimDist, _endAimDist, 
            _initialPosition, _initialRotation, transform, _fieldOfView, _navMeshAgent);
        _canAttack = true;

        InitializeStates();
    }

    private void InitializeStates()
    {
        foreach (StateMapping<FlyingEnemyStateMachine.EEnemyState> stateMapping in StateMappings)
        {
            FlyingEnemyStateMachine.EEnemyState stateMappingKey = stateMapping.Key;
            BaseState<FlyingEnemyStateMachine.EEnemyState> stateMappingValue = stateMapping.Value;

            var stateInstance = (FlyingEnemyState)ScriptableObject.CreateInstance(stateMappingValue.GetType());
            stateInstance.Initialize(_context, stateMappingKey);

            States.Add(stateMappingKey, stateInstance);
        }
    }

    private void FixedUpdate()
    {
        // _fieldOfView's interested layers should only be player
        float distToTarget = 0f;
        // When a target is within the FOV
        if (0 < _fieldOfView.DetectedObjects.Count)
        {
            // Choose one target, keep that target until it leaves chase radius
            // If a different target is within FOV, switch to that target
            _hasTarget = true;
            _targetTransform = _fieldOfView.DetectedObjects[0].transform; // get the first object
            _context.TargetTransform = _targetTransform;
            distToTarget = Vector3.Distance(transform.position, _targetTransform.position);
            Vector3 dir = (_targetTransform.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            float rotationSpeed = 1.5f; // initialize this value somewhere else
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // If Idle and target is within chase distance, start Chasing
            if (CurrentState.StateKey.Equals(EEnemyState.Idle) && distToTarget < _startChaseDist)
            {
                TransitionToState(EEnemyState.Chasing);
            }
            // If Chasing and target is within aim range, start Aiming
            else if (CurrentState.StateKey.Equals(EEnemyState.Chasing) && distToTarget < _startAimDist)
            {
                TransitionToState(EEnemyState.Aiming);
            }
            // If Aiming and target is out of aim range, start Chasing
            else if (CurrentState.StateKey.Equals(EEnemyState.Aiming) && distToTarget > _endAimDist)
            {
                TransitionToState(EEnemyState.Chasing);
            }
            else if (CurrentState.StateKey.Equals(EEnemyState.Aiming) && _canAttack)
            {
                TransitionToState(EEnemyState.Attacking);
                _canAttack = false;
                StartCoroutine(AttackCooldown());
            }
            else if (CurrentState.StateKey.Equals(EEnemyState.Attacking) && !_canAttack)
            {
                TransitionToState(EEnemyState.Aiming);
            }
        }
        // When a target is not within the FOV
        // e.g. still within chase distance or behind obstacle
        else if(_hasTarget)
        {
            distToTarget = Vector3.Distance(transform.position, _targetTransform.position); // recalculate distToTarget
            // If target is farther than endChaseDist, stop chasing, no longer have a target
            if (CurrentState.StateKey.Equals(EEnemyState.Chasing) && distToTarget > _endChaseDist)
            {
                _hasTarget = false;
                TransitionToState(EEnemyState.Idle);
            }
            // Target MUST be within FOV (and unobstructed) to aim
            else if (CurrentState.StateKey.Equals(EEnemyState.Aiming))
            {
                TransitionToState(EEnemyState.Chasing);
            }
        }
    }

    
    protected IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(_attackCooldown);
        _canAttack = true;
    }
    
}
