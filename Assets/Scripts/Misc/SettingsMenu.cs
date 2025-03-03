using Mirror;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.InputSystem;
using UnityEngine.Audio;

public class SettingsMenu : NetworkBehaviour
{
    // UI variables
    private const string GeneralTab = "Setting-General-Container";
    private const string AudioTab = "Setting-Audio-Container";
    private const string ControlsTab = "Setting-Controls-Container";
    [SerializeField] private PlayableDirector menuOpenClose;
    [SerializeField] private PlayableDirector switchSettingTabs;
    [SerializeField] private PlayableDirector showTab;
    [SerializeField] private UIDocument uIDocument;
    [SerializeField] private TimelineAsset menuOpenAnim;
    [SerializeField] private TimelineAsset menuCloseAnim;
    public Dictionary<string, VisualElement> tabElements = new Dictionary<string, VisualElement>();
    public Dictionary<string, Slider> sliderElements = new Dictionary<string, Slider>();
    public Dictionary<string, DropdownField> dropdownElements = new Dictionary<string, DropdownField>();
    public Dictionary<string, (Button button, System.Action action)> buttonActions = new Dictionary<string, (Button button, System.Action action)>();
    public Dictionary<string, Button> controlButtonMap = new Dictionary<string, Button>();


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
    PlayerController controller;
    [SerializeField] private AudioMixer mixer;

    /// <summary>

    /// <summary>
    /// Setup the required variables.
    /// Setup the lobby code.
    /// </summary>
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        Setup();
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
        // Set lobby id
        steamLobby = NetworkManager.FindObjectOfType<SteamLobby>();
        string lobbyId = NetworkManager.FindObjectOfType<CustomNetworkManager>().LobbyId;

        // UI variable
        _rootVisualElement = uIDocument.rootVisualElement;
        
        // animation variable
        isOpen = false;
        currentTab = "Setting-General-Container";

        controller = gameObject.transform.parent.GetComponent<PlayerController>();
        inputActions = controller.playerInput.actions;

        // Fill dictionary with correct reference of visual element
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

        // Non-rebind buttons
        RegisterButton(_navContainer, "Setting-GeneralBtn", () => SwitchTab(GeneralTab));
        RegisterButton(_navContainer, "Setting-AudioBtn", () => SwitchTab(AudioTab));
        RegisterButton(_navContainer, "Setting-ControlsBtn", () => SwitchTab(ControlsTab));
        RegisterButton(tabElements[GeneralTab], "LeaveBtn", OnBtnLeaveGame);

        // Rebind buttons
        RegisterControl(tabElements[ControlsTab], "MovementBtn", "Movement", () => StartInteractiveRebind("Movement"));
        RegisterControl(tabElements[ControlsTab], "JumpBtn", "Jump", () => StartInteractiveRebind("Jump"));
        RegisterControl(tabElements[ControlsTab], "SprintBtn", "Sprint", () => StartInteractiveRebind("Sprint"));
        RegisterControl(tabElements[ControlsTab], "InteractOrEquipBtn", "Interact", () => StartInteractiveRebind("Interact"));
        RegisterControl(tabElements[ControlsTab], "DropBtn", "Drop", () => StartInteractiveRebind("Drop"));
        RegisterControl(tabElements[ControlsTab], "AttackBtn", "Attack", () => StartInteractiveRebind("Attack"));
        RegisterControl(tabElements[ControlsTab], "AltAttackBtn", "Alternate Attack", () => StartInteractiveRebind("Alternate Attack"));
        
        // Screen Setting
        if(UnityUtils.ContainsElement(tabElements[GeneralTab], "ScreenSetting", out DropdownField screenDropdown)){
            dropdownElements.Add("ScreenSetting", screenDropdown);
            screenDropdown.RegisterValueChangedCallback(function => ScreenSetting());
        }

        // Camera Sens
        if(UnityUtils.ContainsElement(tabElements[GeneralTab], "CameraSens", out Slider cameraSlider)){
            sliderElements.Add("CameraSens", cameraSlider);
            cameraSlider.RegisterValueChangedCallback(function => CameraSens());
        }

