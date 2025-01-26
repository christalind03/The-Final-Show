using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "New Dungeon Gameplay State", menuName = "Base State/Gameplay/Dungeon")]
public class GameplayStateDungeon : GameplayState
{
    private SafeZone _safeZone;

    /// <summary>
    /// Sets a custom callback to be executed once the countdown finishes.
    /// If players are in the safe zone, then transitions the game state to the boss fight.
    /// Players not within the safe zone are sent a message to spectate the event.
    /// Otherwise, the game transitions in to the next (default) state in the gameplay flow.
    /// </summary>
    public override void EnterState()
    {
        Debug.Log("Entered dungeon state...");

        CountdownCallback = () =>
        {
            if (_safeZone != null && _safeZone.ContainsPlayers)
            {
                GameplayManager.Instance.TransitionToState(GameplayManager.State.Boss);

                List<GameObject> invalidPlayers = NetworkUtils.RetrievePlayers().Except(_safeZone.SafePlayers).ToList();

                foreach (GameObject invalidPlayer in invalidPlayers)
                {
                    NetworkIdentity invalidPlayerIdentity = invalidPlayer.GetComponent<NetworkIdentity>();

                    invalidPlayerIdentity.connectionToClient.Send(new SpectateMessage { });
                }
            }
            else
            {
                GameplayManager.Instance.TransitionToState(TransitionState);
            }
        };

        base.EnterState();
    }

    /// <summary>
    /// Searches for the safe zone object within the loaded scene.
    /// If found, then set the safe zone reference to the one found.
    /// </summary>
    /// <param name="activeScene">The scene that was loaded</param>
    /// <param name="loadMode">The mode in which the scene was loaded</param>
    protected override void OnSceneLoaded(Scene activeScene, LoadSceneMode loadMode)
    {
        if (activeScene.name != TargetScene) { return; }

        GameplayManager.Instance.FindObject((DungeonGenerator targetObject) =>
        {
            if (targetObject != null)
            {
                GameplayManager.Instance.StartCoroutine(WaitForDungeonGeneration(targetObject));
            }
        });
    }

    // TODO: Documentatin
    private IEnumerator WaitForDungeonGeneration(DungeonGenerator dungeonGenerator)
    {
        while (!dungeonGenerator.IsGenerated())
        {
            yield return null;
        }

        RelocatePlayers();
        dungeonGenerator.SpawnEnemies(StateContext.GameplayTheme.EnemyPrefabs);

        GameplayManager.Instance.FindObject((SafeZone targetObject) =>
        {
            if (targetObject != null)
            {
                Debug.Log("Found the safe zone!");
                _safeZone = targetObject;
            }
        });
        
        if (IsTimed)
        {
            CustomNetworkManager.Instance.Countdown(
                CountdownMessage,
                CountdownCallback ?? (() => GameplayManager.Instance.TransitionToState(TransitionState))
            );
        }
    }

    // TODO: Document
    private void RelocatePlayers()
    {
        foreach (NetworkConnectionToClient clientConnection in NetworkServer.connections.Values)
        {
            if (clientConnection.isReady)
            {
                CustomNetworkManager.Instance.StartCoroutine(CustomNetworkManager.Instance.RelocatePlayer(clientConnection));
            }
        }
    }

    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }
    public override void UpdateState() { }
}
