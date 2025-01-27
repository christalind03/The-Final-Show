using System.Collections.Generic;
using Mirror;
using Org.BouncyCastle.Crypto.Modes;
using UnityEngine;

[RequireComponent(typeof(PlayerInterface))]
public class ScoreBoard : NetworkBehaviour
{
    [SerializeField] private PlayerInterface _playerInterface;
    private struct PlayerData{
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

    private Dictionary<string, Dictionary<uint, string>> SlotWithPlayerName;
    
    private Dictionary<uint, PlayerData> PlayerKDA;
    private Dictionary<uint, string> playerName;
    /// <summary>
    /// Initalizes the SlotWithPlayerName with the slot name null for the player name data
    /// </summary>
    public override void OnStartAuthority()
    {
        SlotWithPlayerName  = new Dictionary<string, Dictionary<uint, string>>
        {
            { "Player-1", null },
            { "Player-2", null },
            { "Player-3", null },
            { "Player-4", null },
            { "Player-5", null }
        };
        PlayerKDA = new Dictionary<uint, PlayerData>();

        base.OnStartAuthority();
    }

    /// <summary>
    /// Gets the dictionary of player names from PlayerManager and iterates through the list 
    /// to find if there is a new player that is not in the PlayerKDA. If this is true, it will
    /// create a new playerData and add it with the netid of player. Also adds the new player data
    /// to the next available slot.
    /// </summary>
    private void InitializesPlayerData(uint netid){
        if(!PlayerKDA.ContainsKey(netid)){ //see if this player already exist
            PlayerData newData = new PlayerData(0, 0, 0); //new player data
            PlayerKDA.Add(netid, newData);
            foreach (var slot in SlotWithPlayerName){ //iterate to the next available slot to add player list
                if (slot.Value == null){
                    SlotWithPlayerName[slot.Key] = new Dictionary<uint, string> {{netid, name}};
                    _playerInterface.AddPlayerToScoreBoard(slot.Key, name); //add the new player to the scoreboard
                    break;
                }
            
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
    /// Request the player list from the server
    /// </summary>
    [Command]
    private void CmdRequestPlayerList(){
        List<string> playerNames = new List<string>();
        List<uint> playerNetId = new List<uint>();
        foreach(var entry in PlayerManager.GetPlayerNameList()){
            playerNetId.Add(entry.Key);
            playerNames.Add(entry.Value);
        }
        ClientRpcRequestPlayerList(playerNetId, playerNames);
    }

    /// <summary>
    /// Sends the requested player list to the correct client
    /// </summary>
    /// <param name="netIds">list of all netids currently on the server</param>
    /// <param name="names">list of all player names current on the server</param>
    [ClientRpc]
    private void ClientRpcRequestPlayerList(List<uint> netIds, List<string> names){
        Dictionary<uint, string> dictionaryNames = new Dictionary<uint, string>();
        int i = 0;
        foreach(var netid in netIds){
            dictionaryNames.Add(netid, names[i]);
            i++;
        }
        playerName = dictionaryNames;
    }

    /// <summary>
    /// Allows outside classes to update score board
    /// </summary>
    public void ShowScoreBoard(){
        _playerInterface.ToggleScoreBoardVisibility();
    }

    public void RemovePlayer(uint netid){
        foreach(KeyValuePair<string, Dictionary<uint, string>> data in SlotWithPlayerName){
            if(data.Value.ContainsKey(netid)){
                _playerInterface.RemovePlayerFromScoreBoard(data.Key);
                SlotWithPlayerName.Remove(data.Key);
                break;
            }
        }
        CmdRequestPlayerList();
    }
    public void UpdatePlayerList(uint netid){
        InitializesPlayerData(netid);
        CmdRequestPlayerList();
    }
}
