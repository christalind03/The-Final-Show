using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Managers the lobby UI 
/// </summary>
public class UIManager : MonoBehaviour
{
    // Gameobjects
    public VisualElement ui;
    private SteamLobby steamLobby;
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

    /// <summary>
    /// Get the different components needed to run the script
    /// </summary>
    private void Start (){
        GameObject NetworkManager = GameObject.Find("NetworkManager");
        steamLobby = NetworkManager.GetComponent<SteamLobby>();
    }

    /// <summary>
    /// Initializes the different UI views 
    /// </summary>
    private void Awake() {
        ui = GetComponent<UIDocument>().rootVisualElement;
        _menuView = ui.Q<VisualElement>("LobbyMenu");
        _lobbyView = ui.Q<VisualElement>("LobbyInfo");
        _lobbyJoinView = ui.Q<VisualElement>("LobbyJoin");
    }

    /// <summary>
    /// Subscribes the different functions for when an action happens for button pressed
    /// </summary>
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
    
    /// <summary>
    /// Switch UI to main menu from the join menu
    /// </summary>
    private void LeaveJoin(){
        SwitchUI(_lobbyJoinView, _menuView);
    }

    /// <summary>
    /// Start the game if you are the host 
    /// </summary>
    private void StartGame(){
        steamLobby.StartGame();
    }

    /// <summary>
    /// Switch UI to menu when player leave the lobby 
    /// </summary>
    private void LeaveLobby(){
        SwitchUI(_lobbyView, _menuView);
        ulong lobbyIDField = _lobbyView.Q<VisualElement>("lobbyContainer").Q<UnsignedLongField>("id").value; // gets the unique lobbyID required to leave the lobby
        steamLobby.LeaveLobby(lobbyIDField);
    }

    /// <summary>
    /// Close the game 
    /// </summary>
    private void ExitGame(){
        Application.Quit();
    }

    /// <summary>
    /// Helper function to switch to different UI views
    /// </summary>
    /// <param name="curScreen">The current screen the player is on</param>
    /// <param name="newScreen">The new screen that the player should be viewing</param>
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

    /// <summary>
    /// Joins the steam lobby using the lobbyID
    /// </summary>
    /// <param name="lobbyID">Unique steam lobbyID</param>
    private void JoinLobby(ulong lobbyID){
        steamLobby.JoinLobby(lobbyID);
    }
    
    /// <summary>
    /// Host the lobby
    /// </summary>
    private void HostLobby(){
        steamLobby.HostLobby();
    }

    /// <summary>
    /// Initializes the join screen and all the functions attached to it
    /// </summary>
    private void JoinScreen(){
        // Get the button and id field from the current screen 
        Button joinLobbyBtn = _lobbyJoinView.Q<VisualElement>("lobbyContainer").Q<Button>("joinButton");
        UnsignedLongField idField= _lobbyJoinView.Q<VisualElement>("lobbyContainer").Q<UnsignedLongField>("id");

        // Reset the id field to 0 when you transition into it
        idField.value = 0;

        // Switch the screen to lobby view 
        SwitchUI(_menuView, _lobbyJoinView);

        // Subscribes to the join lobby function 
        joinLobbyBtn.clicked += () => JoinLobby(idField.value);
    }   

    /// <summary>
    /// If you are the host, enble the join button in lobby screen
    /// </summary>
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

    /// <summary>
    /// Initalizes the lobby screen screen 
    /// </summary>
    /// <param name="lobbyID">Unique steam lobbyID</param>
    /// <param name="lobbyName">Name of the lobby</param>
    public void lobbyScreen(ulong lobbyID, string lobbyName){
        enableJoinHost();   
        SwitchUI(_menuView, _lobbyView);
        SwitchUI(_lobbyJoinView, _lobbyView);

        _lobbyNameText = _lobbyView.Q<VisualElement>("lobbyContainer").Q<TextElement>("text");
        _lobbyNameText.text = lobbyName;

        UnsignedLongField lobbyIDField = _lobbyView.Q<VisualElement>("lobbyContainer").Q<UnsignedLongField>("id");
        lobbyIDField.value = lobbyID;
    }
    
    /// <summary>
    /// Wrapper function for join screen function used by the steam lobby script 
    /// </summary>
    public void invalidLobby(){
        JoinScreen();
    }
}
