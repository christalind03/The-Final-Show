using Mirror;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
public class InventoryItemObject : NetworkBehaviour, IInteractable
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
    /// 
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

    // TODO: Documentation
    [Command(requiresAuthority = false)]
    private void CmdDestroy()
    {
        NetworkServer.Destroy(gameObject);
        Destroy(gameObject);
    }
}
