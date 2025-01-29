using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

[RequireComponent(typeof(FieldOfView), typeof(UnityEngine.AI.NavMeshAgent))]
public class PugStateMachine : BossStateMachine
{
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

}
