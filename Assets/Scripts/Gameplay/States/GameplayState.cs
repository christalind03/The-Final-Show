using Mirror;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GameplayState : BaseState<GameplayManager.State, GameplayContext>
{
    [SerializeField]
    [Tooltip("The scene associated with the Gameplay State")]
    private SceneAsset _targetScene;

    [Header("Countdown Properties")]
    [SerializeField] private bool _isTimed;
    [SerializeField] private CountdownMessage _countdownMessage;
    
    [SerializeField]
    [Tooltip("The state to transition to when the countdown is complete")]
    private GameplayManager.State _transitionState;

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

        if (activeScene.name != _targetScene.name)
        {
            NetworkManager.singleton.ServerChangeScene(_targetScene.name);
        }
    }

    // TODO: Documentation
    private void OnSceneLoaded(Scene activeScene, LoadSceneMode loadMode)
    {
        if (activeScene.name != _targetScene.name) { return; }

        if (_isTimed)
        {
            StateContext.NetworkManager.Countdown(_countdownMessage, () =>
            {
                StateContext.GameplayManager.TransitionToState(_transitionState);
            });
        }
    }

    // TODO: Documentation
    public override void ExitState()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
