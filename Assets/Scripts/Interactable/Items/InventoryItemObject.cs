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
    /// 
    /// </summary>
    /// <param name="playerObject">The player interacting with the object</param>
    public void Interact(GameObject playerObject)
    {
        if (playerObject.TryGetComponent(out PlayerController playerController))
        {
            if (playerController.PlayerInventory.AddItem(InventoryItem))
            {
                Destroy(gameObject);
            }
        }
    }
}
