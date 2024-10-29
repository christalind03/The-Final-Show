using System;
using UnityEngine;

public abstract class FlyingEnemyState : BaseState<FlyingEnemyStateMachine.EEnemyState>
{
    protected FlyingEnemyContext Context;

    public void Initialize(FlyingEnemyContext context, FlyingEnemyStateMachine.EEnemyState stateKey)
    {
        Context = context;
        base.Initialize(stateKey);
    }
}
