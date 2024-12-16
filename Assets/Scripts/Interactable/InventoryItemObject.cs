using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
public class InventoryItemObject : MonoBehaviour, IInteractable
{
    public InventoryItem InventoryItem;

    // TODO: Documentation
    private void Start()
    {
        if (InventoryItem != null)
        {
            Transform currentTransform = transform;
            Instantiate(InventoryItem.ObjectPrefab, currentTransform);
        }
    }

    /// <summary>
    /// Destroys the physical gameObject that the player is interacting with.
    /// Additional logic regarding adding the item to the player's inventory is handled within the `PlayerController` and `PlayerInventory` scripts.
    /// </summary>
    /// <param name="playerObject">The player interacting with the object</param>
    public void Interact(GameObject playerObject)
    {
        Destroy(gameObject);
    }
}
