using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// A reusable state machine manager to apply to various objects that requires a state machine.
/// This was referenced from "Programming A BETTER State Machine" by iHeartGameDev on YouTube:
/// https://www.youtube.com/watch?v=qsIiFsddGV4
/// </summary>
/// <typeparam name="EState">The enumerable type representing the possible states</typeparam>
[System.Serializable]
public abstract class StateManager<EState> : NetworkBehaviour where EState : Enum
{
    [Header("State Paramaters")]
    [Tooltip("Associates each state with its corresponding behavior.")]
    [SerializeField] protected List<StateMapping<EState>> StateMappings = new List<StateMapping<EState>>();
    [SerializeField] private EState _defaultState;

    protected Dictionary<EState, BaseState<EState>> States = new Dictionary<EState, BaseState<EState>>();
    protected BaseState<EState> CurrentState;
    
    protected bool _isTransitioningState = false;

    /// <summary>
    /// Enter the current state.
    /// </summary>
    private void Start()
    {
        CurrentState = States[_defaultState];
        CurrentState.EnterState();
    }

    /// <summary>
    /// Trigger the update loop for the current state.
    /// </summary>
    private void Update()
    {
        if(!isServer){return;}
        if (!_isTransitioningState)
        {
            CurrentState.UpdateState();
        }
    }

    /// <summary>
    /// Called when another collider enters the trigger attached to the current gameObject.
    /// </summary>
    /// <param name="otherCollider">The other collider involved in this collision</param>
    private void OnTriggerEnter(Collider otherCollider)
    {
        CurrentState.OnTriggerEnter(otherCollider);
    }

    /// <summary>
    /// Called when another collider exits the trigger attached to the current gameObject.
    /// </summary>
    /// <param name="otherCollider">The other collider involved in this collision</param>
    private void OnTriggerExit(Collider otherCollider)
    {
        CurrentState.OnTriggerExit(otherCollider);
    }

    /// <summary>
    /// Called when another collider continues to enable the trigger attached to the current gameObject.
    /// </summary>
    /// <param name="otherCollider">The other collider involved in this collision</param>
    private void OnTriggerStay(Collider otherCollider)
    {
        CurrentState.OnTriggerStay(otherCollider);
    }

    /// <summary>
    /// Transition from the current state to the given state.
    /// </summary>
    /// <param name="stateKey">The enumerable member to transition to</param>
    protected void TransitionToState(EState stateKey)
    {
        if (!CurrentState.StateKey.Equals(stateKey))
        {
            _isTransitioningState = true;

            CurrentState.ExitState();
            CurrentState = States[stateKey];
            CurrentState.EnterState();

            _isTransitioningState = false;
        }
    }
}
