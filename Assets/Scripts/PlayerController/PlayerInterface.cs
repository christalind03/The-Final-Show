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
    [SerializeField] private float _messageDuration;

    private VisualElement _rootVisualElement;

    /// <summary>
    /// Initializes the root UI element and the visibility of certain UI elements.
    /// </summary>
    public override void OnStartAuthority()
    {
        _rootVisualElement = gameObject.GetComponent<UIDocument>().rootVisualElement;
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
        StopCoroutine("DisplayInvnetoryMessageCoroutine");
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
        if (UnityUtils.ContainsElement(_rootVisualElement, "Ammo-Count", out Label ammoCount))
        {
            if (UnityUtils.ContainsElement(_rootVisualElement, "Ammo-Remaining", out Label ammoRemaining))
            {
                ammoCount.text = clipAmount.ToString();
                ammoRemaining.text = remainingAmount.ToString();
            }
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
        if (UnityUtils.ContainsElement(_rootVisualElement, "Ammo", out VisualElement ammoElement))
        {
            ammoElement.style.opacity = displayAmmo ? 1 : 0;
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
