using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    // TODO: Document
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

    // TODO: Document
    public void FindObject<TComponent>(Action<TComponent> onFound) where TComponent : Component
    {
        StartCoroutine(UnityUtils.WaitForObject(GeneralUtils.DefaultTimeout, onFound));
    }
}
