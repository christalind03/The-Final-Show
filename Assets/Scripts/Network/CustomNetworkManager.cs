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
        if(sceneName == "Dungeon1" && NetworkServer.active){
            int numPlayer = 0;
            foreach(NetworkConnectionToClient conn in NetworkServer.connections.Values){
                if (!NetworkClient.ready) {NetworkClient.Ready();}

                if (conn.identity == null){
                    int random = UnityEngine.Random.Range(0, startPositions.Count);
                    GameObject player = Instantiate(playerPrefab, startPositions[random]);
                    CSteamID steamId = SteamMatchmaking.GetLobbyMemberByIndex(new CSteamID(LobbyId), numPlayer);
                    player.name = SteamFriends.GetFriendPersonaName(steamId);
                    numPlayer++; 
                    NetworkServer.AddPlayerForConnection(conn, player);
                }
            }
        }
    }

    /// <summary>
    /// When the host disconnects/stops, will unlock cursor and scene will be changed to the offlien scene (Network-Lobby)
    /// </summary>
    public override void OnStopHost(){
        base.OnStopHost();
        if(mode == NetworkManagerMode.Offline){
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }
    }

    /// <summary>
    /// When the client disconnects/stops, will unlock cursor and scene will be changed to the offlien scene (Network-Lobby)
    /// </summary>
    public override void OnStopClient(){
        base.OnStopClient();
        if(mode == NetworkManagerMode.Offline){
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }
    }
}
