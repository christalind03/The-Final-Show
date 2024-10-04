using System;
using UnityEngine;

/// <summary>
/// A reusable state machine manager to apply to various objects that requires a state machine.
/// This was referenced from "Programming A BETTER State Machine" by iHeartGameDev on YouTube:
/// https://www.youtube.com/watch?v=qsIiFsddGV4
/// </summary>
/// <typeparam name="EState">The enumerable type representing the possible states</typeparam>
public abstract class BaseState<EState> where EState : Enum
{
    public EState StateKey {  get; set; }

    public BaseState(EState stateKey)
    {
        StateKey = stateKey;
    }

    public abstract EState GetNextState();
    public abstract void EnterState();
    public abstract void ExitState();
    public abstract void OnTriggerEnter(Collider otherCollider);
    public abstract void OnTriggerExit(Collider otherCollider);
    public abstract void OnTriggerStay(Collider otherCollider);
    public abstract void UpdateState();
}
