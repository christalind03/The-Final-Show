using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

[RequireComponent(typeof(FieldOfView), typeof(NavMeshAgent))]
public class EnemyStateMachine : StateManager<EnemyStateMachine.EEnemyState, EnemyState, EnemyContext>
{
    public enum EEnemyState
    {
        Idle,
        Wandering,
        Chasing,
        Aiming,
        Attacking,
        Ability0
    }

    [Header("Attack Parameters")]
    [SerializeField] protected AttackStats _attackStats;

    [Header("Behavior Parameters")]
    [SerializeField] protected BehaviorStats _behaviorStats;

    [SerializeField] protected AudioManager _audioManager;

    protected Vector3 _initialPosition;
    protected Quaternion _initialRotation;

    protected NavMeshAgent _navMeshAgent;
    protected FieldOfView _fieldOfView;

    protected Transform _targetTransform;
    protected bool _hasTarget;

    protected bool _canAttack;
    protected bool _isAttacking;

    protected Material _material;

    private Animator _enemyAnimator;
    [SerializeField] private float _attackAnimLength;
    [SerializeField]
    [Tooltip("The amount of time to wait before dealing damage to account for the wind-up on attack animations.")]
    private float _attackAnimDelay = 0f;

    /// <summary>
    /// Stores the enemy's initial position and rotation for later use.
    /// Additionally caches relevant components from the GameObject and initializes the context shared by concrete states.
    /// </summary>
    protected virtual void Awake()
    {
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;

        _fieldOfView = GetComponent<FieldOfView>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _material = GetComponentsInChildren<Renderer>()[0].material;

        // To access the animator, we must retrieve the child gameObject that is rendering the player's mesh.
        // This should be the first child of the current gameObject
        _enemyAnimator = transform.GetChild(0).GetComponent<Animator>();

        _canAttack = true;
        _hasTarget = false;

        _navMeshAgent.stoppingDistance = _behaviorStats.StartAimDist;

        StateContext = new EnemyContext(this, _audioManager, _attackStats, _behaviorStats, _initialPosition, _initialRotation, transform, _fieldOfView, _navMeshAgent, _enemyAnimator, _material, _attackAnimDelay);
    }

    /// <summary>
    /// Called every frame to control enemy behavior.
    /// </summary>
    protected void FixedUpdate()
    {
        if(!isServer) { return; }  // only execute enemy behavior on the server
        if (!_navMeshAgent.enabled) { return; } // only execute enemy behavior if the NavMeshAgent is enabled

        // _fieldOfView's interested layers should only be player
        
        // First, check if the current target is valid
        CheckTargetValidity();
        // Perform transition logic
        TransitionLogic();
    }

    /// <summary>
    /// Checks if the current target is valid.
    /// A target is no longer valid if the object has been destroyed or if its health has reached zero.
    /// </summary>
    protected void CheckTargetValidity()
    {
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
    }

    /// <summary>
    /// Contains the logic which controls transitioning between states and finding targets.
    /// This behavior is the same for all basic enemies (non-bosses).
    /// </summary>
    protected virtual void TransitionLogic()
    {
        float distToTarget = 0f;
        if (_hasTarget)
        {
            distToTarget = Vector3.Distance(transform.position, _targetTransform.position); // recalculate distToTarget
            // If in default state and target is closer than startChaseDist, start chasing
            if (CurrentState.StateKey.Equals(_defaultState) && distToTarget < _behaviorStats.StartChaseDist)
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
                    _isAttacking = true;
                    StartCoroutine(AttackCooldown());
                    StartCoroutine(AttackAnimationCooldown());
                }
            }
            // If Attacking and the animation is completed, start Aiming
            else if (CurrentState.StateKey.Equals(EEnemyState.Attacking) && !_isAttacking)
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
                //TransitionToState(EEnemyState.Idle);
                TransitionToState(_defaultState);
            }
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

    /// <summary>
    /// Used to ensure that the enemy remains in the attacking state for the duration of the animation
    /// </summary>
    [Server]
    protected IEnumerator AttackAnimationCooldown()
    {
        yield return new WaitForSeconds(_attackAnimLength); // float for duration of attack animation
        _isAttacking = false;
    }
    
}
