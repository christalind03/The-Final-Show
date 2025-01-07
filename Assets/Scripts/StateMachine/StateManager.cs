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
    [Tooltip("Associates each state with its corresponding behavior.")]
    [SerializeField] protected List<StateMapping<EState, TStateContext>> StateMappings;
    [SerializeField] private EState _defaultState;

    protected Dictionary<EState, BaseState<EState, TStateContext>> States;
    protected BaseState<EState, TStateContext> CurrentState;
    protected TStateContext StateContext;
    
    protected bool _isTransitioningState = false;

    /// <summary>
    /// Enter the current state.
    /// </summary>
    private void Start()
    {
        InitializeStates();

        CurrentState = States[_defaultState];
        CurrentState.EnterState();
    }

    // TODO: Document
    private void InitializeStates()
    {
        States = StateMappings.ToDictionary(
            currentState => currentState.Key,
            currentState =>
            {
                // Since we are using Scriptable Objects to enable a "drag-n-drop" behavior in the Unity Inspector,
                // we need to ensure that we create separate instances in order to prevent syncing state behavior.
                EState stateKey = currentState.Key;
                TState stateInstance = (TState)currentState.Value.Clone();
                
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

    // TODO: Document
    public EState RetrieveState()
    {
        return CurrentState.StateKey;
    }

    /// <summary>
    /// Transition from the current state to the given state.
    /// </summary>
    /// <param name="stateKey">The enumerable member to transition to</param>
    public void TransitionToState(EState stateKey)
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
