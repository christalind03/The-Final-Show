using Mirror;
using Steamworks;
using UnityEngine;

[CreateAssetMenu(fileName = "New Intermission Gameplay State", menuName = "Base State/Gameplay/Intermission")]
public class GameplayStateIntermission : GameplayState
{
    public override void EnterState()
    {
        StateContext.RandomizeTheme();

        foreach (GameObject clientObject in NetworkUtils.RetrievePlayers())
        {
            CameraController clientCamera = clientObject.GetComponent<CameraController>();
            PlayerVisibility clientVisibility = clientObject.GetComponent<PlayerVisibility>();
            NetworkIdentity clientIdentity = clientObject.GetComponent<NetworkIdentity>();
            PlayerHealth clientHealth = clientObject.GetComponent<PlayerHealth>();
            
            clientHealth.TargetResetHealth(clientIdentity.connectionToClient);
            clientVisibility.RpcToggleVisbility(true);
            clientCamera.TargetPlay(clientIdentity.connectionToClient);
        }

        SteamLobby steamLobby = NetworkManager.FindObjectOfType<SteamLobby>();
        if(NetworkClient.activeHost&& steamLobby != null)
        {
            steamLobby.SetSceneData("Intermission");  
            
        }

        base.EnterState();
    }

    public override void OnTriggerEnter(Collider otherCollider) { }
    public override void OnTriggerExit(Collider otherCollider) { }
    public override void OnTriggerStay(Collider otherCollider) { }
    public override void UpdateState() { }
}
