using System;
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
            StateContext = new GameplayContext();

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
}
