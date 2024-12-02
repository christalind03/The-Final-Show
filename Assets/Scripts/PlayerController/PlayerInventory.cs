using System;
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
    private string _elementName;
    ///***private Dictionary<string, GameObject> _inventorySlots;
    ///***now handles PlayerInventoryItem instead of GameObject
    private Dictionary<string, PlayerInventoryItem> _inventorySlots = new Dictionary<string, PlayerInventoryItem>();
    private VisualElement _uiDocument;

    /// <summary>
    /// Sets up the default slot and creates an empty inventory.
    /// </summary>
    public PlayerInventory(VisualElement uiDocument)
    {
        _currentSlot = "Slot 1"; // Default to Slot 1
        ///***
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
        _elementName = _currentSlot.Replace(" ", "-");
        _uiDocument.Q<VisualElement>(_elementName).AddToClassList("active");
    }

    /// <summary>
    /// Selects an inventory slot based on the provided input action context.
    /// </summary>
    /// <param name="context">The input callback context to subscribe/unsubscribe to using the Input System.</param>
    public void SelectSlot(InputAction.CallbackContext context)
    {
        // Handle hotkey input for inventory slots.
        // NOTE: The actionName includes whitespace
        string actionName = context.action.name;

        if (_inventorySlots.ContainsKey(actionName))
        {
            ResetSlots();
            _currentSlot = actionName;
            _elementName = actionName.Replace(" ", "-");
            _uiDocument.Q<VisualElement>(_elementName).AddToClassList("active");
        }

        // Handle input from mouse scroll wheel.
        if (context.action.type == InputActionType.Value && context.action.expectedControlType == "Vector2")
        {
            // TODO: Implement the scroll logic after we have items to reference.
            // This involves possibly converting the _inventorySlots type from a Dictionary to a List in order to utilize indices for cycling.
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
        ///*** get PlayerInventoryItem component from GameObject
        var inventoryItem = equippableItem.GetComponent<PlayerInventoryItem>();
        if (inventoryItem == null)
        {
            Debug.LogError($"The GameObject {equippableItem.name} does not have a PlayerInventoryItem component!");
            return;
        }
        ///***

        _inventorySlots[_currentSlot] = equippableItem;
        _uiDocument.Q<VisualElement>(_elementName).AddToClassList("containsItem");
    }

    /// <summary>
    /// Removes the item from the currently selected inventory slot. ***edited for debug/check for PlayerInventoryItem
    /// </summary>
    /// <returns>The gameObject that was removed from the inventory slot if it exists.</returns>
    public GameObject RemoveItem()
    {
  
        GameObject removedItem = _inventorySlots[_currentSlot];

        if (removedItem != null)
        {
            ///***check for PlayerInventoryItem
            if (removedItem.TryGetComponent<PlayerInventoryItem>(out var inventoryItem))
            {

                inventoryItem.OnRemovedFromInventory();
            }

            _inventorySlots[_currentSlot] = null;
            _uiDocument.Q<VisualElement>(_elementName).RemoveFromClassList("containsItem");

            Debug.Log($"{removedItem.name} removed from slot {_currentSlot}.");
        }
        else
        {
            Debug.LogWarning($"No item to remove from slot {_currentSlot}.");
        }

        return removedItem; //***return the GameObject regardless of whether it has PlayerInventoryItem or not
    }

    /// <summary>
    /// Checks to see if the currently selected inventory slot contains an item.
    /// </summary>
    /// <returns>True if the current slot contains an item; otherwise, false.</returns>
    public bool HasItem()
    {
        return _inventorySlots[_currentSlot] != null;
    }

    /// <summary>
    ///*** Added item logic
    /// </summary>
    public void EquipCurrentItem()
    {
        if (_inventorySlots[_currentSlot] != null)
        {
            _inventorySlots[_currentSlot].Equip();
        }
    }

    public void UnequipCurrentItem()
    {
        if (_inventorySlots[_currentSlot] != null)
        {
            _inventorySlots[_currentSlot].Unequip();
        }
    }

    public void AttackWithCurrentItem()
    {
        if (_inventorySlots[_currentSlot] != null)
        {
            _inventorySlots[_currentSlot].Attack();
        }
    }

    public void AlternateAttackWithCurrentItem()
    {
        if (_inventorySlots[_currentSlot] != null)
        {
            _inventorySlots[_currentSlot].AlternateAttack();
        }
    }

    public void AddItemToSlot(string slot, PlayerInventoryItem item)
    {
        if (_inventorySlots.ContainsKey(slot))
        {
            _inventorySlots[slot] = item;
        }
    }
    ///***End item logic
}
