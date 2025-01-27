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

    protected bool _canAbility1;
    protected bool _usingAbility;


    /// <summary>
    /// Stores the enemy's initial position and rotation for later use
    /// Gets relevant components from the GameObject
    /// Initializes the context shared by concrete states
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        _canAbility1 = true;
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
        else if (CurrentState.StateKey.Equals(EEnemyState.Ability1))
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
        if (_canAbility1 && _fieldOfView.DetectedObjects.Count > 0)
        {
            TransitionToState(EEnemyState.Ability1);
            _canAbility1 = false;
            _usingAbility = true;
            StartCoroutine(Ability1Cooldown());
            StartCoroutine(Ability1Duration());
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
    protected IEnumerator Ability1Cooldown()
    {
        yield return new WaitForSeconds(_abilityStats[0].AbilityCooldown);
        _canAbility1 = true;
    }

    [Server]
    protected IEnumerator Ability1Duration()
    {
        yield return new WaitForSeconds(_abilityStats[0].AbilityDuration);
        _usingAbility = false;
    }

}
