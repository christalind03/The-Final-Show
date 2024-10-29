using UnityEngine;
using Mirror;
using Steamworks;

/// <summary>
/// Controls the different aspects related to steam lobby creation, players can leave, join, create lobbies
/// Works with the ui manager script to switch the ui based on the button that was pressed
/// </summary>
public class SteamLobby : MonoBehaviour
{
    // Callbacks for lobby 
    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> LobbyRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;

    // Variables
    private const string HostAddressKey = "HostAddress";
    private CustomNetworkManager manager; 

    // Gameobjects
    private UIManager uIManagar;

    /// <summary>
    /// When the game starts, this will find all the required GameObjects to let the script run and 
    /// create functions for the different callbacks
    /// </summary>
    private void Start() {
        GameObject lobbyUI = GameObject.Find("LobbyUI");
        uIManagar = lobbyUI.GetComponent<UIManager>();
        manager = GetComponent<CustomNetworkManager>();

        // if the steam manager is not initialized, the script that controls steam in the backend, it will create any of the callbacks functions
        if(!SteamManager.Initialized){ return; }   

        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        LobbyRequest = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyRequest);

    }

    /// <summary>
    /// When a user joins the game via steam ui, this function is called to let them join the lobby
    /// </summary>
    /// <param name="callback">The callback associated when steam request to join a lobby</param>
    private void OnLobbyRequest(GameLobbyJoinRequested_t callback){
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    /// <summary>
    /// When the lobby is created, assigns some metadata to the lobby that will be used for connection and message display
    /// </summary>
    /// <param name="callback">The callback associated when a lobby is created</param>
    private void OnLobbyCreated(LobbyCreated_t callback){
        // Make sure the lobby is actually create
        if(callback.m_eResult != EResult.k_EResultOK){ return;}

        manager.StartHost();

        // Assign the metadata, HostAddressKey and "name"
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", SteamFriends.GetPersonaName().ToString() + "'s Lobby");
    }

    /// <summary>
    /// When the player enter a steam lobby, do different task according to if the player is the server or client
    /// </summary>
    /// <param name="callback">The callback associated when a player enters the lobby</param>
    private void OnLobbyEntered(LobbyEnter_t callback){
        // Server: change the ui to the lobby ui screen
        if(NetworkServer.active){
            if(uIManagar != null){
                string lobbyName = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name");
                uIManagar.lobbyScreen(callback.m_ulSteamIDLobby, lobbyName);            
            }
        }

        // Client: checks if the lobby exists, if it does not, runs invalid lobby and short circuit the client code
        if(!NetworkServer.active){
            if(string.IsNullOrWhiteSpace(SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey))){
                uIManagar.invalidLobby();
                return;
            }
            
            // If the lobby does exist, change the ui screen to lobby ui
            if(uIManagar != null){
                string lobbyName = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name");
                uIManagar.lobbyScreen(callback.m_ulSteamIDLobby, lobbyName);            
            }
            
            // Assign the network address for the client's manager and start the client 
            manager.networkAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
            manager.StartClient();        
        }

        // Everyone: nothing right now
    }

    /// <summary>
    /// Creates lobby 
    /// </summary>
    public void HostLobby(){
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, manager.maxConnections);
    }

    /// <summary>
    /// Joins the lobby based on a lobbyID
    /// </summary>
    /// <param name="lobbyID">The unique steam lobby ID</param>
    public void JoinLobby(ulong lobbyID) {
        CSteamID cLobbyID = new CSteamID(lobbyID);

        // Checks if the lobbyID is a valid ID
        if(cLobbyID.IsValid()){
            SteamMatchmaking.JoinLobby(cLobbyID);
        }else{
            uIManagar.invalidLobby();
        }
    }

    /// <summary>
    /// Leaves the steam lobby 
    /// </summary>
    /// <param name="lobbyID">The unique steam lobby ID</param>
    public void LeaveLobby(ulong lobbyID){
        // If you are the host, it will set the lobby to not joinable and delete the HostAddressKey
        if(NetworkServer.active){
            manager.StopHost();
            SteamMatchmaking.SetLobbyJoinable(new CSteamID(lobbyID), false);
            SteamMatchmaking.DeleteLobbyData(new CSteamID(lobbyID), HostAddressKey);
        }
        // If you are the client, just stops the client 
        else{
            manager.StopClient();
        }
        
        SteamMatchmaking.LeaveLobby(new CSteamID(lobbyID));
    }

    /// <summary>
    /// Helper function used in UIManager, if you are the host you can start the game
    /// </summary>
    public void StartGame(){
        if(!NetworkServer.active){
            return;
        }

        manager.ServerChangeScene("Gameplay");
    }

    /// <summary>
    /// Returns if you are the host of the lobby
    /// </summary>
    /// <returns>If you are or are not the host</returns>
    public bool isHost(){
        return NetworkServer.active;
    }
}