using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mono.CecilX;
using Org.BouncyCastle.Asn1.Crmf;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

public class UITesting : MonoBehaviour
{    
    private const string GeneralTab = "Setting-General-Container";
    private const string AudioTab = "Setting-Audio-Container";
    private const string ControlsTab = "Setting-Controls-Container";
    [SerializeField] PlayableDirector menuOpenClose;
    [SerializeField] PlayableDirector switchSettingTabs;
    [SerializeField] PlayableDirector showTab;
    [SerializeField] UIDocument uIDocument;
    private Dictionary<string, VisualElement> tabElements = new Dictionary<string, VisualElement>();
    private Dictionary<string, TimelineAsset> timelines = new Dictionary<string, TimelineAsset>();
    private VisualElement _rootVisualElement;
    private VisualElement _settingsMenu;
    private VisualElement _navContainer;
    private string currentTab;
    private bool isOpen;


    void Start(){
        TimelineAsset[] loadedTimelines = Resources.LoadAll<TimelineAsset>("UI/Timeline/");
        foreach (TimelineAsset timeline in loadedTimelines){
            timelines.Add(timeline.name, timeline);
        }

        isOpen = false;
        _rootVisualElement = uIDocument.rootVisualElement;
        currentTab = "Setting-General-Container";
        if(UnityUtils.ContainsElement(_rootVisualElement, "Settings-Container", out VisualElement settings)){
            _settingsMenu = settings;
            if(UnityUtils.ContainsElement(settings, "Setting-Nav-Container", out VisualElement nav)){
                _navContainer = nav;
            }
            RegisterVisEle(settings, GeneralTab);
            RegisterVisEle(settings, AudioTab);
            RegisterVisEle(settings, ControlsTab);
        }

        RegisterButton("Setting-GeneralBtn", () => SwitchTab(GeneralTab));
        RegisterButton("Setting-AudioBtn", () => SwitchTab(AudioTab));
        RegisterButton("Setting-ControlsBtn", () => SwitchTab(ControlsTab));
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("escape")){
            OpenMenu();
        }
    }

    private void OpenMenu(){
        if(!isOpen){
            menuOpenClose.playableAsset = timelines["OpenSettings"];
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
        }else{
            TimelineAsset closeTimeline = timelines["CloseSettings"];
            menuOpenClose.playableAsset = closeTimeline;
            tabElements[GeneralTab].visible = true;     
            menuOpenClose.Play();   
            isOpen = false;
        }         
    }
    private void SwitchTab(string newTab){
        if(currentTab != newTab){
            if(switchSettingTabs.state == PlayState.Playing) return;

            // Disgusting hardcode-in string using switch statement
            // Mainly used to load the correct timeline
            switch (currentTab){
                case "Setting-General-Container":
                    switchSettingTabs.playableAsset = timelines["SwitchGeneralTab"];
                    break;
                case "Setting-Audio-Container":
                    switchSettingTabs.playableAsset = timelines["SwitchAudioTab"];
                    break;
                case "Setting-Controls-Container":
                    switchSettingTabs.playableAsset = timelines["SwitchControlsTab"];
                    break;
                default:
                    Debug.LogWarning("Invalid Tab");
                    break;
            }
            switch (newTab){
                case "Setting-General-Container":
                    showTab.playableAsset = timelines["ShowGeneralTab"];
                    break;
                case "Setting-Audio-Container":
                    showTab.playableAsset = timelines["ShowAudioTab"];
                    break;
                case "Setting-Controls-Container":
                    showTab.playableAsset = timelines["ShowControlsTab"];
                    break;
                default:
                    Debug.LogWarning("Invalid Tab");
                    break;
            }

            // Play animation
            switchSettingTabs.Play();
            showTab.Play();

            // Manage which container gets the active container class to keep track
            tabElements[currentTab].RemoveFromClassList("settingActiveContainer");
            currentTab = newTab;
            tabElements[currentTab].AddToClassList("settingActiveContainer");
            tabElements[currentTab].visible = true; //show new tab
        }
    }

    private void RegisterVisEle(VisualElement root, string name){
        if(UnityUtils.ContainsElement(root, name, out VisualElement output)){
            tabElements[name] = output;
            output.visible = false;
        }
    }
    private void RegisterButton(string name, System.Action onClick){
        if(UnityUtils.ContainsElement(_navContainer, name, out Button output)){
            output.clicked += onClick;
        }
    }
}
