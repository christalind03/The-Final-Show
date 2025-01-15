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
        LoadScene();
        SceneManager.sceneLoaded += OnSceneLoaded;
        TargetScene = Path.GetFileNameWithoutExtension(TargetScene);
    }

    // TODO: Documentation
    private void LoadScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();

        if (activeScene.name != TargetScene)
        {
            NetworkManager.singleton.ServerChangeScene(TargetScene);
        }
    }

    // TODO: Documentation
    protected virtual void OnSceneLoaded(Scene activeScene, LoadSceneMode loadMode)
    {
        if (activeScene.name != TargetScene) { return; }

        if (IsTimed)
        {
            CustomNetworkManager.Instance.Countdown(CountdownMessage, () =>
            {
                GameplayManager.Instance.TransitionToState(TransitionState);
            });
        }
    }

    // TODO: Documentation
    public override void ExitState()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
