using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "New Boss Gameplay State", menuName = "Base State/Gameplay/Boss")]
public class GameplayStateBoss : GameplayState
{
    private EnemyHealth _enemyHealth;
    private bool _sceneLoaded;

    /// <summary>
    /// Initializes _sceneLoaded to false to ensure we don't transition to Intermission prematurely.
    /// </summary>
    public override void EnterState()
    {
        Debug.Log("Entering BOSS State");
        _sceneLoaded = false;
        SteamLobby steamLobby = NetworkManager.FindObjectOfType<SteamLobby>();
        if(NetworkClient.activeHost && steamLobby != null)
        {
            steamLobby.SetSceneData("Boss");         
        }
        base.EnterState();
    }

    /// <summary>
    /// When the target scene is loaded, instantiates and spawns the boss depending on the active theme.
    /// </summary>
    /// <param name="activeScene">The scene that was loaded</param>
    /// <param name="loadMode">The mode in which the scene was loaded</param>
    protected override void OnSceneLoaded(Scene activeScene, LoadSceneMode loadMode)
    {
        base.OnSceneLoaded(activeScene, loadMode);
        GameObject boss = Instantiate(StateContext.GameplayTheme.BossPrefab, new Vector3(0, 0, 20), Quaternion.Euler(0, 180, 0)); // Spawned at arbitrary position and rotation
        NetworkServer.Spawn(boss);
        _enemyHealth = boss.GetComponent<EnemyHealth>();
        _sceneLoaded = true;
    }

    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }
    
    /// <summary>
    /// Updates the state logic during gameplay.
    /// Monitors the boss' health status and triggers a state transition into <see cref="GameplayManager.State.Intermission"/>
    /// when the boss is defeated.
    /// </summary>
    public override void UpdateState()
    {
        // The boss' health script becomes null when the boss is defeated, as the reference is destroyed
        // However, when initially loading in to the scene, this health script is automatically set to be null
        // So, we have an additional check to ensure that the scene finished loading to prevent transitioning states too early
        if (_sceneLoaded && _enemyHealth == null)
        {
            CustomNetworkManager.Instance.StopAllCoroutines();
            GameplayManager.Instance.TransitionToState(GameplayManager.State.Intermission);
        }
    }
}
