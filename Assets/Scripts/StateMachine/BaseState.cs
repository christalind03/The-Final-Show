using System;
using UnityEngine;

/// <summary>
/// A reusable state machine manager to apply to various objects that requires a state machine.
/// This was referenced from "Programming A BETTER State Machine" by iHeartGameDev on YouTube:
/// https://www.youtube.com/watch?v=qsIiFsddGV4
/// </summary>
/// <typeparam name="EState">The enumerable type representing the possible states</typeparam>
public abstract class BaseState<EState> : ScriptableObject where EState : Enum
{
    public EState StateKey {  get; set; }

    public void Initialize(EState stateKey)
    {
        StateKey = stateKey;
    }

    /// <summary>
    /// Logic to be executed upon entering this state.
    /// </summary>
    public abstract void EnterState();

    /// <summary>
    /// Logic to be executed upon exiting this state.
    /// </summary>
    public abstract void ExitState();

    /// <summary>
    /// Called when another collider enters the trigger attached to the current gameObject.
    /// </summary>
    /// <param name="otherCollider">The other collider involved in this collision</param>
    public abstract void OnTriggerEnter(Collider otherCollider);

    /// <summary>
    /// Called when another collider exits the trigger attached to the current gameObject.
    /// </summary>
    /// <param name="otherCollider">The other collider involved in this collision</param>
    public abstract void OnTriggerExit(Collider otherCollider);

    /// <summary>
    /// Called when another collider continues to enable the trigger attached to the current gameObject.
    /// </summary>
    /// <param name="otherCollider">The other collider involved in this collision</param>
    public abstract void OnTriggerStay(Collider otherCollider);

    /// <summary>
    /// Logic to be executed every frame while in this state.
    /// </summary>
    public abstract void UpdateState();
}