        // Music Setting
        if(UnityUtils.ContainsElement(tabElements[AudioTab], "MusicSlider", out Slider musicSlider)){
            sliderElements.Add("MusicSlider", musicSlider);
            musicSlider.RegisterValueChangedCallback(function => AdjustSound("Music"));
        }
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
        sliderElements["CameraSens"].UnregisterValueChangedCallback(function => CameraSens());
        dropdownElements["ScreenSetting"].UnregisterValueChangedCallback(function => ScreenSetting());
        sliderElements["MusicSlider"].UnregisterValueChangedCallback(function => AdjustSound("Music"));
    }

    /// <summary>
    /// Stops the host or client when the leave the game
    /// </summary>
    private void OnBtnLeaveGame(){
        if(isServer && isClient){
            CustomNetworkManager.Instance.StopHost();
        }
        else if(isClient){
            CustomNetworkManager.Instance.StopClient();
        }

        // Delete stuff that are on Dont destroy on load
        Destroy(CustomNetworkManager.Instance.gameObject);
        Destroy(ScoreBoard.Instance.gameObject);
        Destroy(GameplayManager.Instance.gameObject);
        Destroy(GameplayAudio.Instance.gameObject);
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
    private void RegisterButton(VisualElement root, string name, System.Action onClick){
        if(UnityUtils.ContainsElement(root, name, out Button output)){
            output.clicked += onClick;
            buttonActions.Add(name, (output, onClick));
        }
    }

    /// <summary>
    /// Subscribe button to their respective action
    /// </summary>
    /// <param name="root">the root visual element of where to start query</param>
    /// <param name="name">name of the button element</param>
    /// <param name="onClick">the action that needs to be performed when clicked</param>
    private void RegisterControl(VisualElement root, string name, string actionName, System.Action onClick){
        if(UnityUtils.ContainsElement(root, name, out Button output)){
            output.clicked += onClick;
            buttonActions.Add(name, (output, onClick));
            controlButtonMap.Add(actionName, output);
        }
    }

    #region Control Tab
    /// NOTE: Based on the code from Unity rebind!! I've rewrote parts of it to fit our project better
    /// <summary>
    /// Return the action and binding index for the binding that is targeted by the component
    /// according to
    /// </summary>
    /// <param name="actionName"></param>
    /// <param name="action"></param>
    /// <param name="bindingIndex"></param>
    /// <returns></returns>
    public bool ResolveActionAndBinding(string actionName, out InputAction action, out int bindingIndex)
    {
        bindingIndex = -1;

        action = inputActions.FindAction(actionName);
        if (action == null)
            return false;

        // Look up binding index.
        for (int i = 0; i < action.bindings.Count; i++)
        {
            if (!string.IsNullOrEmpty(action.bindings[i].path)) // Adjust the condition as needed
            {
                bindingIndex = i;
                break;
            }
        }

        if (bindingIndex == -1)
        {
            return false;
        }

        return true;
    }

    /// NOTE: Taken from Unity rebind sample code!! 
    /// <summary>
    /// Initiate an interactive rebind that lets the player actuate a control to choose a new binding
    /// for the action.
    /// </summary>
    /// <param name="actionName"></param>
    public void StartInteractiveRebind(string actionName)
    {
        if (!ResolveActionAndBinding(actionName, out var action, out var bindingIndex))
            return;

        // If the binding is a composite, we need to rebind each part in turn.
        if (action.bindings[bindingIndex].isComposite)
        {
            var firstPartIndex = bindingIndex + 1;
            if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isPartOfComposite)
                PerformInteractiveRebind(action, firstPartIndex, allCompositeParts: true);
        }
        else
        {
            PerformInteractiveRebind(action, bindingIndex);
        }
    }

    /// NOTE: Based on the code from Unity rebind!! I've rewrote parts of it to fit our project better
    /// <summary>
    /// Performs the actual rebinding process of the input action
    /// </summary>
    /// <param name="action">the action being modified</param>
    /// <param name="bindingIndex">the binding index of the input action</param>
    /// <param name="allCompositeParts">if input action has composite bindings</param>
    private void PerformInteractiveRebind(InputAction action, int bindingIndex, bool allCompositeParts = false)
    {
        _rebindOperation?.Cancel(); 

        void CleanUp()
        {
            _rebindOperation?.Dispose();
            _rebindOperation = null;
        }

        if(action.enabled) action.Disable();

        controlButtonMap[action.name].text = "Waiting for Key";

        // Configure the rebind.
        _rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
            .WithCancelingThrough("<Keyboard>/escape")
            .OnCancel(
                operation =>
                {
                    action.Enable();
                    UpdateRebindButtonText(action, bindingIndex);
                    CleanUp();
                })
            .OnComplete(
                operation =>
                {
                    action.Enable();
                    if(CheckDuplicateBindings(action, bindingIndex, allCompositeParts)) {
                        action.RemoveBindingOverride(bindingIndex);
                        CleanUp();
                        PerformInteractiveRebind(action, bindingIndex, allCompositeParts);
                        return;
                    }   

                    UpdateRebindButtonText(action, bindingIndex, allCompositeParts);
                    CleanUp();

                    // If there's more composite parts we should bind, initiate a rebind
                    // for the next part.
                    if (allCompositeParts)
                    {
                        var nextBindingIndex = bindingIndex + 1;
                        if (nextBindingIndex < action.bindings.Count && action.bindings[nextBindingIndex].isPartOfComposite)
                            PerformInteractiveRebind(action, nextBindingIndex, true);
                    }
                });

        _rebindOperation.Start();
    }

    /// <summary>
    /// Update the display of the control button UIs
    /// </summary>
    /// <param name="action">the input action</param>
    /// <param name="bindingIndx">the binding index of the action</param>
    /// <param name="allCompositeParts">if the action has comoosite binding</param>
    private void UpdateRebindButtonText(InputAction action, int bindingIndx, bool allCompositeParts = false) {
        var displayString = string.Empty;
        var deviceLayoutName = default(string);
        var controlPath = default(string);

        if (action != null)
        {
            if (allCompositeParts) {
                List<string> compositeParts = new List<string>();

                for (int i = 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; i++) {
                    compositeParts.Add(action.GetBindingDisplayString(i, out deviceLayoutName, out controlPath));
                }

                displayString = string.Join(" / ", compositeParts);
            }
            else if (bindingIndx != -1)
                displayString = action.GetBindingDisplayString(bindingIndx, out deviceLayoutName, out controlPath);
        }

        controlButtonMap[action.name].text = displayString;
    }

    /// <summary>
    /// Checks for duplicate when rebinding a key
    /// </summary>
    /// <param name="action">the action</param>
    /// <param name="bindingIndx">the index of the rebind input action</param>
    /// <param name="allCompositeParts">if the action has composite bindings</param>
    /// <returns>true is dupe exist, false otherwise</returns>
    private bool CheckDuplicateBindings(InputAction action, int bindingIndx, bool allCompositeParts = false) {
        InputBinding newBinding = action.bindings[bindingIndx];
        foreach(InputBinding binding in action.actionMap.bindings) {
            if(binding.action == newBinding.action){
                continue;
            }
            if(binding.effectivePath == newBinding.effectivePath) {
                return true;
            }
        }

        if(allCompositeParts){
            for(int i = 0; i < bindingIndx; i++) {
                if(action.bindings[i].effectivePath == newBinding.effectivePath){
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region General Tab 
    /// <summary>
    /// Set the camera sensitivity based on the camera sens slider
    /// </summary>
    /// <param name="slider">the slider UI field</param>
    private void CameraSens() {
        Slider slider = sliderElements["CameraSens"];
        controller._cameraSensitivity = slider.value;
    }

    /// <summary>
    /// Set the screen setting based on the dropdown field value
    /// </summary>
    /// <param name="dropdown">the dropdown UI field</param>
    private void ScreenSetting() {
        DropdownField dropdown = dropdownElements["ScreenSetting"];
        switch(dropdown.index) {
            case 0:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
            case 1:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                int screenWidth = Screen.currentResolution.width;
                int screenHeight = Screen.currentResolution.height;
                Screen.SetResolution(screenWidth, screenHeight, FullScreenMode.Windowed);
                break;
            default:
                break;
        }
    }
    #endregion

    #region Audio Tab 
    /// <summary>
    /// Used to adjust volume for the targeted sound type
    /// </summary>
    /// <param name="soundType">the group in audio mixer</param>
    private void AdjustSound(string soundType){
        float volume = sliderElements["MusicSlider"].value;
        mixer.SetFloat(soundType, Mathf.Log10(volume)*20);
    }

    #endregion
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
