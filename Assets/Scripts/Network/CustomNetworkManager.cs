using UnityEngine;
using Mirror;

public class CustomNetworkManager : NetworkManager
{
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
