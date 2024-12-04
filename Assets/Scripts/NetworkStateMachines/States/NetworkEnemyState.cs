using System;
using UnityEngine;

public abstract class NetworkEnemyState : BaseState<NetworkEnemyStateMachine.EEnemyState>
{
    protected EnemyContext Context;

    public void Initialize(EnemyContext context, NetworkEnemyStateMachine.EEnemyState stateKey)
    {
        Context = context;
        base.Initialize(stateKey);
    }
}
