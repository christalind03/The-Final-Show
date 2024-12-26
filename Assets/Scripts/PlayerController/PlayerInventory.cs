using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// Represent's a player's inventory with multiple slots to storage and manage items.
/// Additionally handles the displaying of items on the player's body across the network.
/// </summary>
[RequireComponent(typeof(PlayerStats))]
public class PlayerInventory : NetworkBehaviour
{
    private struct PlayerInventoryRestriction
    {
        public EquippableItem.EquippableCategory ItemCategory { get; private set; }
        public Type ItemType { get; private set; }

        public PlayerInventoryRestriction(EquippableItem.EquippableCategory itemCategory, Type itemType)
        {
            ItemCategory = itemCategory;
            ItemType = itemType;
        }

        public override bool Equals(object otherObj)
        {
            return otherObj is PlayerInventoryRestriction otherInventoryRestriction && 
                ItemCategory == otherInventoryRestriction.ItemCategory &&
                ItemType == otherInventoryRestriction.ItemType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ItemCategory, ItemType);
        }
    }

    [Header("Equippable References")]
    [SerializeField] private GameObject _handReference;
    [SerializeField] private GameObject _headReference;

    private string _currentSlot;
    private List<string> _inventoryKeys;
    private Dictionary<string, InventoryItem> _inventorySlots;
    private Dictionary<string, PlayerInventoryRestriction> _inventoryRestrictions;
    private VisualElement _inventoryHotbar;
    private PlayerHealth _playerHealth;
    private PlayerStats _playerStats;

    /// <summary>
    /// Sets up the default slot and creates an empty inventory with inventory restrictions.
    /// </summary>
    public override void OnStartAuthority()
    {
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

        // Extract and sort the inventory keys to ensure proper cycling
        _inventoryKeys = _inventorySlots.Keys.OrderBy(slotKey => int.Parse(slotKey.Split("-")[1])).ToList();

        // Retrieve references to player-related components
        _inventoryHotbar = gameObject.GetComponent<UIDocument>().rootVisualElement;
        _playerHealth = gameObject.GetComponent<PlayerHealth>();
        _playerStats = gameObject.GetComponent<PlayerStats>();

        // Default to Slot 1
        string defaultSlot = "Slot-1";
        _currentSlot = defaultSlot;
        SelectSlot(defaultSlot);

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
        if (context.control.device is Keyboard)
        {
            string upcomingSlot = context.action.name;

            if (_inventorySlots.ContainsKey(upcomingSlot))
            {
                SelectSlot(upcomingSlot);
            }
            else
            {
                Debug.LogError($"{upcomingSlot} is not a valid inventory slot key.");
            }
        }
        
        // Handle input from mouse scroll wheel.
        if (context.control.device is Mouse)
        {
            Vector2 scrollValue = context.ReadValue<Vector2>();
            int scrollDirection = 0 < scrollValue.y ? 1 : -1;

            HandleScroll(scrollDirection);
        }
    }

    /// <summary>
    /// Updates the currently active inventory slot and refreshes the UI.
    /// </summary>
    /// <param name="upcomingSlot">The key representing the inventory slot to activate.</param>
    private void SelectSlot(string upcomingSlot)
    {
        string previousSlot = _currentSlot;

        ResetSlots();
        _currentSlot = upcomingSlot;
        _inventoryHotbar.Q<VisualElement>(_currentSlot).AddToClassList("active");

        UpdateSelectedItem(previousSlot);
    }

    /// <summary>
    /// Handles cycling through inventory slots based on scroll wheel input.
    /// </summary>
    /// <param name="scrollDirection">
    /// The direction of the scroll where 1 allows the user to cycle forward and -1 allows the user to cycle backwards.
    /// </param>
    private void HandleScroll(int scrollDirection)
    {
        int currentIndex = _inventoryKeys.IndexOf(_currentSlot);
        if (currentIndex == -1)
        {
            Debug.LogError($"{_currentSlot} is not a valid inventory slot key.");
            return;
        }

        int upcomingIndex = (currentIndex + scrollDirection + _inventoryKeys.Count) % _inventoryKeys.Count;

        SelectSlot(_inventoryKeys[upcomingIndex]);
    }

    /// <summary>
    /// Resets the styles of all slots to the default style.
    /// </summary>
    private void ResetSlots()
    {
        foreach (string actionName in _inventorySlots.Keys)
        {
            _inventoryHotbar.Q<VisualElement>(actionName).RemoveFromClassList("active");
        }
    }

    // TODO: Update documentation?
    /// <summary>
    /// Add an item to the currently selected inventory slot.
    /// If the item is equippable, it may be equipped automatically, and stats may be applied if the item is of the <see cref="Armor"/> type.
    /// </summary>
    /// <param name="inventoryItem">The item to add to the inventory slot.</param>
    /// <returns>
    /// <c>true</c> if the item was successfully added to the inventory; otherwise <c>false</c>.
    /// </returns>
    public bool AddItem(InventoryItem inventoryItem)
    {
        if (!isLocalPlayer) { return false; }

        string availableSlot = FindAvailableSlot(inventoryItem);

        if (availableSlot != null)
        {
            if (inventoryItem is EquippableItem equippableItem)
            {
                if (equippableItem is Armor armorItem)
                {
                    ApplyStats(armorItem);
                }

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
    /// <param name="slotKey">
    /// The key representing the inventory slot to retrieve the item from.
    /// If <c>null</c>, the item from the currently selected slot is returned.
    /// </param>
    /// <returns>The InventoryItem within the specified selected inventory slot, or currently selected slot if no key is provided.</returns>
    public InventoryItem GetItem(string slotKey = null)
    {
        return _inventorySlots[slotKey ?? _currentSlot];
    }

    /// <summary>
    /// Removes the item from the currently selected inventory slot.
    /// If the item is equippable, it is unequipped, and and associated stats are removed.
    /// </summary>
    /// <returns>The gameObject that was removed from the inventory slot, or <c>null</c> if no item exists in the selected slot.</returns>
    public InventoryItem RemoveItem()
    {
        if (!isLocalPlayer) { return null; }

        InventoryItem removedItem = _inventorySlots[_currentSlot];

        if (removedItem is EquippableItem equippableItem)
        {
            if (equippableItem is Armor armorItem)
            {
                RemoveStats(armorItem);
            }

            CmdUnequip(equippableItem);
        }

        _inventorySlots[_currentSlot] = null;
        _inventoryHotbar.Q<VisualElement>(_currentSlot).RemoveFromClassList("containsItem");

        return removedItem;
    }

    /// <summary>
    /// Updates the currently selected, handheld inventory item.
    /// </summary>
    /// <param name="previousSlot">The inventory slot that was previously selected.</param>
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

    /// <summary>
    /// Finds the first available inventory slot that can accomodate the given item.
    /// </summary>
    /// <param name="inventoryItem">The inventory item to be placed into a slot</param>
    /// <returns>The key of the first available inventory slot that can hold the item, or <c>null</c> if no slots are available.</returns>
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

    /// <summary>
    /// Determines whether the given equippable item should be equipped based on the player's current slot and the category of the item.
    /// This ensures that items in the "Hand" category are only equipped if the player isn't already holding an item, and if the slot is valid.
    /// </summary>
    /// <param name="equippableItem">The equippable item to check.</param>
    /// <returns><c>true</c> if the item should be equipped, otherwise <c>false</c>.</returns>
    private bool ShouldEquip(EquippableItem equippableItem)
    {
        GameObject equippableReference = FindEquippableReference(equippableItem.EquipmentCategory);

        if (equippableItem.EquipmentCategory == EquippableItem.EquippableCategory.Hand)
        {
            PlayerInventoryRestriction inventoryRestriction = new PlayerInventoryRestriction(EquippableItem.EquippableCategory.Hand, equippableItem.GetType());
            bool isSlotValid = _currentSlot == UnityExtensions.FindKey(_inventoryRestrictions, inventoryRestriction);
            bool isHandheld = GetItem() is not EquippableItem { EquipmentCategory: EquippableItem.EquippableCategory.Hand };
            bool isEmpty = equippableReference?.transform.childCount == 0;

            return isSlotValid && isHandheld && isEmpty;
        }

        return true;
    }

    /// <summary>
    /// Applies the stat modifiers from the given armor item to the player.
    /// </summary>
    /// <param name="armorItem">The armor item whose stats will be applied to the player.</param>
    private void ApplyStats(Armor armorItem)
    {
        _playerHealth.CmdAddModifier(armorItem.Health);
        _playerStats.Attack.AddModifier(armorItem.Attack);
        _playerStats.Defense.AddModifier(armorItem.Defense);
        _playerStats.Stamina.AddModifier(armorItem.Stamina);

        DebugStats();
    }

    /// <summary>
    /// Removes the stat modifiers from the given armor item from the player.
    /// </summary>
    /// <param name="armorItem">The armor item whose stats will be removed from the player.</param>
    private void RemoveStats(Armor armorItem)
    {
        _playerHealth.CmdRemoveModifier(armorItem.Health);
        _playerStats.Attack.RemoveModifier(armorItem.Attack);
        _playerStats.Defense.RemoveModifier(armorItem.Defense);
        _playerStats.Stamina.RemoveModifier(armorItem.Stamina);

        DebugStats();
    }

    /// <summary>
    /// A debug method to log the current stats of the player.
    /// This is used to verify that stat changes have been applied correctly.
    /// This function will be removed once a more defined UI system is in place to display stat changes.
    /// </summary>
    private void DebugStats()
    {
        Debug.Log($"Player Attack: {_playerHealth.BaseValue}, {_playerHealth.CurrentValue}");
        Debug.Log($"Player Attack: {_playerStats.Attack.BaseValue}, {_playerStats.Attack.CurrentValue}");
        Debug.Log($"Player Defense: {_playerStats.Defense.BaseValue}, {_playerStats.Defense.CurrentValue}");
        Debug.Log($"Player Stamina: {_playerStats.Stamina.BaseValue}, {_playerStats.Stamina.CurrentValue}");
    }

    /// <summary>
    /// Command to equip the given equippable item on the player.
    /// This item is instantiated at the appropriate position and rotation.
    /// This function is executed on the server and synchronizes the equipment action across all clients.
    /// </summary>
    /// <param name="equippableItem">The equippable item to equip.</param>
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

    /// <summary>
    /// Command to unequip the given equippable item from the player.
    /// The item is destroyed from the reference slot and removed from the player.
    /// This function is executed on the server and synchronizes the unequip action across all clients.
    /// </summary>
    /// <param name="equippableItem">The equippable item to unequip.</param>
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

    /// <summary>
    /// ClientRpc method to synchronize the equipping of an item across clients.
    /// This method is called to update the client's local state of the equipment.
    /// </summary>
    /// <param name="equippableItem">The equippable item to equip.</param>
    /// <param name="equippedObject">The GameObject representing the equipped item.</param>
    [ClientRpc]
    private void RpcEquip(EquippableItem equippableItem, GameObject equippedObject)
    {
        GameObject equippableReference = FindEquippableReference(equippableItem.EquipmentCategory);
        equippedObject.transform.SetParent(equippableReference.transform);
    }

    /// <summary>
    /// Finds the reference GameObject where the given equippable item should be attached based on its category.
    /// </summary>
    /// <param name="equippableCategory">The category of the equippable item.</param>
    /// <returns>
    /// The GameObject reference where the equippable item should be attached, or <c>null</c> if no reference exists for the given category.
    /// </returns>
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