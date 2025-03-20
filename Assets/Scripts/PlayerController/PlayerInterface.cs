using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Manages the player's interface through a <see cref="UIDocument"/> reference.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class PlayerInterface : NetworkBehaviour
{
    [SerializeField] private float _messageDuration;

    private VisualElement _rootVisualElement;

    /// <summary>
    /// Initializes the root UI element and the visibility of certain UI elements.
    /// </summary>
    public override void OnStartAuthority()
    {
        UIDocument uiDocument = gameObject.GetComponent<UIDocument>();

        if (uiDocument.visualTreeAsset.name != "PlayerUI")
        {
            uiDocument.visualTreeAsset = Resources.Load<VisualTreeAsset>("UI/PlayerUI");
        }

        _rootVisualElement = uiDocument.rootVisualElement;

        switch (SceneManager.GetActiveScene().name)
        {
            case "Gameplay-Preparation":
                if (UnityUtils.ContainsElement(_rootVisualElement, "RoundTheme", out VisualElement themeContainer))
                {
                    themeContainer.visible = true;
                    if (themeContainer.visible == false) return;
                    if (UnityUtils.ContainsElement(_rootVisualElement, "ThemeText", out TextElement themeText))
                    {
                        ThemeName themeName = NetworkManager.FindObjectOfType<ThemeName>();
                        themeText.text = themeName.theme;
                    }
                }
                break;
            case "Gameplay-Dungeon":
                if (UnityUtils.ContainsElement(_rootVisualElement, "Script-Counter", out TextElement scriptCounter))
                {
                    scriptCounter.visible = true;
                    FindObjectOfType<ScriptManagement>().UpdateMessage();
                }
                break;
            default:
                break;
        }
        base.OnStartAuthority();
    }

    /// <summary>
    /// Activates an inventory slot by making it visually appear as active.
    /// </summary>
    /// <param name="slotKey">The key representing the slot to activate.</param>
    public void ActivateSlot(string slotKey)
    {
        if (UnityUtils.ContainsElement(_rootVisualElement, slotKey, out VisualElement targetSlot))
        {
            targetSlot.AddToClassList("active");
        }
    }

    /// <summary>
    /// Deactivates an inventory slot by making it visually appear as inactive.
    /// </summary>
    /// <param name="slotKey">The key representing the slot to activate.</param>
    public void DeactivateSlot(string slotKey)
    {
        if (UnityUtils.ContainsElement(_rootVisualElement, slotKey, out VisualElement targetSlot))
        {
            targetSlot.RemoveFromClassList("active");
        }
    }

    /// <summary>
    /// Displays a message above the player's inventory for a short duration.
    /// </summary>
    /// <param name="message">The message to display in the UI.</param>
    public void DisplayInventoryMessage(string message)
    {
        StopCoroutine("DisplayInventoryMessageCoroutine");
        StartCoroutine(DisplayInventoryMessageCoroutine(message));
    }

    /// <summary>
    /// Coroutine that handles the timing of displaying and hiding the inventory message.
    /// </summary>
    /// <param name="message">The message to display in the UI.</param>
    /// <returns>An <c>IEnumerator</c> for coroutine execution.</returns>
    private IEnumerator DisplayInventoryMessageCoroutine(string message)
    {
        if (UnityUtils.ContainsElement(_rootVisualElement, "Inventory-Message", out Label messageElement))
        {
            messageElement.text = message;
            messageElement.style.opacity = 1;

            yield return new WaitForSeconds(_messageDuration);

            messageElement.style.opacity = 0;
        }
    }

    /// <summary>
    /// Renders the icon of an inventory item in the UI.
    /// </summary>
    /// <param name="slotKey">The key representing the slot where the inventory icon should be rendered.</param>
    /// <param name="itemSprite">The inventory item to display.</param>
    public void RenderInventoryIcon(string slotKey, Sprite itemSprite)
    {
        // Retrieve the VisualElement that renders the item icon.
        // The `GameUI` inventory is structured as #Slot-Key > #Item, where #Item renders the item icon.
        VisualElement itemElement = _rootVisualElement.Query<VisualElement>(slotKey).Children<VisualElement>().ToList()[0];

        if (itemElement != null)
        {
            if (itemSprite != null)
            {
                itemElement.style.backgroundImage = new StyleBackground(itemSprite.texture);
            }
            else
            {
                Sprite defaultSprite = Resources.Load<Sprite>($"Sprites/{slotKey}");
                itemElement.style.backgroundImage = new StyleBackground(defaultSprite.texture);
            }
        }
        else
        {
            UnityUtils.LogError($"Unable to find '#Item' as the first child of '#{slotKey}'. Please ensure that the element exists and the slot key is correct.");
        }
    }

    /// <summary>
    /// Refreshes the attack statistic display in the UI.
    /// </summary>
    /// <param name="totalAttack">The total attack value to display.</param>
    public void RefreshAttack(float totalAttack)
    {
        RefreshStatus("Attack-Value", totalAttack);
    }

    /// <summary>
    /// Refreshes the defense statistic display in the UI.
    /// </summary>
    /// <param name="totalDefense">The total defense value to display.</param>
    public void RefreshDefense(float totalDefense)
    {
        RefreshStatus("Defense-Value", totalDefense);
    }

    /// <summary>
    /// Refreshes the health bar display in the UI.
    /// </summary>
    /// <param name="baseHealth">The base health value.</param>
    /// <param name="currentHealth">The current health value.</param>
    public void RefreshHealth(float baseHealth, float currentHealth)
    {
        RefreshStatusBar("Health-Foreground", baseHealth, currentHealth);
    }

    /// <summary>
    /// Refreshes the stamina bar display in the UI.
    /// </summary>
    /// <param name="baseStamina">The base stamina value.</param>
    /// <param name="currentStamina">The current stamina value.</param>
    public void RefreshStamina(float baseStamina, float currentStamina)
    {
        RefreshStatusBar("Stamina-Foreground", baseStamina, currentStamina);
    }

    /// <summary>
    /// Refreshes the critical strike chance display in the UI.
    /// </summary>
    /// <param name="critChance">The total critical strike chance to display.</param>
    public void RefreshCriticalStrikeChance(float critChance)
    {
        RefreshStatus("CriticalStrike-Value", critChance);
    }

    /// <summary>
    /// Toggle the score board visibility based on the current class list on the visualelement
    /// </summary>
    /// <returns>True = scoreboard enabled / False = scoreboard disabled or ScoreBoard not found</returns>
    public bool ToggleScoreBoardVisibility()
    {
        if (UnityUtils.ContainsElement(_rootVisualElement, "ScoreBoard", out VisualElement board))
        {
            if (board.ClassListContains("hide"))
            {
                board.RemoveFromClassList("hide");
                return true;
            }
            else
            {
                board.AddToClassList("hide");
            }
        }
        return false;
    }

    /// <summary>
    /// Refresh the scoreboard. This will first get all the available slots and save it to a list.
    /// The slot will then be assigned a player name from scoreboard's playerName dictionary.
    /// The unfilled slots will be cleared and put into hidden
    /// </summary>
    public void RefreshScoreBoard()
    {
        if (!isLocalPlayer) return;
        ScoreBoard scoreboard = NetworkManager.FindObjectOfType<ScoreBoard>();
        int Counter = 0;

        // Gets a list of all player visual elements for scoreboard
        List<VisualElement> playerSlots = new List<VisualElement>();
        if (_rootVisualElement == null)
        {
            _rootVisualElement = gameObject.GetComponent<UIDocument>().rootVisualElement;
        }
        if (UnityUtils.ContainsElement(_rootVisualElement, "ScoreBoard", out VisualElement board))
        {
            if (UnityUtils.ContainsElement(board, "Background", out VisualElement background))
            {
                playerSlots = background.Query<VisualElement>(className: "player").ToList();
            }
        }

        foreach (KeyValuePair<uint, string> data in scoreboard.playerName)
        {
            // Handles player name updating
            if (UnityUtils.ContainsElement(playerSlots[Counter], playerSlots[Counter].name + "-Name", out TextElement textElement))
            {
                textElement.text = data.Value;
                playerSlots[Counter].RemoveFromClassList("hide");
            }

            // Handles stats updating
            if (UnityUtils.ContainsElement(playerSlots[Counter], playerSlots[Counter].name + "-Kills", out TextElement killElement))
            {
                killElement.text = scoreboard.PlayerKDA.GetValueOrDefault(data.Key).KillData.ToString();
            }
            if (UnityUtils.ContainsElement(playerSlots[Counter], playerSlots[Counter].name + "-Deaths", out TextElement deathElement))
            {
                deathElement.text = scoreboard.PlayerKDA.GetValueOrDefault(data.Key).DeathData.ToString();
            }
            if (UnityUtils.ContainsElement(playerSlots[Counter], playerSlots[Counter].name + "-Assists", out TextElement assistElement))
            {
                assistElement.text = scoreboard.PlayerKDA.GetValueOrDefault(data.Key).AssistData.ToString();
            }

            Counter++;
        }

        // Hides player visual element if they do not exist in scoreboard.playerName
        for (int i = Counter; i < playerSlots.Count; i++)
        {
            if (UnityUtils.ContainsElement(playerSlots[Counter], playerSlots[Counter].name + "-Name", out TextElement textEle))
            {
                textEle.text = null;
                playerSlots[Counter].AddToClassList("hide");
            }
            Counter++;
        }
    }


    /// <summary>
    /// Updates the UI displaying the script collection task
    /// </summary>
    /// <param name="info"></param>
    [TargetRpc]
    public void RpcRefreshScriptCount(string info)
    {
        if (UnityUtils.ContainsElement(_rootVisualElement, "Script-Counter", out TextElement scriptCounter))
        {
            scriptCounter.text = info;
        }
    }

    /// <summary>
    /// Refreshes the display of a numeric statistic.
    /// </summary>
    /// <param name="elementName">The name of the UI element to update.</param>
    /// <param name="targetValue">The value to display on the element.</param>
    private void RefreshStatus(string elementName, float targetValue)
    {
        if (UnityUtils.ContainsElement(_rootVisualElement, elementName, out Label statusLabel))
        {
            statusLabel.text = targetValue.ToString();
        }
    }

    /// <summary>
    /// Refreshes the display of a status bar.
    /// </summary>
    /// <param name="elementName">The name of the UI element to update.</param>
    /// <param name="baseValue">The base value of the status bar.</param>
    /// <param name="currentValue">The current value of the status bar.</param>
    private void RefreshStatusBar(string elementName, float baseValue, float currentValue)
    {
        if (UnityUtils.ContainsElement(_rootVisualElement, elementName, out VisualElement statusBar))
        {
            float staminaPercentage = Mathf.Clamp01(currentValue / baseValue);

            statusBar.style.width = new Length(staminaPercentage * 100, LengthUnit.Percent);
        }
    }
}
