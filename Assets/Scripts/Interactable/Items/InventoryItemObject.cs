using Mirror;
using UnityEngine;

/// <summary>
/// Represents an interactive in-game object linked to an <see cref="InventoryItem"/>.
/// This object can be picked up by a player and added to their inventory.
/// </summary>
[RequireComponent(typeof(NetworkIdentity))]
public class InventoryItemObject : NetworkBehaviour, IInteractable
{
    public InventoryItem InventoryItem;

    /// <summary>
    /// Initializes the object by instantiating its visual representation from the item prefab.
    /// </summary>
    private void Start()
    {
        if (InventoryItem != null)
        {
            Transform currentTransform = transform;
            Instantiate(InventoryItem.ObjectPrefab, currentTransform);
        }
    }

    /// <summary>
    /// Allows a player to interact with the object, attempting to add the associated inventory item to their inventory.
    /// If successful, the object is destroyed.
    /// </summary>
    /// <param name="playerObject">The player interacting with the object</param>
    public void Interact(GameObject playerObject)
    {
        if (playerObject.TryGetComponent(out PlayerInventory playerInventory))
        {
            if (playerInventory.AddItem(InventoryItem))
            {
                CmdDestroy();
            }
        }
    }

    /// <summary>
    /// Destroys this object on the server and propagates the destructiont to all clients.
    /// </summary>
    [Command(requiresAuthority = false)]
    private void CmdDestroy()
    {
        NetworkServer.Destroy(gameObject);
        Destroy(gameObject);
    }
}
