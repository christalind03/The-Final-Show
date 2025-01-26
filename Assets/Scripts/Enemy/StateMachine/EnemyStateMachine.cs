using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

[RequireComponent(typeof(FieldOfView), typeof(NavMeshAgent))]
public class EnemyStateMachine : StateManager<EnemyStateMachine.EEnemyState, EnemyState, EnemyContext>
{
    public enum EEnemyState
    {
        Idle,
        Chasing,
        Aiming,
        Attacking
    }

    [Header("Attack Parameters")]
    [SerializeField] protected AttackStats _attackStats;

    [Header("Behavior Parameters")]
    [SerializeField] protected BehaviorStats _behaviorStats;

    protected Vector3 _initialPosition;
    protected Quaternion _initialRotation;

    protected NavMeshAgent _navMeshAgent;
    protected FieldOfView _fieldOfView;

    protected Transform _targetTransform;
    protected bool _hasTarget;

    protected bool _canAttack;

    protected Material _material;

    /// <summary>
    /// Stores the enemy's initial position and rotation for later use
    /// Gets relevant components from the GameObject
    /// Initializes the context shared by concrete states
    /// </summary>
    private void Awake()
    {
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;

        _navMeshAgent = GetComponent<NavMeshAgent>();
        _fieldOfView = GetComponent<FieldOfView>();
        _material = GetComponentsInChildren<Renderer>()[0].material;

        _canAttack = true;

        StateContext = new EnemyContext(_attackStats, _initialPosition, _initialRotation, transform, _fieldOfView, _navMeshAgent, _material);
    }

    /// <summary>
    /// Contains logic for keeping track of the target and transitioning between states
    /// </summary>
    private void FixedUpdate()
    {
        if (!_navMeshAgent.enabled) { return; }

        // _fieldOfView's interested layers should only be player
        float distToTarget = 0f;
        // if the current target has been destroyed, go to idle
        if (_hasTarget && StateContext.TargetTransform == null)
        {
            TransitionToState(EEnemyState.Idle);
            _hasTarget = false;
        }
        // if the current target has no health, go to idle
        
        else if (_hasTarget && StateContext.TargetTransform.root.TryGetComponent(out AbstractHealth targetHealth))
        {
            if (targetHealth.CurrentValue <= 0)
            {
                TransitionToState(EEnemyState.Idle);
                _hasTarget = false;
            }
        }
        // When a target is within the FOV
        if (0 < _fieldOfView.DetectedObjects.Count)
        {
            // Choose one target, keep that target until it leaves chase radius
            // If a different target is within FOV, switch to that target
            _hasTarget = true;
            _targetTransform = _fieldOfView.DetectedObjects[0].transform; // get the first object
            StateContext.TargetTransform = _targetTransform;
            distToTarget = Vector3.Distance(transform.position, _targetTransform.position);

            // If Idle and target is within chase distance, start Chasing
            if (CurrentState.StateKey.Equals(EEnemyState.Idle) && distToTarget < _behaviorStats.StartChaseDist)
            {
                TransitionToState(EEnemyState.Chasing);
            }
            // If Chasing and target is within aim range, start Aiming
            else if (CurrentState.StateKey.Equals(EEnemyState.Chasing) && distToTarget < _behaviorStats.StartAimDist)
            {
                TransitionToState(EEnemyState.Aiming);
            }
            // If Aiming and target is out of aim range, start Chasing
            else if (CurrentState.StateKey.Equals(EEnemyState.Aiming) && distToTarget > _behaviorStats.EndAimDist)
            {
                TransitionToState(EEnemyState.Chasing);
            }
            // If Aiming and can attack, Attack
            else if (CurrentState.StateKey.Equals(EEnemyState.Aiming) && _canAttack)
            {
                TransitionToState(EEnemyState.Attacking);
                _canAttack = false;
                StartCoroutine(AttackCooldown());
            }
            // If Attacking but can no longer attack, start Aiming
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
            if (CurrentState.StateKey.Equals(EEnemyState.Chasing) && distToTarget > _behaviorStats.EndChaseDist)
            {
                _hasTarget = false;
                TransitionToState(EEnemyState.Idle);
            }
            // Target MUST be within FOV (and unobstructed) to aim
            else if (CurrentState.StateKey.Equals(EEnemyState.Aiming))
            {
                TransitionToState(EEnemyState.Chasing);
            }
            // Target is not in FOV, but Attacking
            else if (CurrentState.StateKey.Equals(EEnemyState.Attacking))
            {
                TransitionToState(EEnemyState.Aiming);
            }
        }
        // Abnormal behavior: go to Idle
        else
        {
            TransitionToState(EEnemyState.Idle);
        }
    }

    /// <summary>
    /// Used to control how often the enemy can attack
    /// </summary>
    [Server]
    protected IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(_attackStats.AttackCooldown);
        _canAttack = true;
    }
    
}
