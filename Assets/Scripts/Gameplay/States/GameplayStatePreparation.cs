using UnityEngine;
using Steamworks;
using Mirror;

[CreateAssetMenu(fileName = "New Preparation Gameplay State", menuName = "Base State/Gameplay/Preparation")]
public class GameplayStatePreparation : GameplayState
{
    public override void EnterState()
    {
        SteamLobby steamLobby = NetworkManager.FindObjectOfType<SteamLobby>();
        if(NetworkClient.activeHost && steamLobby != null)
        {
            steamLobby.SetSceneData("Preparation");         
        }

        base.EnterState();
    }
    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }
    public override void UpdateState() { }
}
