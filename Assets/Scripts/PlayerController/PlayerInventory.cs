using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// Represent's a player's inventory with  multiple slots to storage and manage items.
/// </summary>
public class PlayerInventory
{
    private string _currentSlot;
    private Dictionary<string, GameObject> _inventorySlots;
    private VisualElement _uiDocument;

    /// <summary>
    /// Sets up the default slot and creates an empty inventory.
    /// </summary>
    public PlayerInventory(VisualElement uiDocument)
    {
        _currentSlot = "Slot 1"; // Default to Slot 1
        _inventorySlots = new Dictionary<string, GameObject>
        {
            { "Slot 1", null },
            { "Slot 2", null },
            { "Slot 3", null },
            { "Slot 4", null },
            { "Slot 5", null },
            { "Slot 6", null },
            { "Slot 7", null },
            { "Slot 8", null },
            { "Slot 9", null },
        };

        _uiDocument = uiDocument;
    }

    /// <summary>
    /// Selects an inventory slot based on the provided input action context.
    /// </summary>
    /// <param name="context">The input callback context to subscribe/unsubscribe to using the Input System.</param>
    public void SelectSlot(InputAction.CallbackContext context)
    {
        // NOTE: The actionName includes whitespace
        string actionName = context.action.name;
        string elementName = actionName.Replace(" ", "-");

        if (_inventorySlots.ContainsKey(actionName))
        {
            ResetSlots();
            _currentSlot = actionName;
            _uiDocument.Q<VisualElement>(actionName.Replace(" ", "-")).AddToClassList("active");

        }
    }
    
    /// <summary>
    /// Resets the styles of all slots to the default style.
    /// </summary>
    public void ResetSlots()
    {
        foreach (string actionName in _inventorySlots.Keys)
        {
            string elementName = actionName.Replace(" ", "-");
            _uiDocument.Q<VisualElement>(elementName).RemoveFromClassList("active");
        }
    }

    /// <summary>
    /// Add an item to the currently selected inventory slot.
    /// </summary>
    /// <param name="equippableItem">The item to add to the inventory slot.</param>
    public void AddItem(GameObject equippableItem)
    {
        _inventorySlots[_currentSlot] = equippableItem;
    }

    /// <summary>
    /// Removes the item from the currently selected inventory slot.
    /// </summary>
    /// <returns>The gameObject that was removed from the inventory slot if it exists.</returns>
    public GameObject RemoveItem()
    {
        GameObject removedItem = _inventorySlots[_currentSlot];
        _inventorySlots[_currentSlot] = null;
     
        return removedItem;
    }

    /// <summary>
    /// Checks to see if the currently selected inventory slot contains an item.
    /// </summary>
    /// <returns>True if the current slot contains an item; otherwise, false.</returns>
    public bool HasItem()
    {
        return _inventorySlots[_currentSlot] != null;
    }
}
