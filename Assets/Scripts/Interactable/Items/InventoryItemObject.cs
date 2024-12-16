using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity), typeof(NetworkTransformReliable))]
public class InventoryItemObject : MonoBehaviour, IInteractable
{
    [SerializeField] private InventoryItem _inventoryItem;

    public InventoryItem InventoryItem { get { return _inventoryItem; } }

    private void Start()
    {
        Transform currentTransform = transform;
        Instantiate(_inventoryItem.ObjectPrefab, currentTransform);

        NetworkTransformReliable networkTransform = GetComponent<NetworkTransformReliable>();
        networkTransform.syncDirection = SyncDirection.ServerToClient; // For items, the server should be sending data to the clients.
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="playerObject"></param>
    public void Interact(GameObject playerObject)
    {
        Destroy(gameObject);
    }
}
