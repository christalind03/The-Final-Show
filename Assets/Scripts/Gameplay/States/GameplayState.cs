using Mirror;
using System;
using System.IO;
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
    [SerializeField]
    [Tooltip("Animation Class for scene transition")]

    protected Action CountdownCallback;

    /// <summary>
    /// Loads the target scene and subscribes to the <c>sceneLoaded</c> event listener.
    /// </summary>
    public override void EnterState()
    {
        // Since the [Scene] attribute allows us to retrieve the scene's name by reference, it returns the entire file name.
        // To prevent issues with string comparisons, we need to ensure we don't have the file extension at the end.
        TargetScene = Path.GetFileNameWithoutExtension(TargetScene);

        LoadScene();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// Loads the target scene on the server if it is not already the active scene.
    /// </summary>
    private void LoadScene()
    {   
        Scene activeScene = SceneManager.GetActiveScene();

        if (activeScene.name != TargetScene)
        {
            NetworkManager.singleton.ServerChangeScene(TargetScene);
        }
    }

    /// <summary>
    /// Performed when the scene is loaded on the server.
    /// If the loaded scene matches the target scene and the state is timed, starts a countdown sequence.
    /// </summary>
    /// <param name="activeScene">The scene that was loaded</param>
    /// <param name="loadMode">The mode in which the scene was loaded</param>
    protected virtual void OnSceneLoaded(Scene activeScene, LoadSceneMode loadMode)
    {
        if (activeScene.name != TargetScene) { return; }
        if (IsTimed)
        {
            CustomNetworkManager.Instance.Countdown(
                CountdownMessage,
                CountdownCallback ?? (() => GameplayManager.Instance.TransitionToState(TransitionState))
            );
        }
    }

    /// <summary>
    /// Unsubscribe from the <c>sceneLoaded</c> event listener.
    /// </summary>
    public override void ExitState()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
