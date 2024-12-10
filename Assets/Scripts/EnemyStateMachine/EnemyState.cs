using System;
using UnityEngine;

public abstract class EnemyState : BaseState<EnemyStateMachine.EEnemyState>
{
    protected EnemyContext Context;

    /// <summary>
    /// Initializes the state with context, which is shared by the derived concrete states.
    /// </summary>
    /// <param name="context">The EnemyContext to be shared among concrete states.</param>
    /// <param name="stateKey">The enum value corresponding to the state.</param>
    public virtual void Initialize(EnemyContext context, EnemyStateMachine.EEnemyState stateKey)
    {
        Context = context;
        base.Initialize(stateKey);
    }
}
