using UnityEngine;
using Mirror;

/// <summary>
/// Network Manager that controls all the server and client processing
/// </summary>
public class CustomNetworkManager : NetworkManager
{
    /// <summary>
    /// If the scence changes to the gameplay scene, Instantiate all the players and add them to the connection 
    /// </summary>
    /// <param name="sceneName">Scene that is being transitioned to</param>
    public override void OnServerSceneChanged(string sceneName){
        base.OnServerSceneChanged(sceneName);
        if(sceneName == "Gameplay"){
            if(NetworkServer.active){
                foreach(NetworkConnectionToClient conn in NetworkServer.connections.Values){
                    if (!NetworkClient.ready)
                        NetworkClient.Ready();
                    if (conn.identity == null){
                        GameObject player = Instantiate(playerPrefab);
                        NetworkServer.AddPlayerForConnection(conn, player);
                    }
                }
            }
        }
    }
}
