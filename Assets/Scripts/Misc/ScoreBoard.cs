using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(PlayerInterface))]
public class ScoreBoard : NetworkBehaviour
{
    [SerializeField] private PlayerInterface _playerInterface;
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

    
    public readonly SyncDictionary<uint, PlayerData> PlayerKDA = new SyncDictionary<uint, PlayerData>();
    public readonly SyncDictionary<uint, string> playerName = new SyncDictionary<uint, string>();

    /// <summary>
    /// The server version of InitializesPlayerData, instead of loop through the whole
    /// playerName dictionary, it will just add the most recent player that joined
    /// </summary>
    /// <param name="netid">most receent player joined</param>
    /// <returns></returns>
    private void AddPlayerData(NetworkIdentity newPlayer){
        if(!PlayerKDA.ContainsKey(newPlayer.netId)){ //see if this player already exist
            PlayerData newData = new PlayerData(0, 0, 0); //new player data
            PlayerKDA.Add(newPlayer.netId, newData);
            if(playerName.Count < 5){
                playerName.Add(newPlayer.netId, newPlayer.gameObject.name);    
                _playerInterface.RefreshScoreBoard();          
            }
        }

    }

    /// <summary>
    /// Updates the player KDA data by adding the parameters to the dictionary
    /// </summary>
    /// <param name="netid">player netidentity used as key</param>
    /// <param name="k">amount of kills to add</param>
    /// <param name="d">amount of death to add</param>
    /// <param name="a">amount of assist to add</param>
    private void UpdatePlayerData(uint netid, int k, int d, int a){
        if(PlayerKDA.ContainsKey(netid)){
            PlayerData data = PlayerKDA[netid];
            data.KillData += k;
            data.DeathData += d;
            data.AssistData += a;
            PlayerKDA[netid] = data;
        }
    }

    /// <summary>
    /// Allows outside classes to access show score board
    /// also updates the score board if there are any changes to the data
    /// </summary>
    public void ShowScoreBoard(){
        Debug.Log(playerName.Count);
        _playerInterface.ToggleScoreBoardVisibility();
    }

    public void RemovePlayer(){
        
    }
    public void UpdatePlayerList(NetworkIdentity newPlayer){
        AddPlayerData(newPlayer);
    }
}
