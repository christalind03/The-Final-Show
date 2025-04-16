using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class SettingSaveLoad : NetworkBehaviour
{
    public InputActionAsset actions;
    private SettingsMenu settingsMenu;

    /// <summary>
    /// Loads the saved setting once player joins the game
    /// </summary>
    public override void OnStartClient()
    {
        if (!isLocalPlayer) return;
        base.OnStartAuthority();
        settingsMenu = GetComponent<SettingsMenu>();
        PlayerController controller = gameObject.transform.parent.GetComponent<PlayerController>();
        LoadRebind(controller);
        settingsMenu.dropdownElements["ScreenSetting"].index = LoadSetting("Screen Setting", 0);
        settingsMenu.sliderElements["CameraSens"].value = LoadSetting("Camera Sensitivity", 3.5f);
        settingsMenu.sliderElements["MusicSlider"].value = LoadSetting("Music Volume", 1.0f);
    }

    /// <summary>
    /// Save setting when game is stopped 
    /// </summary>
    private void OnApplicationQuit()
    {
        if (!isLocalPlayer) return;
        SaveSetting("Screen Setting", settingsMenu.dropdownElements["ScreenSetting"].index);
        SaveSetting("Camera Sensitivity", settingsMenu.sliderElements["CameraSens"].value);
        SaveSetting("Music Volume", settingsMenu.sliderElements["MusicSlider"].value);
        SaveRebind();
    }

    /// <summary>
    /// Saves the player setting once player switch scene the game 
    /// </summary>
    private void OnDisable()
    {
        if (!isLocalPlayer) return;
        SaveSetting("Screen Setting", settingsMenu.dropdownElements["ScreenSetting"].index);
        SaveSetting("Camera Sensitivity", settingsMenu.sliderElements["CameraSens"].value);
        SaveSetting("Music Volume", settingsMenu.sliderElements["MusicSlider"].value);
        SaveRebind();
    }

    /// <summary>
    /// Load key binds function 
    /// </summary>
    /// <param name="controller"></param>
    private void LoadRebind(PlayerController controller)
    {
        actions = controller.playerInput.actions;
        InputActionAsset inputActions = controller.playerInput.actions;
        var rebinds = PlayerPrefs.GetString("Rebinds");
        if (!string.IsNullOrEmpty(rebinds))
        {
            actions.LoadBindingOverridesFromJson(rebinds);
        }

        // Overwrite UI for rebind buttons from playerprefs
        foreach (InputAction action in inputActions.actionMaps[0].actions)
        {
            string displayString = string.Empty;
            if (action.bindings[0].isComposite)
            {
                List<string> compositeParts = new List<string>();

                for (int i = 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; i++)
                {
                    compositeParts.Add(InputControlPath.ToHumanReadableString(action.bindings[i].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice));
                }

                displayString = string.Join(" / ", compositeParts);
            }
            else
            {
                displayString = InputControlPath.ToHumanReadableString(action.bindings[0].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
            }

            if (settingsMenu.controlButtonMap.ContainsKey(action.name))
            {
                settingsMenu.controlButtonMap[action.name].text = displayString;
            }
        }
    }

    /// <summary>
    /// Save key binds function 
    /// </summary>
    private void SaveRebind()
    {
        var rebinds = actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("Rebinds", rebinds);
    }

    /// <summary>
    /// Universal save setting 
    /// </summary>
    /// <typeparam name="T">the type of value being saved</typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    private void SaveSetting<T>(string key, T value)
    {
        if (typeof(T) == typeof(float))
        {
            PlayerPrefs.SetFloat(key, Convert.ToSingle(value));
        }
        else if (typeof(T) == typeof(int))
        {
            PlayerPrefs.SetInt(key, Convert.ToInt32(value));
        }
        else if (typeof(T) == typeof(string))
        {
            PlayerPrefs.SetString(key, value.ToString());
        }
    }

    /// <summary>
    /// Universal load setting 
    /// </summary>
    /// <typeparam name="T">the type of value being loaded</typeparam>
    /// <param name="key"></param>
    /// <param name="defaultValue">default value if key is not found</param>
    /// <returns>the saved value in player prefs</returns>
    private T LoadSetting<T>(string key, T defaultValue)
    {
        if (typeof(T) == typeof(float))
        {
            return (T)(object)PlayerPrefs.GetFloat(key, Convert.ToSingle(defaultValue));
        }
        else if (typeof(T) == typeof(int))
        {
            return (T)(object)PlayerPrefs.GetInt(key, Convert.ToInt32(defaultValue));
        }
        else if (typeof(T) == typeof(string))
        {
            return (T)(object)PlayerPrefs.GetString(key, defaultValue.ToString());
        }
        return defaultValue;
    }
}
