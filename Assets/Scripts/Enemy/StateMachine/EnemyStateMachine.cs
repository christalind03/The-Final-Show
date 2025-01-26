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
        Attacking,
        Ability1
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
    protected virtual void Awake()
    {
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;

        _navMeshAgent = GetComponent<NavMeshAgent>();
        _fieldOfView = GetComponent<FieldOfView>();
        _material = GetComponentsInChildren<Renderer>()[0].material;

        _canAttack = true;
        _hasTarget = false;

        StateContext = new EnemyContext(_attackStats, _initialPosition, _initialRotation, transform, _fieldOfView, _navMeshAgent, _material);
    }

    /// <summary>
    /// Contains logic for keeping track of the target and transitioning between states
    /// </summary>
    protected virtual void FixedUpdate()
    {
        // TODO: Do we need if(!isServer) { return }; here?
        // _fieldOfView's interested layers should only be player
        float distToTarget = 0f;

        // First, check if the current target is valid
        if (_hasTarget)
        {
            // Check if the target has been destroyed (e.g. a player disconnects)
            if (StateContext.TargetTransform == null)
            {
                _hasTarget = false;
            }
            // Check if the target has a health component
            else if (StateContext.TargetTransform.root.TryGetComponent(out AbstractHealth targetHealth))
            {
                // Check if the target's health has reached 0
                if (targetHealth.CurrentValue <= 0f)
                {
                    _hasTarget = false;
                }
            }
        }
        // If we have a valid target, perform transition logic
        if (_hasTarget)
        {
            distToTarget = Vector3.Distance(transform.position, _targetTransform.position); // recalculate distToTarget
            // If Idle and target is closer than startChaseDist, start chasing
            if (CurrentState.StateKey.Equals(EEnemyState.Idle) && distToTarget < _behaviorStats.StartChaseDist)
            {
                TransitionToState(EEnemyState.Chasing);
            }
            // If Chasing...
            else if (CurrentState.StateKey.Equals(EEnemyState.Chasing))
            {
                // and target is farther than endChaseDist, stop chasing, no longer have a target
                if (distToTarget > _behaviorStats.EndChaseDist)
                {
                    _hasTarget = false;
                }
                // and target is closer than startAimDist, start Aiming
                else if (distToTarget < _behaviorStats.StartAimDist)
                {
                    TransitionToState(EEnemyState.Aiming);
                }
            }
            // If Aiming...
            else if (CurrentState.StateKey.Equals(EEnemyState.Aiming))
            {
                // and target is farther than endAimDist, start Chasing
                if (CurrentState.StateKey.Equals(EEnemyState.Aiming) && distToTarget > _behaviorStats.EndAimDist)
                {
                    TransitionToState(EEnemyState.Chasing);
                }
                // and can attack, Attack
                else if (CurrentState.StateKey.Equals(EEnemyState.Aiming) && _canAttack)
                {
                    TransitionToState(EEnemyState.Attacking);
                    _canAttack = false;
                    StartCoroutine(AttackCooldown());
                    // TODO: add a delay here for duration of attack animation
                }
            }
            // If Attacking but can no longer attack, start Aiming
            else if (CurrentState.StateKey.Equals(EEnemyState.Attacking) && !_canAttack)
            {
                TransitionToState(EEnemyState.Aiming);
            }
        }
        // If we don't have a target anymore, scan for a new one
        if (!_hasTarget)
        {
            if (_fieldOfView.DetectedObjects.Count > 0)
            {
                _hasTarget = true;
                // TODO: The first object in the list seems to always be the host, so maybe find the closest obj in the list
                _targetTransform = _fieldOfView.DetectedObjects[0].transform; // get the first object
                StateContext.TargetTransform = _targetTransform;
            }
            // No target and couldn't find a new one -> go to Idle
            else
            {
                TransitionToState(EEnemyState.Idle);
            }
        }
        return;
        
        // ORIGINAL LOGIC
        
        // if the current target has been destroyed (e.g. a player disconnects), go to idle
        if (_hasTarget && StateContext.TargetTransform == null)
        {
            TransitionToState(EEnemyState.Idle); // TODO: get rid of this transition, and transition based on next logic
            _hasTarget = false;
        }
        // if the current target has no health, go to idle
        else if (_hasTarget && StateContext.TargetTransform.root.TryGetComponent(out AbstractHealth targetHealth))
        {
            if (targetHealth.CurrentValue <= 0)
            {
                TransitionToState(EEnemyState.Idle); // TODO: get rid of this transition, and transition based on next logic
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
