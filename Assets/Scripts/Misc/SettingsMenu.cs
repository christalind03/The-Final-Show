using Mirror;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.InputSystem;

public class SettingsMenu : NetworkBehaviour
{
    // UI variables
    private const string GeneralTab = "Setting-General-Container";
    private const string AudioTab = "Setting-Audio-Container";
    private const string ControlsTab = "Setting-Controls-Container";
    [SerializeField] PlayableDirector menuOpenClose;
    [SerializeField] PlayableDirector switchSettingTabs;
    [SerializeField] PlayableDirector showTab;
    [SerializeField] UIDocument uIDocument;
    [SerializeField] TimelineAsset menuOpenAnim;
    [SerializeField] TimelineAsset menuCloseAnim;
    private Dictionary<string, VisualElement> tabElements = new Dictionary<string, VisualElement>();
    private Dictionary<string, (Button button, System.Action action)> buttonActions = new Dictionary<string, (Button button, System.Action action)>();


    private VisualElement _rootVisualElement;
    private VisualElement _settingsMenu;
    private VisualElement _navContainer;
    private string currentTab;
    private bool isOpen;

    // Rebind variables
    [SerializeField] private InputActionAsset inputActions;
    private InputActionRebindingExtensions.RebindingOperation _rebindOperation;

    
    // Misc
    private SteamLobby steamLobby;

    /// <summary>
    /// Setup the required variables.
    /// Setup the lobby code.
    /// </summary>
    public override void OnStartAuthority()
    {
        Setup();
        base.OnStartAuthority();
    }
    /// <summary>
    /// Unsubscribe from the button action 
    /// </summary>
    public override void OnStopAuthority()
    {
        UnsubActions();
        base.OnStopAuthority();
    }

    #region Functionality
    /// <summary>
    /// Assigns and populate all the required UI reference when the game starts up
    /// </summary>
    private void Setup(){
        steamLobby = NetworkManager.FindObjectOfType<SteamLobby>();
        
        _rootVisualElement = uIDocument.rootVisualElement;
        string lobbyId = NetworkManager.FindObjectOfType<CustomNetworkManager>().LobbyId;
        isOpen = false;
        currentTab = "Setting-General-Container";

        if(UnityUtils.ContainsElement(_rootVisualElement, "Settings-Container", out VisualElement settings)){
            _settingsMenu = settings;
            if(UnityUtils.ContainsElement(settings, "Setting-Nav-Container", out VisualElement nav)){
                _navContainer = nav;
            }

            RegisterVisEle(settings, GeneralTab);
            RegisterVisEle(settings, AudioTab);
            RegisterVisEle(settings, ControlsTab);

            if(UnityUtils.ContainsElement(tabElements[GeneralTab], "Id", out TextField LobbyText)){
                LobbyText.value = lobbyId;
            }
        }

        RegisterButton(_navContainer, "Setting-GeneralBtn", () => SwitchTab(GeneralTab), out Button GeneralBtn);
        RegisterButton(_navContainer, "Setting-AudioBtn", () => SwitchTab(AudioTab), out Button AudioBtn);
        RegisterButton(_navContainer, "Setting-ControlsBtn", () => SwitchTab(ControlsTab), out Button ControlsBtn);
        RegisterButton(tabElements[GeneralTab], "LeaveBtn", OnBtnLeaveGame, out Button LeaveBtn);

        // Rebind buttons
        Button ForwardBtn = null, BackwardBtn = null, LeftBtn = null, RightBtn = null, JumpBtn = null;
        Button InteractOrEquipBtn = null, DropBtn = null;
        Button AttackBtn = null, AltAttackBtn = null;
        RegisterButton(tabElements[ControlsTab], "ForwardBtn", () => StartRebind("Movement", ForwardBtn, 1), out ForwardBtn);
        RegisterButton(tabElements[ControlsTab], "BackwardBtn", () => StartRebind("Movement", BackwardBtn, 3), out BackwardBtn);
        RegisterButton(tabElements[ControlsTab], "LeftBtn", () => StartRebind("Movement", LeftBtn, 2), out LeftBtn);
        RegisterButton(tabElements[ControlsTab], "RightBtn", () => StartRebind("Movement", RightBtn, 4), out RightBtn);
        RegisterButton(tabElements[ControlsTab], "JumpBtn", () => StartRebind("Jump", JumpBtn), out JumpBtn);
        RegisterButton(tabElements[ControlsTab], "InteractOrEquipBtn", () => StartRebind("Interact", InteractOrEquipBtn), out InteractOrEquipBtn);
        RegisterButton(tabElements[ControlsTab], "DropBtn", () => StartRebind("Drop", DropBtn), out DropBtn);
        RegisterButton(tabElements[ControlsTab], "AttackBtn", () => StartRebind("Attack", AttackBtn), out AttackBtn);
        RegisterButton(tabElements[ControlsTab], "AltAttackBtn", () => StartRebind("Alternate Attack", AltAttackBtn), out AltAttackBtn);
    } 

