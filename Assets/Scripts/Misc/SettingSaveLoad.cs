using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class SettingSaveLoad : NetworkBehaviour
{
    public InputActionAsset actions;
    private SettingsMenu settingsMenu;

    public override void OnStartAuthority() {
        base.OnStartAuthority();
        settingsMenu = GetComponent<SettingsMenu>();
        PlayerController controller = gameObject.transform.parent.GetComponent<PlayerController>();
        LoadRebind(controller);
        LoadScreenSetting();
        LoadCameraSens();
    }

    public override void OnStopAuthority() {
        SaveScreenSetting();
        SaveCameraSens();
        SaveRebind();
        base.OnStopAuthority();
    }

    private void LoadRebind(PlayerController controller) {
        actions = controller.playerInput.actions;
        InputActionAsset inputActions = controller.playerInput.actions;
        var rebinds = PlayerPrefs.GetString("Rebinds");
        if (!string.IsNullOrEmpty(rebinds)){
            actions.LoadBindingOverridesFromJson(rebinds);        
        }
        
        // Overwrite UI for rebind buttons from playerprefs
        foreach(InputAction action in inputActions.actionMaps[0].actions){
            string displayString = string.Empty;
            if(action.bindings[0].isComposite){
                List<string> compositeParts = new List<string>();

                for (int i = 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; i++) {
                    compositeParts.Add(InputControlPath.ToHumanReadableString(action.bindings[i].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice));
                }

                displayString = string.Join(" / ", compositeParts);
            }
            else{
                displayString = InputControlPath.ToHumanReadableString(action.bindings[0].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
            }

            if (settingsMenu.controlButtonMap.ContainsKey(action.name)){
                settingsMenu.controlButtonMap[action.name].text = displayString;
            }
        }
    }
    private void SaveRebind() {
        var rebinds = actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("Rebinds", rebinds);
    }

    private void LoadCameraSens() {
        float value = PlayerPrefs.GetFloat("Camera Sensitivity");
        settingsMenu.sliderElements["CameraSens"].value = value;
    }

    private void SaveCameraSens() {
        float value = settingsMenu.sliderElements["CameraSens"].value;
        PlayerPrefs.SetFloat("Camera Sensitivity", value);
    }

    private void LoadScreenSetting() {
        int value = PlayerPrefs.GetInt("Screen Setting");
        settingsMenu.dropdownElements["ScreenSetting"].index = value;
    }

    private void SaveScreenSetting() {
        int value = settingsMenu.dropdownElements["ScreenSetting"].index;
        PlayerPrefs.SetInt("Screen Setting", value);
    }
}
