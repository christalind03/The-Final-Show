using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    // Gameobjects
    private SteamLobby steamLobby;
    public VisualElement ui;
    private VisualElement _menuView;
    private VisualElement _lobbyView;
    private VisualElement _lobbyJoinView;
    private TextElement _lobbyNameText;
    private Button _hostButton;
    private Button _joinButton;
    private Button _leaveButtonLobbyView;
    private Button _startButtonLobbyView;
    private Button _leaveButtonJoinView;
    private Button _exitButton;

    private void Start (){
        GameObject NetworkManager = GameObject.Find("NetworkManager");
        steamLobby = NetworkManager.GetComponent<SteamLobby>();
    }

    private void Awake() {
        ui = GetComponent<UIDocument>().rootVisualElement;
        _menuView = ui.Q<VisualElement>("LobbyMenu");
        _lobbyView = ui.Q<VisualElement>("LobbyInfo");
        _lobbyJoinView = ui.Q<VisualElement>("LobbyJoin");
    }

    private void OnEnable() {
        _hostButton = _menuView.Q<VisualElement>("Container").Q<Button>("hostButton");
        _hostButton.clicked += HostLobby;

        _joinButton = _menuView.Q<VisualElement>("Container").Q<Button>("joinButton");
        _joinButton.clicked += JoinScreen;
        
        _exitButton = _menuView.Q<VisualElement>("Container").Q<Button>("exitButton");
        _exitButton.clicked += ExitGame;

        _leaveButtonLobbyView = _lobbyView.Q<VisualElement>("lobbyContainer").Q<Button>("leaveButton");
        _leaveButtonLobbyView.clicked += LeaveLobby;

        _leaveButtonJoinView = _lobbyJoinView.Q<VisualElement>("lobbyContainer").Q<Button>("backButton");
        _leaveButtonJoinView.clicked += LeaveJoin;

        _startButtonLobbyView = _lobbyView.Q<VisualElement>("lobbyContainer").Q<Button>("startButton");
        _startButtonLobbyView.clicked += StartGame;

    }
    
    private void LeaveJoin(){
        SwitchUI(_lobbyJoinView, _menuView);
    }

    private void StartGame(){
        steamLobby.StartGame();
    }

    private void LeaveLobby(){
        SwitchUI(_lobbyView, _menuView);
        ulong lobbyIDField = _lobbyView.Q<VisualElement>("lobbyContainer").Q<UnsignedLongField>("id").value;
        steamLobby.LeaveLobby(lobbyIDField);
    }

    private void ExitGame(){
        Application.Quit();
    }

    private void SwitchUI(VisualElement curScreen, VisualElement newScreen){
        if(curScreen != null){
            curScreen.RemoveFromClassList("show");
            curScreen.AddToClassList("hide");
            curScreen.SetEnabled(false);            
        }

        if(newScreen != null){
            newScreen.RemoveFromClassList("hide");
            newScreen.AddToClassList("show");
            newScreen.SetEnabled(true);            
        }
    }

    private void JoinLobby(ulong lobbyID){
        steamLobby.JoinLobby(lobbyID);
    }
    
    private void HostLobby(){
        steamLobby.HostLobby();
    }
    private void JoinScreen(){
        Button joinLobbyBtn = _lobbyJoinView.Q<VisualElement>("lobbyContainer").Q<Button>("joinButton");
        UnsignedLongField idField= _lobbyJoinView.Q<VisualElement>("lobbyContainer").Q<UnsignedLongField>("id");
        idField.value = 0;
        SwitchUI(_menuView, _lobbyJoinView);
        joinLobbyBtn.clicked += () => JoinLobby(idField.value);

    }   

    private void enableJoinHost(){
        if(!steamLobby.isHost()){
            _startButtonLobbyView.RemoveFromClassList("show");
            _startButtonLobbyView.AddToClassList("hide");
            _startButtonLobbyView.SetEnabled(false);  
        }else{
            _startButtonLobbyView.RemoveFromClassList("hide");
            _startButtonLobbyView.AddToClassList("show");
            _startButtonLobbyView.SetEnabled(true);  
        }
    }

    public void lobbyScreen(ulong lobbyID, string lobbyName){
        enableJoinHost();   
        SwitchUI(_menuView, _lobbyView);
        SwitchUI(_lobbyJoinView, _lobbyView);

        _lobbyNameText = _lobbyView.Q<VisualElement>("lobbyContainer").Q<TextElement>("text");
        _lobbyNameText.text = lobbyName;

        UnsignedLongField lobbyIDField = _lobbyView.Q<VisualElement>("lobbyContainer").Q<UnsignedLongField>("id");
        lobbyIDField.value = lobbyID;
    }
    
    public void invalidLobby(){
        JoinScreen();
    }
}
