using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

[RequireComponent(typeof(PlayerInterface))]
public class ScoreBoard : NetworkBehaviour
{
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

    [SerializeField] private PlayerInterface _playerInterface;
    
    public readonly SyncDictionary<uint, PlayerData> PlayerKDA = new SyncDictionary<uint, PlayerData>();
    public readonly SyncDictionary<uint, string> playerName = new SyncDictionary<uint, string>();
    
    /// <summary>
    /// Used for added player to the game. If the player is the host they will propagate the change to 
    /// all clients to also update the client version of the scoreboard
    /// </summary>
    /// <param name="newPlayer">data of new player that joined</param>
    private void AddPlayerData(NetworkIdentity connectedPlayer){
        if(!PlayerKDA.ContainsKey(connectedPlayer.netId)){ //see if this player already exist
            PlayerData newData = new PlayerData(0, 0, 0); //new player data
            PlayerKDA.Add(connectedPlayer.netId, newData);
            if(playerName.Count < 5){
                playerName.Add(connectedPlayer.netId, connectedPlayer.gameObject.name);    
                _playerInterface.RefreshScoreBoard();          
            }
        }
        if(connectionToClient == NetworkServer.localConnection){
            UpdateAddDataOnAllClient(connectionToClient, connectedPlayer);          
        }
    }

    /// <summary>
    /// Used for remove disconnected player's data
    /// </summary>
    /// <param name="disconnectedPlayer">the disconnected player's net identity</param>
    private void RemovePlayerData(NetworkIdentity disconnectedPlayer){
        if(PlayerKDA.ContainsKey(disconnectedPlayer.netId)){ 
            PlayerKDA.Remove(disconnectedPlayer.netId);
        }
        if(playerName.ContainsKey(disconnectedPlayer.netId)){
            playerName.Remove(disconnectedPlayer.netId);
        }

        if(connectionToClient == NetworkServer.localConnection){
            UpdateRemoveDataOnAllClient(connectionToClient, disconnectedPlayer);          
        }

        _playerInterface.RefreshScoreBoard();
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
        foreach(var conn in NetworkServer.connections){
            if(conn.Value != sender){
                conn.Value.identity.GetComponent<ScoreBoard>().AddPlayerData(newNetId);
            }
        }  
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
        foreach(var conn in NetworkServer.connections){
            if(conn.Value != sender){
                conn.Value.identity.GetComponent<ScoreBoard>().RemovePlayerData(disconnectedPlayer);
            }
        }  
    }

    /// <summary>
    /// When the player first joins the game, they need to get the current scoreboard and playerData from the host
    /// </summary>
    public void InitialAddPlayerData(){
        ScoreBoard serverScoreBoard = NetworkServer.localConnection.identity.GetComponent<ScoreBoard>();
        foreach(KeyValuePair<uint, string> nameData in serverScoreBoard.playerName){
            if(!playerName.ContainsKey(nameData.Key)){
                playerName.Add(nameData);
            }
        }   
        foreach(var data in serverScoreBoard.PlayerKDA){
            if(!PlayerKDA.ContainsKey(data.Key)){
                PlayerKDA.Add(data);
            }
        } 
    }

    /// <summary>
    /// Allows outside classes to access show score board
    /// also updates the score board if there are any changes to the data
    /// </summary>
    public void ShowScoreBoard(){
        
        _playerInterface.ToggleScoreBoardVisibility();
    }

    public void PlayerLeftUpdatePlayerList(NetworkIdentity disconnectedPlayer){
        RemovePlayerData(disconnectedPlayer);
    }
    
    public void PlayerJoinedUpdatePlayerList(NetworkIdentity connectedPlayer){
        AddPlayerData(connectedPlayer);
    }
}
