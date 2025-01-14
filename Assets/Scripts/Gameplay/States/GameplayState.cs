using Mirror;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GameplayState : BaseState<GameplayManager.State, GameplayContext>
{
    [Scene]
    [SerializeField]
    [Tooltip("The scene associated with the Gameplay State")]
    protected string TargetScene;

    [Header("Countdown Properties")]
    [SerializeField] protected bool IsTimed;
    [SerializeField] protected CountdownMessage CountdownMessage;
    
    [SerializeField]
    [Tooltip("The state to transition to when the countdown is complete")]
    protected GameplayManager.State TransitionState;

    // TODO: Documentation
    public override void EnterState()
    {
        StateContext.GameplayManager.BroadcastScene(TargetScene);

        if (IsTimed)
        {
            StateContext.CustomNetworkManager.Countdown(CountdownMessage, () =>
            {
                StateContext.GameplayManager.TransitionToState(TransitionState);
            });
        }
    }
}
