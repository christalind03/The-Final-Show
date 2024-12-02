using UnityEngine;
using Mirror;
using Steamworks;
using UnityEditor;

/// <summary>
/// Network Manager that controls all the server and client processing
/// </summary>
public class CustomNetworkManager : NetworkManager
{

    public ulong LobbyId{ get; set; }
 
    /// <summary>
    /// If the scence changes to the gameplay scene, Instantiate all the players and add them to the connection 
    /// </summary>
    /// <param name="sceneName">Scene that is being transitioned to</param>
    public override void OnServerSceneChanged(string sceneName){
        base.OnServerSceneChanged(sceneName);
        if(sceneName == "Gameplay" && NetworkServer.active){
            int numPlayer = 0;
            foreach(NetworkConnectionToClient conn in NetworkServer.connections.Values){
                if (!NetworkClient.ready) {NetworkClient.Ready();}

                if (conn.identity == null){
                    GameObject player = Instantiate(playerPrefab);
                    CSteamID steamId = SteamMatchmaking.GetLobbyMemberByIndex(new CSteamID(LobbyId), numPlayer);
                    player.name = SteamFriends.GetFriendPersonaName(steamId);
                    numPlayer++; 
                    NetworkServer.AddPlayerForConnection(conn, player);
                }
            }
        }
    }
}
