using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Represent's a player's inventory with multiple slots to storage and manage items.
/// Additionally handles the displaying of items on the player's body across the network.
/// </summary>
[RequireComponent(typeof(PlayerInterface))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerInventory : NetworkBehaviour
{
    [System.Serializable]
    private struct EquippableReference
    {
        public InventoryItem.InventoryCategory InventoryCategory;
        public GameObject Reference;
    }

    private struct InventoryRestriction
    {
        public InventoryItem.InventoryCategory ItemCategory { get; private set; }
        public Type ItemType { get; private set; }

        public InventoryRestriction(InventoryItem.InventoryCategory itemCategory, Type itemType)
        {
            ItemCategory = itemCategory;
            ItemType = itemType;
        }

        public override bool Equals(object otherObj)
        {
            return otherObj is InventoryRestriction otherInventoryRestriction &&
                ItemCategory == otherInventoryRestriction.ItemCategory &&
                ItemType == otherInventoryRestriction.ItemType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ItemCategory, ItemType);
        }
    }

    private struct Renderer
    {
        public Mesh Mesh;
        public Material[] Materials;
    }

    [SerializeField] private Animator _playerAnimator;
    [SerializeField] private List<EquippableReference> _equippableReferences;

    private readonly SyncDictionary<string, InventoryItem> _inventorySlots = new SyncDictionary<string, InventoryItem>();

    private string _currentSlot;
    private List<string> _inventoryKeys;
    private Dictionary<string, InventoryRestriction> _inventoryRestrictions;
    private Dictionary<InventoryItem.InventoryCategory, Renderer> _initialRenderers;
    private Dictionary<InventoryItem.InventoryCategory, GameObject> _equippedRenderers;
    private PlayerInterface _playerInterface;
    private PlayerHealth _playerHealth;
    private PlayerStats _playerStats;

    private AudioManager _audioManager;

    public List<InventoryItem> Inventory
    {
        get { return _inventorySlots.Values.ToList(); }
    }

    /// <summary>
    /// Initializes dictionaries for tracking the initial and equipped renderers,
    /// mapping inventory categories to their respective visual representations.
    /// </summary>
    private void Awake()
    {
        _audioManager = gameObject.GetComponent<AudioManager>();
        _initialRenderers = new Dictionary<InventoryItem.InventoryCategory, Renderer>();
        _equippedRenderers = new Dictionary<InventoryItem.InventoryCategory, GameObject>();

        foreach (EquippableReference equippableReference in _equippableReferences)
        {
            _equippedRenderers[equippableReference.InventoryCategory] = equippableReference.Reference;
        }

        foreach ((InventoryItem.InventoryCategory inventoryCategory, GameObject initialRender) in _equippedRenderers)
        {
            SkinnedMeshRenderer skinnedMeshRenderer = initialRender.GetComponent<SkinnedMeshRenderer>();

            _initialRenderers[inventoryCategory] = new Renderer
            {
                Mesh = skinnedMeshRenderer.sharedMesh,
                Materials = skinnedMeshRenderer.materials,
            };
        }
    }

    /// <summary>
    /// Called when the server starts. 
    /// This function initializes the inventory slots to null, effectively clearing the server's default inventory.
    /// </summary>
    public override void OnStartServer()
    {
        _inventorySlots["Slot-1"] = null;
        _inventorySlots["Slot-2"] = null;
        _inventorySlots["Slot-3"] = null;
        _inventorySlots["Slot-4"] = null;
        _inventorySlots["Slot-5"] = null;
        // _inventorySlots["Slot-6"] = null;
        // _inventorySlots["Slot-7"] = null;
        // _inventorySlots["Slot-8"] = null;
        // _inventorySlots["Slot-9"] = null;

        base.OnStartServer();
    }

    /// <summary>
    /// Called on the client when the client gains authority over the player object.
    /// This function sets up inventory slot restrictions, sorts inventory keys, retrieves
    /// references to player components, and sets the initial active inventory slot.
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        // Define inventory slot restrictions
        _inventoryRestrictions = new Dictionary<string, InventoryRestriction>
        {
            { "Slot-1", new InventoryRestriction(InventoryItem.InventoryCategory.Weapon, typeof(MeleeWeapon)) },
            { "Slot-2", new InventoryRestriction(InventoryItem.InventoryCategory.Weapon, typeof(RangedWeapon)) },
            { "Slot-3", new InventoryRestriction(InventoryItem.InventoryCategory.Helmet, typeof(Armor)) },
            { "Slot-4", new InventoryRestriction(InventoryItem.InventoryCategory.Chest, typeof(Armor)) },
            { "Slot-5", new InventoryRestriction(InventoryItem.InventoryCategory.Legs, typeof(Armor)) },
        };

        // Extract and sort the inventory keys to ensure proper cycling
        _inventoryKeys = _inventorySlots.Keys.OrderBy(slotKey => int.Parse(slotKey.Split("-")[1])).ToList();

        // Retrieve references to player-related components
        _playerInterface = gameObject.GetComponent<PlayerInterface>();
        _playerHealth = gameObject.GetComponent<PlayerHealth>();
        _playerStats = gameObject.GetComponent<PlayerStats>();

        // Default to "Slot-1"
        _currentSlot = "Slot-1";
        SelectSlot(_currentSlot);

        base.OnStartLocalPlayer();
    }

    /// <summary>
    /// Loads the player's inventory on the client.
    /// This function is called by the server to synchronize the client's inventory with the saved data.
    /// </summary>
    /// <param name="clientConnection">The network connection of the client whose inventory is being loaded.</param>
    /// <param name="inventoryItems">A list of InventoryItem objects representing the player's inventory.</param>
    [TargetRpc]
    public void TargetLoadInventory(NetworkConnectionToClient clientConnection, List<InventoryItem> inventoryItems)
    {
        foreach (InventoryItem inventoryItem in inventoryItems)
        {
            if (inventoryItem != null)
            {
                AddItem(inventoryItem);
            }
        }
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
                UnityUtils.LogError($"{upcomingSlot} is not a valid inventory slot key.");
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
        InventoryItem inventoryItem = _inventorySlots[upcomingSlot];
        string previousSlot = _currentSlot;

        ResetSlots();
        _currentSlot = upcomingSlot;
        _playerInterface.ActivateSlot(_currentSlot);

        if (inventoryItem != null)
        {
            _playerInterface.DisplayInventoryMessage(inventoryItem.name);
        }

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
            UnityUtils.LogError($"{_currentSlot} is not a valid inventory slot key.");
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
        foreach (string slotKey in _inventorySlots.Keys)
        {
            _playerInterface.DeactivateSlot(slotKey);
        }
    }

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
        if (inventoryItem == null) { return false; }

        string availableSlot = FindAvailableSlot(inventoryItem);

        if (availableSlot != null)
        {
            if (inventoryItem is Armor armorItem)
            {
                ApplyStats(armorItem);
            }

            if (ShouldEquip(inventoryItem))
            {
                CmdEquip(inventoryItem);
            }

            CmdAddItem(availableSlot, inventoryItem);
            _playerInterface.RenderInventoryIcon(availableSlot, inventoryItem.ObjectSprite);
            return true;
        }

        _playerInterface.DisplayInventoryMessage("INVENTORY FULL!");
        return false;
    }

    /// <summary>
    /// Adds an item to the specified inventory slot on the server.
    /// </summary>
    /// <param name="availableSlot">The slot to assign the item to.</param>
    /// <param name="inventoryItem">The item to add.</param>
    [Command]
    private void CmdAddItem(string availableSlot, InventoryItem inventoryItem)
    {
        _inventorySlots[availableSlot] = inventoryItem;
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

        if (removedItem != null)
        {
            if (removedItem is Armor armorItem)
            {
                RemoveStats(armorItem);
            }

            CmdUnequip(removedItem);
        }

        CmdRemoveItem(_currentSlot);
        _playerInterface.RenderInventoryIcon(_currentSlot, null);

        return removedItem;
    }

    /// <summary>
    /// Removes an item from the specified inventory slot on the server.
    /// </summary>
    /// <param name="slotKey">The key of the slot to clear.</param>
    [Command]
    private void CmdRemoveItem(string slotKey)
    {
        _inventorySlots[slotKey] = null;
    }

    /// <summary>
    /// Updates the currently selected, handheld inventory item.
    /// </summary>
    /// <param name="previousSlot">The inventory slot that was previously selected.</param>
    private void UpdateSelectedItem(string previousSlot)
    {
        InventoryItem previousItem = GetItem(previousSlot);
        InventoryItem currentItem = GetItem();

        if (previousItem != null && previousItem.ItemCategory == InventoryItem.InventoryCategory.Weapon)
        {
            CmdUnequip(previousItem);
        }

        if (currentItem != null && currentItem.ItemCategory == InventoryItem.InventoryCategory.Weapon)
        {
            CmdEquip(currentItem);
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
    private bool ShouldEquip(InventoryItem inventoryItem)
    {
#if UNITY_EDITOR
        if (_equippedRenderers == null)
        {
            Debug.Log("Equipped renderers is null");
        }
        if (inventoryItem == null)
        {
            Debug.Log("Inventory item is null");
        }
        else if (_equippedRenderers[inventoryItem.ItemCategory] == null)
        {
            Debug.Log("Whole thing is null");
        }
#endif

        GameObject equippableReference = _equippedRenderers[inventoryItem.ItemCategory];

        if (inventoryItem.ItemCategory == InventoryItem.InventoryCategory.Weapon)
        {
            InventoryRestriction inventoryRestriction = new InventoryRestriction(InventoryItem.InventoryCategory.Weapon, inventoryItem.GetType());
            bool isSlotValid = _currentSlot == GeneralUtils.LocateDictionaryKey(_inventoryRestrictions, inventoryRestriction);
            bool isHandheld = GetItem() is not InventoryItem { ItemCategory: InventoryItem.InventoryCategory.Weapon };
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
    }

    /// <summary>
    /// Command to equip the given equippable item on the player.
    /// This item is instantiated at the appropriate position and rotation.
    /// This function is executed on the server and synchronizes the equipment action across all clients.
    /// </summary>
    /// <param name="equippableItem">The equippable item to equip.</param>
    [Command]
    private void CmdEquip(InventoryItem inventoryItem)
    {
        GameObject equippableReference = _equippedRenderers[inventoryItem.ItemCategory];

        if (equippableReference != null)
        {
            RpcEquip(inventoryItem);
        }
    }

    /// <summary>
    /// Command to unequip the given equippable item from the player.
    /// The item is destroyed from the reference slot and removed from the player.
    /// This function is executed on the server and synchronizes the unequip action across all clients.
    /// </summary>
    /// <param name="inventoryItem">The inventory item to unequip.</param>
    [Command]
    private void CmdUnequip(InventoryItem inventoryItem)
    {
        GameObject equippableReference = _equippedRenderers[inventoryItem.ItemCategory];

        if (equippableReference != null)
        {
            RpcUnequip(inventoryItem);
        }
    }

    /// <summary>
    /// ClientRpc method to synchronize the equipping of an item across clients.
    /// This method is called to update the client's local state of the equipment.
    /// </summary>
    /// <param name="inventoryItem">The equippable item to equip.</param>
    /// <param name="equippedObject">The GameObject representing the equipped item.</param>
    [ClientRpc]
    private void RpcEquip(InventoryItem inventoryItem)
    {
        if (inventoryItem is Weapon weaponItem)
        {
            _audioManager.ChangeAudio("Weapon", weaponItem.AttackAudio);
            _playerAnimator.runtimeAnimatorController = weaponItem.AnimatorController;
        }

        GameObject equippableReference = _equippedRenderers[inventoryItem.ItemCategory];
        SkinnedMeshRenderer skinnedMeshRenderer = equippableReference.GetComponent<SkinnedMeshRenderer>();

        skinnedMeshRenderer.sharedMesh = inventoryItem.SkinnedMeshRenderer.sharedMesh;
        skinnedMeshRenderer.sharedMaterials = inventoryItem.SkinnedMeshRenderer.sharedMaterials;
    }

    /// <summary>
    /// Restores the initial mesh adn materials for the specified inventory item to all clients.
    /// </summary>
    /// <param name="inventoryItem">The item to unequip.</param>
    [ClientRpc]
    private void RpcUnequip(InventoryItem inventoryItem)
    {
        Renderer initialRenderer = _initialRenderers[inventoryItem.ItemCategory];
        GameObject equippableReference = _equippedRenderers[inventoryItem.ItemCategory];
        SkinnedMeshRenderer skinnedMeshRenderer = equippableReference.GetComponent<SkinnedMeshRenderer>();

        skinnedMeshRenderer.sharedMesh = initialRenderer.Mesh;
        skinnedMeshRenderer.materials = initialRenderer.Materials;
    }
}