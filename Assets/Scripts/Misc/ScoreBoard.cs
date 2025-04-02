using System.Collections;
using System.Collections.Generic;
using Mirror;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreBoard : NetworkBehaviour
{
    public static ScoreBoard Instance { get; private set; }
    [Serializable]
    public struct PlayerData
    {
        public int KillData;
        public int DeathData;
        public int AssistData;
        public PlayerData(int k, int d, int a)
        {
            this.KillData = k;
            this.DeathData = d;
            this.AssistData = a;
        }
    }

    public bool nameReady = false;

    public readonly SyncDictionary<uint, PlayerData> PlayerKDA = new SyncDictionary<uint, PlayerData>();
    public readonly SyncDictionary<uint, string> playerName = new SyncDictionary<uint, string>();
    private Dictionary<NetworkConnectionToClient, uint> playerConnectionToClient = new Dictionary<NetworkConnectionToClient, uint>();

    /// <summary>
    /// Awake
    /// </summary>
    private void Awake()
    {
        // Makes this object a singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Subscribe to events
    /// </summary>
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    /// <summary>
    /// Unsubscribe to events
    /// </summary>
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    /// <summary>
    /// Manage actions on speific scene
    /// </summary>
    /// <param name="scene">scene name</param>
    /// <param name="mode">scene loat status</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If the scene is back in lobby, deletes this object
        if (scene.name == "Gameplay-Lobby")
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
                Instance = null;
            }
        }

        playerName.OnAdd += OnScoreBoardAdd;
        playerName.OnRemove += OnScoreBoardRemove;
        PlayerKDA.OnSet += OnPlayerKDASet;
    }

    /// <summary>
    /// Manage actions when scene is not loaded
    /// </summary>
    /// <param name="scene">name of scene that unloaded(?) need checking of documetation</param>
    private void OnSceneUnloaded(Scene scene)
    {
        playerName.OnAdd -= OnScoreBoardAdd;
        playerName.OnRemove -= OnScoreBoardRemove;
        PlayerKDA.OnSet -= OnPlayerKDASet;
    }

    /// <summary>
    /// Used for added player to the game. If the player is the host they will propagate the change to 
    /// all clients to also update the client version of the scoreboard
    /// </summary>
    /// <param name="newPlayer">data of new player that joined</param>
    private void AddPlayerData(NetworkConnectionToClient conn)
    {
        NetworkIdentity connectedPlayer = conn.identity;
        if (!PlayerKDA.ContainsKey(connectedPlayer.netId))
        { //see if this player already exist
            PlayerData newData = new PlayerData(); //new player data
            PlayerKDA.Add(connectedPlayer.netId, newData);
            if (playerName.Count < 5)
            {
                playerName.Add(connectedPlayer.netId, connectedPlayer.gameObject.name);
            }
        }
        if (isServer)
        {
            playerConnectionToClient.Add(conn, connectedPlayer.netId);
        }
    }

    /// <summary>
    /// Used for remove disconnected player's data
    /// </summary>
    /// <param name="disconnectedPlayer">the disconnected player's net identity</param>
    private void RemovePlayerData(NetworkConnectionToClient conn)
    {
        NetworkIdentity disconnectedPlayer = conn.identity;
        if (PlayerKDA.ContainsKey(disconnectedPlayer.netId))
        {
            PlayerKDA.Remove(disconnectedPlayer.netId);
        }
        if (playerName.ContainsKey(disconnectedPlayer.netId))
        {
            playerName.Remove(disconnectedPlayer.netId);
        }
        if (isServer && playerConnectionToClient.ContainsKey(conn))
        {
            playerConnectionToClient.Remove(conn);
        }
    }

    /// <summary>
    /// Updates the scoreboard when a player joins
    /// </summary>
    /// <param name="netid">netid of the player who joined</param>
    private void OnScoreBoardAdd(uint netid)
    {
        if (NetworkClient.localPlayer != null)
        {
            NetworkClient.localPlayer.GetComponent<PlayerInterface>().RefreshScoreBoard();
        }
    }

    /// <summary>
    /// Updates the scoreboard when a player is removed
    /// </summary>
    /// <param name="netid">netid for the player removed</param>
    /// <param name="name">name of the player removed</param>
    private void OnScoreBoardRemove(uint netid, string name)
    {
        if (NetworkClient.localPlayer != null)
        {
            NetworkClient.localPlayer.GetComponent<PlayerInterface>().RefreshScoreBoard();
        }
    }

    /// <summary>
    /// Updates the scoreboard when playerdata changes
    /// </summary>
    /// <param name="netid">netid for the player who has their data changed</param>
    /// <param name="data">the old data</param>
    private void OnPlayerKDASet(uint netid, PlayerData data)
    {
        if (NetworkClient.localPlayer != null)
        {
            NetworkClient.localPlayer.GetComponent<PlayerInterface>().RefreshScoreBoard();
        }
    }

    /// <summary>
    /// Updates the player's kill
    /// </summary>
    /// <param name="netid">player netidentity used as key</param>
    /// <param name="k">amount of kill(s) to update by</param>
    /// <param name="mode">1 = add, 2 = set, 3 = remove, 0 = reset</param>
    [Command(requiresAuthority = false)]
    public void CmdUpdateKillData(uint netid, int k, int mode)
    {
        if (PlayerKDA.TryGetValue(netid, out PlayerData data))
        {
            switch (mode)
            {
                case 1:
                    data.KillData += k;
                    break;
                case 2:
                    data.KillData = k;
                    break;
                case 3:
                    data.KillData -= k;
                    break;
                default:
                    data.KillData = 0;
                    break;
            }
            PlayerKDA[netid] = data;
        }
    }

    /// <summary>
    /// Updates the player's death
    /// </summary>
    /// <param name="netid">player netidentity used as key</param>
    /// <param name="d">amount of death(s) to update by</param>
    /// <param name="mode">1 = add, 2 = set, 3 = remove, 0 = reset</param>
    [Command(requiresAuthority = false)]
    public void CmdUpdateDeathData(uint netid, int d, int mode)
    {
        if (PlayerKDA.TryGetValue(netid, out PlayerData data))
        {
            switch (mode)
            {
                case 1:
                    data.DeathData += d;
                    break;
                case 2:
                    data.DeathData = d;
                    break;
                case 3:
                    data.DeathData -= d;
                    break;
                default:
                    data.DeathData = 0;
                    break;
            }
            PlayerKDA[netid] = data;
        }
    }

    /// <summary>
    /// Updates the player's assist
    /// </summary>
    /// <param name="netid">player netidentity used as key</param>
    /// <param name="a">amount of assist(s) to update by</param>
    /// <param name="mode">1 = add, 2 = set, 3 = remove, 0 = reset</param>
    [Command(requiresAuthority = false)]
    public void CmdUpdateAssistData(uint netid, int a, int mode)
    {
        if (PlayerKDA.TryGetValue(netid, out PlayerData data))
        {
            switch (mode)
            {
                case 1:
                    data.AssistData += a;
                    break;
                case 2:
                    data.AssistData = a;
                    break;
                case 3:
                    data.AssistData -= a;
                    break;
                default:
                    data.AssistData = 0;
                    break;
            }
            PlayerKDA[netid] = data;
        }
    }

    /// <summary>
    /// Server call to remove the data for the disconnected player
    /// </summary>
    /// <param name="disconnectedPlayer">player disconnected</param>
    public void PlayerLeftUpdatePlayerList(NetworkConnectionToClient conn)
    {
        if (!isServer) return;
        RemovePlayerData(conn);
    }

    /// <summary>
    /// Server call to add the data for the connected player 
    /// </summary>
    /// <param name="connectedPlayer">player connected</param>
    public IEnumerator PlayerJoinedUpdatePlayerList(NetworkConnectionToClient conn)
    {
        yield return new WaitUntil(() => conn.identity != null && nameReady);
        if (isServer)
        {
            AddPlayerData(conn);
            nameReady = false;
        }
    }

    /// <summary>
    /// Updates the netid key for each element in playerKDA and PlayerName after a scene change
    /// </summary>
    /// <param name="conn">the connection of the player ready</param>
    public void UpdateNetId(NetworkConnectionToClient conn)
    {
        uint newNetId = conn.identity.netId;
        uint oldNetId = playerConnectionToClient.GetValueOrDefault(conn);
        // Update player kda key 
        PlayerKDA.Add(newNetId, PlayerKDA.GetValueOrDefault(oldNetId));
        PlayerKDA.Remove(oldNetId);

        // Update player name key
        playerName.Add(newNetId, playerName.GetValueOrDefault(oldNetId));
        playerName.Remove(oldNetId);

        // Update the player connection netid
        playerConnectionToClient.Remove(conn);
        playerConnectionToClient.Add(conn, newNetId);
    }

}
