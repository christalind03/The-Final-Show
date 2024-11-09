using System;
using UnityEngine;

public abstract class RangedEnemyState : BaseState<RangedEnemyStateMachine.EEnemyState>
{
    protected RangedEnemyContext Context;

    public void Initialize(RangedEnemyContext context, RangedEnemyStateMachine.EEnemyState stateKey)
    {
        Context = context;
        base.Initialize(stateKey);
    }
}
