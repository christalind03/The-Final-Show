using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingsMenu : NetworkBehaviour
{
    
    private VisualElement _rootVisualElement;
    private Button _leaveGameBtn;
    private SteamLobby steamLobby;
    /// <summary>
    /// Setup the required variables.
    /// Setup the lobby code.
    /// </summary>
    public override void OnStartAuthority()
    {

        steamLobby = NetworkManager.FindObjectOfType<SteamLobby>();
        
        _rootVisualElement = gameObject.GetComponent<UIDocument>().rootVisualElement;
        string lobbyId = NetworkManager.FindObjectOfType<CustomNetworkManager>().LobbyId;

        if(UnityUtils.ContainsElement(_rootVisualElement, "Settings-Container", out VisualElement settingContainer)){
            if(UnityUtils.ContainsElement(settingContainer, "Id", out TextField LobbyText)){
                LobbyText.value = lobbyId;
            }

            if(UnityUtils.ContainsElement(settingContainer, "LeaveBtn", out Button leaveBtn)){
                _leaveGameBtn = leaveBtn;
            }
        }

        _leaveGameBtn.clicked += OnBtnLeaveGame;
        base.OnStartAuthority();
    }
    /// <summary>
    /// Unsubscribe from the button action 
    /// </summary>
    public override void OnStopAuthority()
    {
        _leaveGameBtn.clicked -= OnBtnLeaveGame; 
        base.OnStopAuthority();
    }

    /// <summary>
    /// Toggles on and off the settings menu.
    /// </summary>
    /// <returns>if settings menu is on, returns true. False otherwise</returns>
    public bool ToggleSettingsMenu(){
        if(UnityUtils.ContainsElement(_rootVisualElement, "Settings-Container", out VisualElement settingsMenu)){
            if(settingsMenu.ClassListContains("secondaryContainer")){
                settingsMenu.RemoveFromClassList("secondaryContainer");
                settingsMenu.AddToClassList("primaryContainer");
                return true;
            }else{
                settingsMenu.RemoveFromClassList("primaryContainer");
                settingsMenu.AddToClassList("secondaryContainer");
            }
        }
        return false;
    }

    /// <summary>
    /// Stops the host or client when the leave the game
    /// </summary>
    private void OnBtnLeaveGame(){
        if(isServer){
            NetworkManager.singleton.StopHost();
        }else{
            NetworkManager.singleton.StopClient();
        }

        // Delete the network game object when the player leaves the game. Doesn't work for some reason....
        CustomNetworkManager networkManager = GameObject.FindObjectOfType<CustomNetworkManager>();
        Destroy(networkManager.gameObject);
    }
}
