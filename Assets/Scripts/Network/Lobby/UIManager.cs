using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Managers the lobby UI 
/// </summary>
public class UIManager : MonoBehaviour
{
    // Gameobjects
    public VisualElement rootVisualElement;
    private SteamLobby steamLobby;
    private VisualElement _menuView;
    private VisualElement _joinView;
    private Button _mainHostButton;
    private Button _mainJoinButton;
    private Button _mainExitButton;
    private Button _joinJoinButton;
    private Button _joinBackButton;

    /// <summary>
    /// Get the different components needed to run the script
    /// </summary>
    private void Start()
    {
        GameObject NetworkManager = GameObject.Find("NetworkManager");
        steamLobby = NetworkManager.GetComponent<SteamLobby>();
    }

    /// <summary>
    /// Initializes the different UI views 
    /// </summary>
    private void Awake()
    {
        rootVisualElement = GetComponent<UIDocument>().rootVisualElement;

        // Find and save the different views for navigating menu
        // VisualElements
        if(UnityUtils.ContainsElement(rootVisualElement, "LobbyMenu", out VisualElement mainmenu)){
            _menuView = mainmenu;
        }
        if(UnityUtils.ContainsElement(rootVisualElement, "LobbyJoin", out VisualElement joinmenu)){
            _joinView = joinmenu;
        }

        // Buttons - main menu
        if(UnityUtils.ContainsElement(mainmenu, "hostButton", out Button mainHostBtn)){
            _mainHostButton = mainHostBtn;
        }
        if(UnityUtils.ContainsElement(mainmenu, "joinButton", out Button mainJoinBtn)){
            _mainJoinButton = mainJoinBtn;
        }
        if(UnityUtils.ContainsElement(mainmenu, "exitButton", out Button mainExitBtn)){
            _mainExitButton = mainExitBtn;
        }

        // Buttons - join menu
        if(UnityUtils.ContainsElement(_joinView, "joinButton", out Button joinJoinBtn)){
            _joinJoinButton = joinJoinBtn;
        }
        if(UnityUtils.ContainsElement(_joinView, "backButton", out Button joinBackBtn)){
            _joinBackButton = joinBackBtn;
        }
    }

    /// <summary>
    /// Subscribes the different functions for when an action happens for button pressed
    /// </summary>
    private void OnEnable()
    {
        _mainHostButton.clicked += HostLobby;
        _mainJoinButton.clicked += JoinScreen;
        _mainExitButton.clicked += ExitGame;
        _joinJoinButton.clicked += JoinLobby;
        _joinBackButton.clicked += LeaveJoin;
    }

    /// <summary>
    /// Close the game 
    /// </summary>
    private void ExitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// Switch UI to main menu from the join menu
    /// </summary>
    private void LeaveJoin()
    {
        SwitchUI(_joinView, _menuView);
    }

    /// <summary>
    /// Host the lobby
    /// </summary>
    private void HostLobby()
    {
        steamLobby.HostLobby();
    }

    /// <summary>
    /// Change from main menu screen to join screen
    /// </summary>
    private void JoinScreen(){
        SwitchUI(_menuView, _joinView);
    }

    /// <summary>
    /// Initializes the join screen and all the functions attached to it
    /// </summary>
    private void JoinLobby()
    {
        TextField idField = new TextField("");
        // Get the id field from the current screen and save the value
        if(UnityUtils.ContainsElement(_joinView, "id", out TextField id)){
            idField = id;
        }

        steamLobby.JoinLobby(idField.text);
    }

    /// <summary>
    /// Wrapper function for join screen function used by the steam lobby script 
    /// </summary>
    public void invalidLobby()
    {
        JoinScreen();
    }

    /// <summary>
    /// Helper function to switch to different UI views
    /// </summary>
    /// <param name="curScreen">The current screen the player is on</param>
    /// <param name="newScreen">The new screen that the player should be viewing</param>
    private void SwitchUI(VisualElement curScreen, VisualElement newScreen)
    {
        if (curScreen != null)
        {
            curScreen.RemoveFromClassList("show");
            curScreen.AddToClassList("hide");
            curScreen.SetEnabled(false);
        }

        if (newScreen != null)
        {
            newScreen.RemoveFromClassList("hide");
            newScreen.AddToClassList("show");
            newScreen.SetEnabled(true);
        }
    }
}