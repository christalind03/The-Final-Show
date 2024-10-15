using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using Edgegap;


public class SteamLobby : MonoBehaviour
{
    // Callbacks for lobby 
    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> LobbyRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;

    // Variables
    public ulong CurrentLobbyID;
    private const string HostAddressKey = "HostAddress";
    private CustomNetworkManager manager; 

    // Gameobjects
    private UIManager uIManagar;

    private void Start() {
        GameObject lobbyUI = GameObject.Find("LobbyUI");
        uIManagar = lobbyUI.GetComponent<UIManager>();
        if(!SteamManager.Initialized){ return; }

        manager = GetComponent<CustomNetworkManager>();
        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        LobbyRequest = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyRequest);

    }
    
    
    public void HostLobby(){
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, manager.maxConnections);
    }

    public void JoinLobby(ulong lobbyID) {
        CSteamID cLobbyID = new CSteamID(lobbyID);
        if(cLobbyID.IsValid()){
            SteamMatchmaking.JoinLobby(cLobbyID);
        }else{
            uIManagar.invalidLobby();
        }
    }

    public void LeaveLobby(ulong lobbyID){
        // Host logic
        if(NetworkServer.active){
            manager.StopHost();
            SteamMatchmaking.DeleteLobbyData(new CSteamID(lobbyID), HostAddressKey);
        }
        // Client logic
        else{
            manager.StopClient();
        }
        
        SteamMatchmaking.LeaveLobby(new CSteamID(lobbyID));
    }

    
    private void OnLobbyRequest(GameLobbyJoinRequested_t callback){
        
    }

    private void OnLobbyCreated(LobbyCreated_t callback){
        if(callback.m_eResult != EResult.k_EResultOK){ return;}

        manager.StartHost();

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", SteamFriends.GetPersonaName().ToString() + "'s Lobby");

    }

    private void OnLobbyEntered(LobbyEnter_t callback){
        // Server
        if(NetworkServer.active){
            if(uIManagar != null){
                string lobbyName = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name");
                uIManagar.lobbyScreen(callback.m_ulSteamIDLobby, lobbyName);            
            }
        }

        // Client 
        if(!NetworkServer.active){
            manager.networkAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
            if(string.IsNullOrWhiteSpace(manager.networkAddress)){
                SteamMatchmaking.LeaveLobby(new CSteamID(callback.m_ulSteamIDLobby));
                uIManagar.invalidLobby();
                return;
            }

            if(SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey) == ""){
                SteamMatchmaking.LeaveLobby(new CSteamID(callback.m_ulSteamIDLobby));
                uIManagar.invalidLobby();
                return;
            }

            if(uIManagar != null){
                string lobbyName = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name");
                uIManagar.lobbyScreen(callback.m_ulSteamIDLobby, lobbyName);            
            }
            manager.StartClient();        
        }

        // Everyone 
    }
}