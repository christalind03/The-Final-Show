using Mirror;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

// TODO: Document
[RequireComponent(typeof(UIDocument))]
public class PlayerInterface : NetworkBehaviour
{
    [SerializeField] private float _inventoryMessageDuration;

    private VisualElement _rootVisualElement;

    // TODO: Document
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

    // TODO: Document
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

    // TODO: Document
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

    // TODO: Document
    public void DisplayInventoryMessage(string inventoryMessage)
    {
        StartCoroutine(DisplayInventoryMessageCoroutine(inventoryMessage));
    }

    // TODO: Document
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

    // TODO: Document
    public void RenderInventoryIcon(string slotKey, InventoryItem inventoryItem)
    {
        // Retrieve the VisualElement that renders the item icon.
        // The `GameUI` inventory is structured as #Slot-Key > #Item, where #Item renders the item icon.
        VisualElement inventoryIcon = _rootVisualElement.Query<VisualElement>(slotKey).Children<VisualElement>().ToList()[0];

        if (inventoryIcon != null )
        {
            if (inventoryItem && inventoryItem.ObjectSprite != null)
            {
                inventoryIcon.style.backgroundImage = new StyleBackground(inventoryItem.ObjectSprite.texture);
            }
            else
            {
                Sprite defaultSprite = Resources.Load<Sprite>($"Sprites/{slotKey}");
                inventoryIcon.style.backgroundImage = new StyleBackground(defaultSprite.texture);
            }
        }
        else
        {
            UnityUtils.LogError($"Unable to find '#Item' as the first child of '#{slotKey}'. Please ensure that the element exists and the slot key is correct.");
        }
    }

    // TODO: Document
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

    // TODO: Document
    public void RefreshAttack(float totalAttack)
    {
        RefreshStatus("Attack-Value", totalAttack);
    }
    
    // TODO: Document
    public void RefreshDefense(float totalDefense)
    {
        RefreshStatus("Defense-Value", totalDefense);
    }

    // TODO: Document
    public void RefreshHealth(float baseHealth, float currentHealth)
    {
        RefreshStatusBar("Health-Foreground", baseHealth, currentHealth);
    }

    // TODO: Document
    public void RefreshStamina(float baseStamina, float currentStamina)
    {
        RefreshStatusBar("Stamina-Foreground", baseStamina, currentStamina);
    }
    
    // TODO: Document
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

    // TODO: Document
    private void MissingElementError(string elementType, string elementName)
    {
        UnityUtils.LogError($"Unable to locate {elementType} titled '{elementName}'");
    }

    // TODO: Document
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

    // TODO: Document
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
}
