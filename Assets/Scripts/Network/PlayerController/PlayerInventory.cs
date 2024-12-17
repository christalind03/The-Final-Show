using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// Represent's a player's inventory with  multiple slots to storage and manage items.
/// </summary>
public class PlayerInventory
{
    private struct PlayerInventoryRestriction
    {
        public Enum ItemCategory {  get; private set; }
        public Type ItemType {  get; private set; }

        public PlayerInventoryRestriction(Enum itemCategory, Type itemType)
        {
            ItemCategory = itemCategory;
            ItemType = itemType;
        }
    }

    private string _currentSlot;
    private Dictionary<string, InventoryItem> _inventorySlots;
    private Dictionary<string, PlayerInventoryRestriction> _inventoryRestrictions;
    private VisualElement _uiDocument;

    /// <summary>
    /// Sets up the default slot and creates an empty inventory.
    /// </summary>
    public PlayerInventory(VisualElement uiDocument)
    {
        // Default to Slot 1
        _currentSlot = "Slot-1";

        // Define inventory slots and their respective restrictions
        _inventorySlots = new Dictionary<string, InventoryItem>
        {
            { "Slot-1", null }, // Melee
            { "Slot-2", null }, // Ranged
            { "Slot-3", null }, // Armor (Head)
            { "Slot-4", null }, // Armor (Chest)
            { "Slot-5", null }, // Armor (Legs)
            { "Slot-6", null }, // Armor (Feet)
            { "Slot-7", null }, // Undefined (Reserved for non-armor/weapon types)
            { "Slot-8", null }, // Undefined (Reserved for non-armor/weapon types)
            { "Slot-9", null }, // Undefined (Reserved for non-armor/weapon types)
        };

        _inventoryRestrictions = new Dictionary<string, PlayerInventoryRestriction>
        {
            { "Slot-3", new PlayerInventoryRestriction(Armor.ArmorCategory.Head, typeof(Armor)) },
        };

        _uiDocument = uiDocument;
        _uiDocument.Q<VisualElement>(_currentSlot).AddToClassList("active");
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
            _uiDocument.Q<VisualElement>(_currentSlot).AddToClassList("active");
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
            _uiDocument.Q<VisualElement>(actionName).RemoveFromClassList("active");
        }
    }

    // TODO: Update documentation?
    /// <summary>
    /// Add an item to the currently selected inventory slot.
    /// </summary>
    /// <param name="inventoryItem">The item to add to the inventory slot.</param>
    public bool AddItem(InventoryItem inventoryItem)
    {
        string availableSlot = FindAvailableSlot(inventoryItem);

        if (availableSlot != null)
        {
            _inventorySlots[availableSlot] = inventoryItem;
            _uiDocument.Q<VisualElement>(availableSlot).AddToClassList("containsItem");
            return true;
        }

        // TODO: Display some sort of error to the user through the Game UI
        Debug.Log("All inventory slots are full!");
        return false;
    }

    /// <summary>
    /// Removes the item from the currently selected inventory slot.
    /// </summary>
    /// <returns>The gameObject that was removed from the inventory slot if it exists.</returns>
    public InventoryItem RemoveItem()
    {
        InventoryItem removedItem = _inventorySlots[_currentSlot];

        _inventorySlots[_currentSlot] = null;
        _uiDocument.Q<VisualElement>(_currentSlot).RemoveFromClassList("containsItem");

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

    // TODO: Documentation
    private string FindAvailableSlot(InventoryItem inventoryItem)
    {
        // Check to see if there is a specialized slot this item can go into.
        foreach (var inventoryRestriction in _inventoryRestrictions)
        {
            string restrictionCategory = inventoryRestriction.Value.ItemCategory.ToString();
            Type restrictionType = inventoryRestriction.Value.ItemType;

            if (inventoryItem.GetType() == restrictionType)
            {
                // Ensure that the item category is present within the item.
                // This is designed to be highly generic so that it can work with different armor and weapon categories without the need to hardcode it.
                var itemFields = restrictionType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                for (int i = 0; i < itemFields.Length; i++)
                {
                    // If the item category matches with the restricted category, then we can attempt to assign this item to the specified slot.
                    if (restrictionCategory == itemFields[i].GetValue(inventoryItem).ToString())
                    {
                        // If the target slot is empty, then return the slot key. Otherwise, return null.
                        return _inventorySlots[inventoryRestriction.Key] == null ? inventoryRestriction.Key : null;
                    }
                }
            }
        }

        // If the item cannot go into a specialized slot, then find the closest available slot.
        foreach (var inventorySlot in _inventorySlots)
        {
            if (inventorySlot.Value == null)
            {
                return inventorySlot.Key;
            }
        }

        // There are no slots available.
        return null;
    }
}