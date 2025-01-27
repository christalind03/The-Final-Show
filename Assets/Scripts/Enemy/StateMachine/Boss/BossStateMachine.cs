using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

[RequireComponent(typeof(FieldOfView), typeof(NavMeshAgent))]
public class BossStateMachine : EnemyStateMachine
{
    // has all attributes of EnemyStateMachine PLUS
    [Header("Ability Parameters")]
    [SerializeField] protected List<AbilityStats> _abilityStats;
    protected List<bool> _canUseAbility;
    protected bool _usingAbility;


    /// <summary>
    /// Calls base.Awake() then initializes the _canUseAbility list with true
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        _canUseAbility = new List<bool>();
        int numAbilities = _abilityStats.Count;
        for (int i = 0; i < numAbilities; i++)
        {
            _canUseAbility.Add(true);
        }
        _usingAbility = false;
    }


    protected override void TransitionLogic()
    {
        float distToTarget = 0f;
        // if we're in the middle of using an ability, do nothing
        if (_usingAbility)
        {
            return;
        }
        // if we're not using an ability anymore but we're still in the ability state
        else if (CurrentState.StateKey.Equals(EEnemyState.Ability0))
        {
            // If we still have a target, start Chasing
            if (_hasTarget)
            {
                TransitionToState(EEnemyState.Chasing);
            }
            // otherwise, go Idle
            else
            {
                TransitionToState(EEnemyState.Idle);
            }
        }
        // use push ability if able and there are players in the FOV
        if (_canUseAbility[0] && _fieldOfView.DetectedObjects.Count > 0)
        {
            TransitionToState(EEnemyState.Ability0);
            _canUseAbility[0] = false;
            _usingAbility = true;
            StartCoroutine(Ability0Cooldown());
            StartCoroutine(Ability0Duration());
        }
        else if (_hasTarget)
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
    }

    /// <summary>
    /// Used to control how often the boss can use ability 1
    /// </summary>
    [Server]
    protected IEnumerator Ability0Cooldown()
    {
        yield return new WaitForSeconds(_abilityStats[0].AbilityCooldown);
        _canUseAbility[0] = true;
    }

    [Server]
    protected IEnumerator Ability0Duration()
    {
        yield return new WaitForSeconds(_abilityStats[0].AbilityDuration);
        _usingAbility = false;
    }

}