    /// <summary>
    /// List of all buttons that needs to unsub their actions
    /// </summary>
    private void UnsubActions(){
        foreach(var kvp in buttonActions){
            var (button, action) = kvp.Value;
            if (button != null) button.clicked -= action;
        }
        buttonActions.Clear();
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

    /// <summary>
    /// Adds the required visual elements into a dictionary for easier reference
    /// </summary>
    /// <param name="root">visual element of the root where the query should start</param>
    /// <param name="name">the name of the target visual element</param>
    private void RegisterVisEle(VisualElement root, string name){
        if(UnityUtils.ContainsElement(root, name, out VisualElement output)){
            tabElements[name] = output;
            output.visible = false;
        }
    }

    /// <summary>
    /// Subscribe button to their respective action
    /// </summary>
    /// <param name="root">the root visual element of where to start query</param>
    /// <param name="name">name of the button element</param>
    /// <param name="onClick">the action that needs to be performed when clicked</param>
    private void RegisterButton(VisualElement root, string name, System.Action onClick, out Button button){
        if(UnityUtils.ContainsElement(root, name, out Button output)){
            output.clicked += onClick;
            button = output;
            buttonActions.Add(name, (output, onClick));
        }else{
            button = null;
        }
    }

    /// <summary>
    /// Unsubscribe button with their respective actio
    /// </summary>
    /// <param name="root">the root visual element of where to start query</param>
    /// <param name="name">name of the button element</param>
    /// <param name="onClick">the action that needs to be unsub</param>
    private void UnregisterButton(VisualElement root, string name, System.Action onClick){
        if(UnityUtils.ContainsElement(root, name, out Button output)){
            output.clicked -= onClick;
        }
    }

    private void StartRebind(string actionName, Button rebindButton, int compositeIndx = 0)
    {
        _rebindOperation?.Cancel();

        void CleanUp(){
            _rebindOperation?.Dispose();
            _rebindOperation = null;
        }
        
        InputAction action = inputActions.FindAction(actionName);
        if (action == null) return;

        if (action.enabled) action.Disable();
        rebindButton.text = "Press any key...";

        _rebindOperation?.Cancel();
        _rebindOperation = action.PerformInteractiveRebinding(compositeIndx)
            .OnCancel(operation =>
            {
                action.Enable();
                CleanUp();
            })
            .OnComplete(operation =>
            {
                action.Enable();
                UpdateRebindButtonText(action, rebindButton, compositeIndx);
                CleanUp();
            })
            .Start();
    }

    private void UpdateRebindButtonText(InputAction action, Button rebindButton, int compositeIndx = 0)
    {
        if (action != null && action.bindings.Count > 0){
            rebindButton.text = InputControlPath.ToHumanReadableString(
                action.bindings[compositeIndx].effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice);
        }
        else{
            rebindButton.text = "Not Bound";
        }
    }
    #endregion

    #region UI Management 
    /// <summary>
    /// Open or closes the setting menu depending on the state of the menu currently
    /// </summary>
    /// <returns>return true for open menu, false otherwise</returns>
    public bool OpenMenu(){
        if(!isOpen){
            menuOpenClose.playableAsset = menuOpenAnim;
            _settingsMenu.RemoveFromClassList("secondaryContainer");
            _settingsMenu.AddToClassList("primaryContainer");
            foreach(var elementsPair in tabElements){
                if(elementsPair.Key != GeneralTab){
                    elementsPair.Value.visible = false;
                }else{
                    elementsPair.Value.visible = true;
                }
            }
            currentTab = GeneralTab;
            tabElements[GeneralTab].visible = true;
            tabElements[GeneralTab].AddToClassList("settingActiveContainer");
            menuOpenClose.Play();   
            isOpen = true;
            return true;
        }else{
            TimelineAsset closeTimeline = menuCloseAnim;
            menuOpenClose.playableAsset = closeTimeline;
            tabElements[GeneralTab].visible = true;     
            menuOpenClose.Play();   
            isOpen = false;
        }         
        return false;
    }

    /// <summary>
    /// Animationing and logic for switching the setting tabs
    /// </summary>
    /// <param name="newTab">the new tab that needs to be displayed</param>
    private void SwitchTab(string newTab){
        if(currentTab != newTab){
            if(switchSettingTabs.state == PlayState.Playing) return;

            // Manage which container in front
            tabElements[newTab].BringToFront();

            // Play animation and manage class to anim correct container
            tabElements[currentTab].AddToClassList("settingTransitionContainer");
            switchSettingTabs.Play();
            tabElements[currentTab].RemoveFromClassList("settingActiveContainer");
            currentTab = newTab;
            tabElements[currentTab].AddToClassList("settingActiveContainer");
            showTab.Play();

            // Remove class once done
            switchSettingTabs.stopped += (PlayableDirector director) =>{
                tabElements[currentTab].RemoveFromClassList("settingTransitionContainer");
            };

            tabElements[currentTab].visible = true; //show new tab
        }
    }
    #endregion
}
