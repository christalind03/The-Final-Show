using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

/// <summary>
/// A reusable state machine manager to apply to various objects that requires a state machine.
/// This was referenced from "Programming A BETTER State Machine" by iHeartGameDev on YouTube:
/// https://www.youtube.com/watch?v=qsIiFsddGV4
/// </summary>
/// <typeparam name="EState">The enumerable type representing the possible states</typeparam>
/// <typeparam name="TState">The type representing the state behavior</typeparam>
/// <typeparam name="TStateContext">The context type representing the shared data or dependencies provided to states</typeparam>
[System.Serializable]
public abstract class StateManager<EState, TState, TStateContext> : NetworkBehaviour
    where EState : Enum
    where TState : BaseState<EState, TStateContext>
{
    [Header("State Paramaters")]
    [SerializeField] private bool _cloneStates;
    [SerializeField] private EState _defaultState;

    [Tooltip("Associates each state with its corresponding behavior.")]
    [SerializeField] protected List<StateMapping<EState, TStateContext>> StateMappings;

    protected Dictionary<EState, BaseState<EState, TStateContext>> States;
    protected BaseState<EState, TStateContext> CurrentState;
    protected TStateContext StateContext;
    
    protected bool _isTransitioningState = false;

    /// <summary>
    /// Enter the current state.
    /// </summary>
    protected virtual void Start()
    {
        if (!isServer) { return; }
        
        InitializeStates();

        CurrentState = States[_defaultState];
        CurrentState.EnterState();
    }

    /// <summary>
    /// Initializes the states by creating instances of the state objects based on the provided mappings.
    /// This ensures that separate instances are created for each state to prevent shared behavior across states.
    /// </summary>
    /// <remarks>Is this *really* needed?</remarks>
    private void InitializeStates()
    {
        States = StateMappings.ToDictionary(
            currentState => currentState.Key,
            currentState =>
            {
                // Since we are using Scriptable Objects to enable a "drag-n-drop" behavior in the Unity Inspector,
                // we need to ensure that we create separate instances in order to prevent syncing state behavior.
                EState stateKey = currentState.Key;
                TState stateInstance = (TState)(_cloneStates ? currentState.Value.Clone() : currentState.Value);
                
                stateInstance.Initialize(stateKey, StateContext);

                return (BaseState<EState, TStateContext>)stateInstance;
            }
        );
    }

    /// <summary>
    /// Trigger the update loop for the current state.
    /// </summary>
    private void Update()
    {
        if (!isServer) { return; }
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
        if (!isServer) { return; }
        CurrentState.OnTriggerEnter(otherCollider);
    }

    /// <summary>
    /// Called when another collider exits the trigger attached to the current gameObject.
    /// </summary>
    /// <param name="otherCollider">The other collider involved in this collision</param>
    private void OnTriggerExit(Collider otherCollider)
    {
        if (!isServer) { return; }
        CurrentState.OnTriggerExit(otherCollider);
    }

    /// <summary>
    /// Called when another collider continues to enable the trigger attached to the current gameObject.
    /// </summary>
    /// <param name="otherCollider">The other collider involved in this collision</param>
    private void OnTriggerStay(Collider otherCollider)
    {
        if (!isServer) { return; }
        CurrentState.OnTriggerStay(otherCollider);
    }

    /// <summary>
    /// Transition from the current state to the given state.
    /// </summary>
    /// <param name="stateKey">The enumerable member to transition to</param>
    public void TransitionToState(EState stateKey)
    {
        if (!isServer) { return; }
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
