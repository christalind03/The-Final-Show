using Mirror;
using UnityEngine;

[CreateAssetMenu(fileName = "New Defeat Gameplay State", menuName = "Base State/Gameplay/Defeat")]
public class GameplayStateDefeat : GameplayState
{
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
