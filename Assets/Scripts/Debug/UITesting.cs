using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;

public class UITesting : MonoBehaviour
{
    [SerializeField] PlayableDirector openMenu;
    [SerializeField] PlayableDirector switchSettingTabs;
    [SerializeField] PlayableDirector showAudioTab;
    [SerializeField] UIDocument uIDocument;
    private VisualElement _rootVisualElement;
    private VisualElement _navContainer;
    private VisualElement _generalContainer;
    private VisualElement _audioContainer;
    private VisualElement _controlsContainer;
    private string currentTab;

    void Start(){
        _rootVisualElement = uIDocument.rootVisualElement;
        currentTab = "Setting-General-Container";
        if(UnityUtils.ContainsElement(_rootVisualElement, "Settings-Container", out VisualElement settings)){
            if(UnityUtils.ContainsElement(settings, "Setting-Nav-Container", out VisualElement nav)){
                _navContainer = nav;
            }
            if(UnityUtils.ContainsElement(settings, "Setting-General-Container", out VisualElement gen)){
                _generalContainer = gen;
            }
            if(UnityUtils.ContainsElement(settings, "Setting-Audio-Container", out VisualElement aud)){
                _audioContainer = aud;
            }
            if(UnityUtils.ContainsElement(settings, "Setting-Controls-Container", out VisualElement con)){
                _controlsContainer = con;
            }
        }

        if(UnityUtils.ContainsElement(_navContainer, "Setting-GeneralBtn", out Button genBtn)){
            genBtn.clicked += SwitchToGeneral;           
        }
        if(UnityUtils.ContainsElement(_navContainer, "Setting-AudioBtn", out Button audioBtn)){
            audioBtn.clicked += SwitchToAudio;           
        }
        if(UnityUtils.ContainsElement(_navContainer, "Setting-ControlsBtn", out Button conBtn)){
            conBtn.clicked += SwitchToControl;           
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("escape")){
            OpenMenu();
        }
    }

    private void OpenMenu(){
        _generalContainer.visible = true;
        _audioContainer.visible = false;
        _controlsContainer.visible = false;
        openMenu.Play();
    }
    private void SwitchToGeneral(){
        currentTab = "Setting-Audio-Container";
        _audioContainer.visible = true;
        switchSettingTabs.Play();
        showAudioTab.Play();
    }
    private void SwitchToAudio(){
        currentTab = "Setting-Audio-Container";
        _audioContainer.visible = true;
        switchSettingTabs.Play();
        showAudioTab.Play();
    }
    private void SwitchToControl(){
        currentTab = "Setting-Audio-Container";
        _audioContainer.visible = true;
        switchSettingTabs.Play();
        showAudioTab.Play();
    }
}
