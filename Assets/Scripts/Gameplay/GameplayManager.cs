using System;
using Mirror;
using UnityEngine;

public class GameplayManager : StateManager<GameplayManager.State, GameplayState, GameplayContext>
{
    public enum State
    {
        Boss,
        Defeat,
        Dungeon,
        Intermission,
        Preparation,
    }

    public static GameplayManager Instance { get; private set; }

    [SerializeField] private GameplayTheme[] _gameplayThemes;

    /// <summary>
    /// Initializes a single instance of this class and sets up the initial gameplay context.
    /// If an instance of this class already exists, then destroy the current instance.
    /// </summary>
    public override void OnStartServer()
    {
        if (Instance == null)
        {
            CustomNetworkManager networkManager = GameObject.FindObjectOfType<CustomNetworkManager>();

            Instance = this;
            StateContext = new GameplayContext(_gameplayThemes);

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Finds an object of a specified component type within the current scene and performs an action on it.
    /// Uses a coroutine to wait for the object to become available within a default timeout period.
    /// </summary>
    /// <typeparam name="TComponent">The type of component to find</typeparam>
    /// <param name="onFound">The action to perform once the component has been found</param>
    public void FindObject<TComponent>(Action<TComponent> onFound) where TComponent : Component
    {
        StartCoroutine(UnityUtils.WaitForObject(GeneralUtils.DefaultTimeout, onFound));
    }

    /// <summary>
    /// Called by script objects when collected to increment the total number of scripts collected.
    /// Updates the script count in StateContext so other states may access it.
    /// </summary>
    public void CollectScript()
    {
        StateContext.CollectScript();
        ScriptManagement scriptManager = NetworkManager.FindObjectOfType<ScriptManagement>();
        scriptManager.currentScript = StateContext.scriptsCollected;
        scriptManager.UpdateMessage();
    }

    public string GetTheme()
    {
        return StateContext.GameplayTheme.Theme;
    }

    public int GetScriptsNeeded()
    {
        return StateContext.scriptsNeeded;
    }
}
