using System.Collections.Generic;
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
    [SerializeField] TimelineAsset menuOpenAnim;
    [SerializeField] TimelineAsset menuCloseAnim;
    private Dictionary<string, VisualElement> tabElements = new Dictionary<string, VisualElement>();

    private VisualElement _rootVisualElement;
    private VisualElement _settingsMenu;
    private VisualElement _navContainer;
    private string currentTab;
    private bool isOpen;


    void Start(){
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
        }else{
            TimelineAsset closeTimeline = menuCloseAnim;
            menuOpenClose.playableAsset = closeTimeline;
            tabElements[GeneralTab].visible = true;     
            menuOpenClose.Play();   
            isOpen = false;
        }         
    }
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
