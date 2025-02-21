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
    private int _scriptsNeeded = 3; // This would change depending on difficulty, using predefined value for testing

    /// <summary>
    /// Sets a custom callback to be executed once the countdown finishes.
    /// If players are in the safe zone, then transitions the game state to the boss fight.
    /// Players not within the safe zone are sent a message to spectate the event.
    /// Otherwise, the game transitions in to the next (default) state in the gameplay flow.
    /// </summary>
    public override void EnterState()
    {
        Debug.Log("Entered dungeon state...");
        StateContext._scriptsCollected = 0; // Reset scripts collected
        CountdownCallback = () =>
        {
            Debug.Log("CountdownCallback: Scripts Needed: " + _scriptsNeeded);
            Debug.Log("CountdownCallback: Scripts Collected: " + StateContext._scriptsCollected);
            Debug.Log("Boolean: " + (StateContext._scriptsCollected >= _scriptsNeeded));
            if (_safeZone != null && _safeZone.ContainsPlayers && StateContext._scriptsCollected >= _scriptsNeeded)
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
                GameplayManager.Instance.StartCoroutine(OnDungeonGenerationComplete(targetObject));
            }
        });
    }

    /// <summary>
    /// Waits for the dungeon to finish generating then relocates players to their start positions, spawns enemies, and finds the safe zone.
    /// Optionally, starts the countdown timer if the gameplay is timed.
    /// </summary>
    /// <param name="dungeonGenerator">The <see cref="DungeonGenerator"/> instance responsible for generating the dungeon</param>
    /// <returns>An <c>IEnumerator</c> for coroutine execution.</returns>
    private IEnumerator OnDungeonGenerationComplete(DungeonGenerator dungeonGenerator)
    {
        while (!dungeonGenerator.IsGenerated)
        {
            yield return null;
        }

        //RelocatePlayers();
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

    ///// <summary>
    ///// Relocates all ready players to their respective spawn positions in the game.
    ///// This method is invoked for each connected client to ensure proper placement in the newly generated dungeon.
    ///// </summary>
    //private void RelocatePlayers()
    //{
    //    foreach (NetworkConnectionToClient clientConnection in NetworkServer.connections.Values)
    //    {
    //        if (clientConnection.isReady)
    //        {
    //            CustomNetworkManager.Instance.StartCoroutine(CustomNetworkManager.Instance.RelocatePlayer(clientConnection));
    //        }
    //    }
    //}

    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }
    public override void UpdateState() { }
}
