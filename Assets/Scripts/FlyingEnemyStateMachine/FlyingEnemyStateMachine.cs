using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class FlyingEnemyStateMachine : StateManager<FlyingEnemyStateMachine.EEnemyState>
{
    public enum EEnemyState
    {
        Idle,
        Chasing,
        Attacking
    }

    public Transform _playerTransform;

    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private SphereCollider _spherecollider;

    [Header("Attack Parameters")]
    [SerializeField] protected float _attackCooldown;
    [SerializeField] protected float _attackDamage;
    [SerializeField] protected float _attackRange;

    [Header("Behavior parameters")]
    [SerializeField] protected float _startChaseDist;
    [SerializeField] protected float _endChaseDist;
    [SerializeField] protected float _startAttackDist;
    [SerializeField] protected float _endAttackDist;

    protected Vector3 _initialPosition;
    protected Quaternion _initialRotation;

    protected NavMeshAgent _navMeshAgent;

    protected FlyingEnemyContext _context;
    protected bool _canAttack;

    
    private void Awake()
    {
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;

        _navMeshAgent = GetComponent<NavMeshAgent>();

        _context = new FlyingEnemyContext(_attackDamage, _startChaseDist, _endChaseDist, _startAttackDist, _endAttackDist, 
            _initialPosition, _initialRotation, transform, _rigidbody, _navMeshAgent, _playerTransform);
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
        float distToPlayer = Vector3.Distance(_playerTransform.position, transform.position);
        if (CurrentState.StateKey.Equals(EEnemyState.Idle) && distToPlayer < _startChaseDist)
        {
            TransitionToState(EEnemyState.Chasing);
        }
        else if (CurrentState.StateKey.Equals(EEnemyState.Chasing) && distToPlayer > _endChaseDist)
        {
            TransitionToState(EEnemyState.Idle);
        }
        else if (CurrentState.StateKey.Equals(EEnemyState.Chasing) && distToPlayer < _startAttackDist)
        {
            TransitionToState(EEnemyState.Attacking);
        }
        else if (CurrentState.StateKey.Equals(EEnemyState.Attacking) && distToPlayer > _endAttackDist)
        {
            TransitionToState(EEnemyState.Chasing);
        }
    }

    /*
    protected IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(_attackCooldown);
        _canAttack = true;
    }
    */
}
