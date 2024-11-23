using System;
using UnityEngine;

public abstract class EnemyState : BaseState<EnemyStateMachine.EEnemyState>
{
    protected EnemyContext Context;

    public void Initialize(EnemyContext context, EnemyStateMachine.EEnemyState stateKey)
    {
        Context = context;
        base.Initialize(stateKey);
    }
}
