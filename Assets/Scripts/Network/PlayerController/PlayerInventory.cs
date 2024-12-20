using Mirror;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

// TODO: Update class documentation
/// <summary>
/// Represent's a player's inventory with  multiple slots to storage and manage items.
/// </summary>
public class PlayerInventory : NetworkBehaviour
{
    private struct PlayerInventoryRestriction
    {
        public Enum ItemCategory { get; private set; }
        public Type ItemType { get; private set; }

        public PlayerInventoryRestriction(Enum itemCategory, Type itemType)
        {
            ItemCategory = itemCategory;
            ItemType = itemType;
        }
    }

    [Header("Equippable References")]
    [SerializeField] private GameObject _handReference;
    [SerializeField] private GameObject _headReference;

    private string _currentSlot;
    private Dictionary<string, InventoryItem> _inventorySlots;
    private Dictionary<string, PlayerInventoryRestriction> _inventoryRestrictions;
    private VisualElement _inventoryHotbar;

    /// <summary>
    /// Sets up the default slot and creates an empty inventory.
    /// </summary>
    public override void OnStartAuthority()
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
            { "Slot-1", new PlayerInventoryRestriction(EquippableItem.EquippableCategory.Hand, typeof(MeleeWeapon)) },
            { "Slot-2", new PlayerInventoryRestriction(EquippableItem.EquippableCategory.Hand, typeof(RangedWeapon)) },
            { "Slot-3", new PlayerInventoryRestriction(EquippableItem.EquippableCategory.Head, typeof(Armor)) },
        };

        _inventoryHotbar = gameObject.GetComponent<UIDocument>().rootVisualElement;
        _inventoryHotbar.Q<VisualElement>(_currentSlot).AddToClassList("active");

        base.OnStartAuthority();
    }

    /// <summary>
    /// Selects an inventory slot based on the provided input action context.
    /// </summary>
    /// <param name="context">The input callback context to subscribe/unsubscribe to using the Input System.</param>
    public void SelectSlot(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) { return; }

        // Handle hotkey input for inventory slots.
        string actionName = context.action.name;

        if (_inventorySlots.ContainsKey(actionName))
        {
            string previousSlot = _currentSlot;

            ResetSlots();
            _currentSlot = actionName;
            _inventoryHotbar.Q<VisualElement>(_currentSlot).AddToClassList("active");

            UpdateSelectedItem(previousSlot);
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
            _inventoryHotbar.Q<VisualElement>(actionName).RemoveFromClassList("active");
        }
    }

    // TODO: Update documentation?
    /// <summary>
    /// Add an item to the currently selected inventory slot.
    /// </summary>
    /// <param name="inventoryItem">The item to add to the inventory slot.</param>
    public bool AddItem(InventoryItem inventoryItem)
    {
        if (!isLocalPlayer) { return false; }

        string availableSlot = FindAvailableSlot(inventoryItem);

        if (availableSlot != null)
        {
            if (inventoryItem is EquippableItem equippableItem)
            {
                if (ShouldEquip(equippableItem))
                {
                    CmdEquip(equippableItem);
                }
            }

            _inventorySlots[availableSlot] = inventoryItem;
            _inventoryHotbar.Q<VisualElement>(availableSlot).AddToClassList("containsItem");
            return true;
        }

        // TODO: Display some sort of error to the user through the Game UI
        Debug.Log("All inventory slots are full!");
        return false;
    }

    /// <summary>
    /// Retrieve the InventoryItem within the currently selected inventory slot.
    /// </summary>
    /// <returns>The InventoryItem within the currently selected inventory slot.</returns>
    public InventoryItem GetItem(string slotKey = null)
    {
        return _inventorySlots[slotKey ?? _currentSlot];
    }

    /// <summary>
    /// Removes the item from the currently selected inventory slot.
    /// </summary>
    /// <returns>The gameObject that was removed from the inventory slot if it exists.</returns>
    public InventoryItem RemoveItem()
    {
        if (!isLocalPlayer) { return null; }

        InventoryItem removedItem = _inventorySlots[_currentSlot];

        if (removedItem is EquippableItem equippableItem)
        {
            CmdUnequip(equippableItem);
        }

        _inventorySlots[_currentSlot] = null;
        _inventoryHotbar.Q<VisualElement>(_currentSlot).RemoveFromClassList("containsItem");

        return removedItem;
    }

    // TODO: Documentation
        // Update the currently selected item in the player's hand.
    private void UpdateSelectedItem(string previousSlot)
    {
        InventoryItem previousItem = GetItem(previousSlot);
        InventoryItem currentItem = GetItem();

        if (previousItem is EquippableItem previousEquippableItem && previousEquippableItem.EquipmentCategory == EquippableItem.EquippableCategory.Hand)
        {
            CmdUnequip(previousEquippableItem);
        }

        if (currentItem is EquippableItem currentEquippableItem && currentEquippableItem.EquipmentCategory == EquippableItem.EquippableCategory.Hand)
        {
            CmdEquip(currentEquippableItem);
        }
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
                // This is designed to be highly generic so that it can work with different equippable items without the need to hardcode it.                
                var itemProperties = restrictionType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                for (int i = 0; i < itemProperties.Length; i++)
                {
                    // If the item category matches with the restricted category, then we can attempt to assign this item to the specified slot.
                    if (restrictionCategory == itemProperties[i].GetValue(inventoryItem).ToString())
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

    // TODO: Documentation
    // Check to see if we should render this weapon, as the player should only be equipping items they are currently using.
    private bool ShouldEquip(EquippableItem equippableItem)
    {
        GameObject equippableReference = FindEquippableReference(equippableItem.EquipmentCategory);

        if (equippableItem.EquipmentCategory == EquippableItem.EquippableCategory.Hand)
        {
            if (equippableReference?.transform.childCount != 0)
            {
                return false;
            }

            if (GetItem() is EquippableItem selectedEquippableItem)
            {
                if (selectedEquippableItem.EquipmentCategory != EquippableItem.EquippableCategory.Hand)
                {
                    return false;
                }
            }
        }

        return true;
    }

    // TODO: Documentation
    [Command]
    private void CmdEquip(EquippableItem equippableItem)
    {
        GameObject equippableReference = FindEquippableReference(equippableItem.EquipmentCategory);

        // Since it will take a small amount of time to swap between items, the total child count for this reference should be less than 2.
        if (equippableReference != null && equippableReference.transform.childCount < 2)
        {
            Vector3 spawnPosition = equippableReference.transform.position + equippableItem.PositionOffset;
            Quaternion spawnRotation = equippableReference.transform.rotation * equippableItem.ObjectPrefab.transform.rotation;

            GameObject equippedObject = Instantiate(equippableItem.ObjectPrefab, spawnPosition, spawnRotation);            
            NetworkServer.Spawn(equippedObject);

            RpcEquip(equippableItem, equippedObject);
        }
    }

    // TODO: Documentation
    [Command]
    private void CmdUnequip(EquippableItem equippableItem)
    {
        GameObject equippableReference = FindEquippableReference(equippableItem.EquipmentCategory);

        if (equippableReference != null && equippableReference.transform.childCount > 0)
        {
            GameObject targetObject = equippableReference.transform.GetChild(0).gameObject;
            
            NetworkServer.Destroy(targetObject);
            Destroy(targetObject);
        }
    }

    // TODO: Documentation
    [ClientRpc]
    private void RpcEquip(EquippableItem equippableItem, GameObject equippedObject)
    {
        GameObject equippableReference = FindEquippableReference(equippableItem.EquipmentCategory);
        equippedObject.transform.SetParent(equippableReference.transform);
    }

    // TODO: Documentation
    private GameObject FindEquippableReference(EquippableItem.EquippableCategory equippableCategory)
    {
        switch (equippableCategory)
        {
            case EquippableItem.EquippableCategory.Hand:
                return _handReference;

            case EquippableItem.EquippableCategory.Head:
                return _headReference;

            default:
                Debug.LogWarning($"CmdEquip() support for {equippableCategory} has not yet been implemented.");
                return null;
        }
    }
}