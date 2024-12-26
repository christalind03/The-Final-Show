using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

// TODO: Document
[RequireComponent(typeof(UIDocument))]
public class PlayerInterface : NetworkBehaviour
{
    private VisualElement _rootVisualElement;

    // TODO: Document
    public override void OnStartAuthority()
    {
        _rootVisualElement = gameObject.GetComponent<UIDocument>().rootVisualElement;

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
            UnityExtensions.LogError($"Unable to locate VisualElement titled '{slotKey}'");
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
            UnityExtensions.LogError($"Unable to locate VisualElement titled '{slotKey}'");
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
}
