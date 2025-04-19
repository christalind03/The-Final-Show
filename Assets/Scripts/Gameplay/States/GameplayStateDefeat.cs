using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "New Defeat Gameplay State", menuName = "Base State/Gameplay/Defeat")]
public class GameplayStateDefeat : GameplayState
{
    private bool _sceneLoaded;
    public override void EnterState()
    {
        Debug.Log("Entering the DEFEAT gameplay state...");
        SteamLobby steamLobby = NetworkManager.FindObjectOfType<SteamLobby>();
        if(NetworkClient.activeHost && steamLobby != null)
        {
            steamLobby.SetSceneData("Defeat");         
        }
        base.EnterState();
    }

    /// <summary>
    /// When the target scene is loaded, instantiates and spawns the map depending on the active theme.
    /// </summary>
    /// <param name="activeScene">The scene that was loaded</param>
    /// <param name="loadMode">The mode in which the scene was loaded</param>
    protected override void OnSceneLoaded(Scene activeScene, LoadSceneMode loadMode)
    {
        base.OnSceneLoaded(activeScene, loadMode);
        GameObject defeatRoom = Instantiate(StateContext.GameplayTheme.DefeatRoomPrefab, Vector3.zero, Quaternion.identity);
        NetworkServer.Spawn(defeatRoom);
        _sceneLoaded = true;
    
    }
    public override void ExitState()
    {
        Debug.Log("Exiting the DEFEAT gameplay state...");
        base.ExitState();
    }

    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }
    public override void UpdateState() { }
}
