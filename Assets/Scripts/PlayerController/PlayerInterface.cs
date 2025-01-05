using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Manages the player's interface through a <see cref="UIDocument"/> reference.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class PlayerInterface : NetworkBehaviour
{
    [SerializeField] private float _inventoryMessageDuration;

    private VisualElement _rootVisualElement;

    /// <summary>
    /// Initializes the root UI element and the visibility of certain UI elements.
    /// </summary>
    public override void OnStartAuthority()
    {
        _rootVisualElement = gameObject.GetComponent<UIDocument>().rootVisualElement;

        // Ensure specific elements remain hidden until necessary
        Label inventoryMessageElement = _rootVisualElement.Query<Label>("Message");
        inventoryMessageElement.style.opacity = 0;

        VisualElement ammunitionElement = _rootVisualElement.Query<VisualElement>("Ammo");
        ammunitionElement.style.opacity = 0;

        base.OnStartAuthority();
    }

    /// <summary>
    /// Activates an inventory slot by making it visually appear as active.
    /// </summary>
    /// <param name="slotKey">The key representing the slot to activate.</param>
    public void ActivateSlot(string slotKey)
    {
        VisualElement targetSlot = _rootVisualElement.Query<VisualElement>(slotKey);

        if (targetSlot != null)
        {
            targetSlot.AddToClassList("active");
        }
        else
        {
            MissingElementError("VisualElement", slotKey);
        }
    }

    /// <summary>
    /// Deactivates an inventory slot by making it visually appear as inactive.
    /// </summary>
    /// <param name="slotKey">The key representing the slot to activate.</param>
    public void DeactivateSlot(string slotKey)
    {
        VisualElement targetSlot = _rootVisualElement.Query<VisualElement>(slotKey);

        if (targetSlot != null)
        {
            targetSlot.RemoveFromClassList("active");
        }
        else
        {
            MissingElementError("VisualElement", slotKey);
        }
    }

    /// <summary>
    /// Displays a message above the player's inventory for a short duration.
    /// </summary>
    /// <param name="inventoryMessage">The message to display in the UI.</param>
    public void DisplayInventoryMessage(string inventoryMessage)
    {
        StartCoroutine(DisplayInventoryMessageCoroutine(inventoryMessage));
    }

    /// <summary>
    /// Coroutine that handles the timing of displaying and hiding the inventory message.
    /// </summary>
    /// <param name="inventoryMessage">The message to display in the UI.</param>
    /// <returns>An <c>IEnumerator</c> for coroutine execution.</returns>
    private IEnumerator DisplayInventoryMessageCoroutine(string inventoryMessage)
    {
        string elementName = "Message";
        Label inventoryMessageElement = _rootVisualElement.Query<Label>(elementName);

        if (inventoryMessageElement != null)
        {
            inventoryMessageElement.text = inventoryMessage;
            inventoryMessageElement.style.opacity = 1;

            yield return new WaitForSeconds(_inventoryMessageDuration);

            inventoryMessageElement.style.opacity = 0;
        }
        else
        {
            MissingElementError("Label", elementName);
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

        if (itemElement != null )
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
    /// Refreshes the ammunition UI with the current clip and remaining ammo amounts.
    /// </summary>
    /// <param name="clipAmount">The current clip count.</param>
    /// <param name="remainingAmount">The remaining amount of ammo.</param>
    public void RefreshAmmo(int clipAmount, int remainingAmount)
    {
        Label ammoCount = _rootVisualElement.Query<Label>("Ammo-Count"); 
        Label ammoRemaining = _rootVisualElement.Query<Label>("Ammo-Remaining"); 

        if (ammoCount != null && ammoRemaining != null)
        {
            ammoCount.text = clipAmount.ToString();
            ammoRemaining.text = remainingAmount.ToString();
        }
        else
        {
            UnityUtils.LogError("Unable to locate Label titled 'Ammo-Count' and/or 'Ammo-Remaining'");
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
    /// Toggles the visibility of the ammunition UI.
    /// </summary>
    /// <param name="displayAmmo">If true, the ammunition UI will be displayed; otherwise, hides it.</param>
    public void ToggleAmmoVisibility(bool displayAmmo)
    {
        string elementName = "Ammo";
        VisualElement ammoElement = _rootVisualElement.Query<VisualElement>(elementName);

        if (ammoElement != null)
        {
            ammoElement.style.opacity = displayAmmo ? 1 : 0;
        }
        else
        {
            MissingElementError("VisualElement", elementName);
        }
    }

    /// <summary>
    /// Refreshes the display of a numeric statistic.
    /// </summary>
    /// <param name="elementName">The name of the UI element to update.</param>
    /// <param name="targetValue">The value to display on the element.</param>
    private void RefreshStatus(string elementName, float targetValue)
    {
        Label statusLabel = _rootVisualElement.Query<Label>(elementName);

        if (statusLabel != null)
        {
            statusLabel.text = targetValue.ToString();
        }
        else
        {
            MissingElementError("Label", elementName);
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
        VisualElement statusBar = _rootVisualElement.Query<VisualElement>(elementName);

        if (statusBar != null)
        {
            float staminaPercentage = Mathf.Clamp01(currentValue / baseValue);

            statusBar.style.width = new Length(staminaPercentage * 100, LengthUnit.Percent);
        }
        else
        {
            MissingElementError("VisualElement", elementName);
        }
    }

    /// <summary>
    /// Logs an error if the specified UI element is missing.
    /// </summary>
    /// <remarks>
    /// This function is important to have as querying elements by name will often not raise an error, ultimately causing problems.
    /// </remarks>
    /// <param name="elementType">The type of the element that should be displayed.</param>
    /// <param name="elementName">The name of the element that should be displayed.</param>
    private void MissingElementError(string elementType, string elementName)
    {
        UnityUtils.LogError($"Unable to locate {elementType} titled '{elementName}'");
    }
}
