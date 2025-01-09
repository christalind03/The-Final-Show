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

    // TODO: Document
    public override void OnStartServer()
    {
        if (Instance == null)
        {
            CustomNetworkManager networkManager = GameObject.FindObjectOfType<CustomNetworkManager>();

            Instance = this;
            StateContext = new GameplayContext(this, networkManager);

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
