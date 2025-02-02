using System.Collections;
using System.Collections.Generic;
using Mirror;
using Org.BouncyCastle.Crypto.Modes;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreBoard : NetworkBehaviour
{
    public static ScoreBoard Instance { get; private set; }
    public struct PlayerData{
        public int KillData { get; set; }
        public int DeathData { get; set; }
        public int AssistData { get; set; }
        public PlayerData(int k, int d, int a)
        {
            KillData = k;
            DeathData = d;
            AssistData = a;
        }
    } 

    public bool nameReady = false;
    
    public readonly SyncDictionary<uint, PlayerData> PlayerKDA = new SyncDictionary<uint, PlayerData>();
    public readonly SyncDictionary<uint, string> playerName = new SyncDictionary<uint, string>();
    
    private void Awake() {
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else{
            Destroy(gameObject);
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
 
    }

    private void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;   
        playerName.OnAdd += OnScoreBoardAdd;
        playerName.OnRemove += OnScoreBoardRemove;
    }
    private void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        playerName.OnAdd -= OnScoreBoardAdd;
        playerName.OnRemove -= OnScoreBoardRemove;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Gameplay-Lobby"){
            if (Instance != null){
                Destroy(Instance.gameObject);
                Instance = null;
            }
        }
    }

    /// <summary>
    /// Used for added player to the game. If the player is the host they will propagate the change to 
    /// all clients to also update the client version of the scoreboard
    /// </summary>
    /// <param name="newPlayer">data of new player that joined</param>
    private void AddPlayerData(NetworkConnectionToClient conn){
        NetworkIdentity connectedPlayer = conn.identity;
        if(!PlayerKDA.ContainsKey(connectedPlayer.netId)){ //see if this player already exist
            PlayerData newData = new PlayerData(0, 0, 0); //new player data
            PlayerKDA.Add(connectedPlayer.netId, newData);
            if(playerName.Count < 5){
                playerName.Add(connectedPlayer.netId, connectedPlayer.gameObject.name);   
            }
        }
    }

    /// <summary>
    /// Used for remove disconnected player's data
    /// </summary>
    /// <param name="disconnectedPlayer">the disconnected player's net identity</param>
    private void RemovePlayerData(NetworkConnectionToClient conn){
        NetworkIdentity disconnectedPlayer = conn.identity;
        if(PlayerKDA.ContainsKey(disconnectedPlayer.netId)){ 
            PlayerKDA.Remove(disconnectedPlayer.netId);
        }
        if(playerName.ContainsKey(disconnectedPlayer.netId)){
            playerName.Remove(disconnectedPlayer.netId);
        }
    }

    /// <summary>
    /// Updates the player KDA data by adding the parameters to the dictionary
    /// </summary>
    /// <param name="netid">player netidentity used as key</param>
    /// <param name="k">amount of kills to add</param>
    /// <param name="d">amount of death to add</param>
    /// <param name="a">amount of assist to add</param>
    private void AddDataToPlayerKDA(uint netid, int k, int d, int a){
        if(PlayerKDA.ContainsKey(netid)){
            PlayerData data = PlayerKDA[netid];
            data.KillData += k;
            data.DeathData += d;
            data.AssistData += a;
            PlayerKDA[netid] = data;
        }
    }

    /// <summary>
    /// Propagates the new data from the sender to all clients instance on the server and update the values.
    /// Will ignore to update the original send's data since it should already be updated
    /// </summary>
    /// <param name="sender">the connection that sent the update</param>
    /// <param name="newNetId">the data that needs to be added/updated</param>
    private void UpdateAddDataOnAllClient(NetworkConnectionToClient sender, NetworkIdentity newNetId){
        // foreach(var conn in NetworkServer.connections){
        //     if(conn.Value != sender){
        //         conn.Value.identity.GetComponent<ScoreBoard>().AddPlayerData(newNetId);
        //     }
        // }  
    }
    
    /// <summary>
    /// Propagates the new data from the sender to all client instances on the server and update the values.
    /// Will ignore to update the original send's data since it should already be updated. Used for updating
    /// kill, death, assist
    /// </summary>
    /// <param name="sender">the connection that sent the update</param>
    /// <param name="netid">the netid of the player that needs to be updated</param>
    /// <param name="k">amount of kills to add (optional)</param>
    /// <param name="d">amount of deaths to add (optional)</param>
    /// <param name="a">amount of assist to add (optional)</param>
    private void UpdateAddDataOnAllClient(NetworkConnectionToClient sender, NetworkIdentity targetPlayer, int k = 0, int d = 0, int a = 0){
        foreach(var conn in NetworkServer.connections){
            if(conn.Value != sender){
                conn.Value.identity.GetComponent<ScoreBoard>().AddDataToPlayerKDA(targetPlayer.netId, k, d, a);
            }
        }  
    }

    /// <summary>
    /// Propagates the disconnected player data to all client instances on the server.
    /// </summary>
    /// <param name="sender">the sender's connection</param>
    /// <param name="disconnectedPlayer">the player that needs to be removed</param>
    private void UpdateRemoveDataOnAllClient(NetworkConnectionToClient sender, NetworkIdentity disconnectedPlayer){
        // foreach(var conn in NetworkServer.connections){
        //     if(conn.Value != sender){
        //         conn.Value.identity.GetComponent<ScoreBoard>().RemovePlayerData(disconnectedPlayer);
        //     }
        // }  
    }

    /// <summary>
    /// Refresh the scoreboard when player name dictionary changes
    /// </summary>
    /// <param name="netid">key for the item added</param>
    private void OnScoreBoardAdd(uint netid){
            NetworkClient.localPlayer.GetComponent<PlayerInterface>().RpcRefreshScoreBoard();
    }

    /// <summary>
    /// Refresh the scoreboard when player name dictionary changes
    /// </summary>
    /// <param name="netid">key for the key value pair removed</param>
    /// <param name="name">value for the key value pair removed</param>
    private void OnScoreBoardRemove(uint netid, string name){
        foreach(var conn in NetworkServer.connections){
            conn.Value.identity.GetComponent<PlayerInterface>().RpcRefreshScoreBoard();
        }
    }

    /// <summary>
    /// When the player first joins the game, they need to get the current scoreboard and playerData from the host
    /// </summary>
    public void InitialAddPlayerData(){
        // ScoreBoard serverScoreBoard = NetworkServer.localConnection.identity.GetComponent<ScoreBoard>();
        // foreach(KeyValuePair<uint, string> nameData in serverScoreBoard.playerName){
        //     if(!playerName.ContainsKey(nameData.Key)){
        //         playerName.Add(nameData.Key, nameData.Value);
        //     }
        // }   
        // foreach(var data in serverScoreBoard.PlayerKDA){
        //     if(!PlayerKDA.ContainsKey(data.Key)){
        //         PlayerKDA.Add(data);
        //     }
        // } 
    }

    /// <summary>
    /// Server call to remove the data for the disconnected player
    /// </summary>
    /// <param name="disconnectedPlayer">player disconnected</param>
    public void PlayerLeftUpdatePlayerList(NetworkConnectionToClient conn){
        if(!isServer) return;
        RemovePlayerData(conn);
    }
    
    /// <summary>
    /// Server call to add the data for the connected player 
    /// </summary>
    /// <param name="connectedPlayer">player connected</param>
    public IEnumerator PlayerJoinedUpdatePlayerList(NetworkConnectionToClient conn){
        yield return new WaitUntil(() => conn.identity != null && nameReady);
        if(isServer){
            AddPlayerData(conn);
            nameReady = false;            
        }
    }
}
