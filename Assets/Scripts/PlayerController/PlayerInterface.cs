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
            MissingElementError(slotKey);
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
            MissingElementError(slotKey);
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
            MissingElementError(elementName);
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
            UnityExtensions.LogError($"Unable to find '#Item' as the first child of '#{slotKey}'. Please ensure that the element exists and the slot key is correct.");
        }
    }

    // TODO: Document
    public void RefreshAttack(float totalAttack)
    {
        string elementName = "Attack-Value";
        Label attackLabel = _rootVisualElement.Query<Label>(elementName);

        if (attackLabel != null)
        {
            attackLabel.text = totalAttack.ToString();
        }
        else
        {
            MissingElementError(elementName);
        }
    }
    
    // TODO: Document
    public void RefreshDefense(float totalDefense)
    {
        string elementName = "Defense-Value";
        Label defenseLabel = _rootVisualElement.Query<Label>(elementName);

        if (defenseLabel != null)
        {
            defenseLabel.text = totalDefense.ToString();
        }
        else
        {
            MissingElementError(elementName);
        }
    }

    // TODO: Document
    public void RefreshHealth(float currentHealth, float baseHealth)
    {
        string elementName = "Health-Foreground";
        VisualElement healthInterface = _rootVisualElement.Query<VisualElement>(elementName);

        if (healthInterface != null)
        {
            float healthPercentage = Mathf.Clamp01(currentHealth / baseHealth);

            healthInterface.style.width = new Length(healthPercentage * 100, LengthUnit.Percent);
        }
        else
        {
            MissingElementError(elementName);
        }
    }

    // TODO: Document
    public void RefreshStamina(float currentStamina, float baseStamina)
    {
        string elementName = "Stamina-Foreground";
        VisualElement staminaInterface = _rootVisualElement.Query<VisualElement>(elementName);

        if ( staminaInterface != null)
        {
            float staminaPercentage = Mathf.Clamp01(currentStamina / baseStamina);

            staminaInterface.style.width = new Length(staminaPercentage * 100, LengthUnit.Percent);
        }
        else
        {
            MissingElementError(elementName);
        }
    }

    // TODO: Document
    private void MissingElementError(string elementName)
    {
        UnityExtensions.LogError($"Unable to locate VisualElement titled '{elementName}'");
    }
}
