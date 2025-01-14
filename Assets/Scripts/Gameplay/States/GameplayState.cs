using Mirror;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GameplayState : BaseState<GameplayManager.State, GameplayContext>
{
    [SerializeField]
    [Tooltip("The scene associated with the Gameplay State")]
    protected SceneAsset TargetScene;

    [Header("Countdown Properties")]
    [SerializeField] protected bool IsTimed;
    [SerializeField] protected CountdownMessage CountdownMessage;
    
    [SerializeField]
    [Tooltip("The state to transition to when the countdown is complete")]
    protected GameplayManager.State TransitionState;

    // TODO: Documentation
    public override void EnterState()
    {
        LoadScene();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // TODO: Documentation
    private void LoadScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();

        if (activeScene.name != TargetScene.name)
        {
            NetworkManager.singleton.ServerChangeScene(TargetScene.name);
        }
    }

    // TODO: Documentation
    protected virtual void OnSceneLoaded(Scene activeScene, LoadSceneMode loadMode)
    {
        if (activeScene.name != TargetScene.name) { return; }

        if (IsTimed)
        {
            StateContext.NetworkManager.Countdown(CountdownMessage, () =>
            {
                StateContext.GameplayManager.TransitionToState(TransitionState);
            });
        }
    }

    // TODO: Documentation
    public override void ExitState()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
