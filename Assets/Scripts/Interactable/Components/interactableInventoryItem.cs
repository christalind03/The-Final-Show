using Mirror;
using UnityEngine;

/// <summary>
/// Represents an interactive in-game object linked to an <see cref="InventoryItem"/>.
/// This object can be picked up by a player and added to their inventory.
/// </summary>
[RequireComponent(typeof(NetworkIdentity))]
public class InteractableInventoryItem : NetworkBehaviour, IInteractable
{
    [SerializeField] private bool _isSkinned;

    public InventoryItem InventoryItem;

    private SkinnedMeshRenderer _skinnedMeshRenderer;
    private Mesh _initialMesh;
    private Material[] _initialMaterials;

    /// <summary>
    /// Sets the object's visual representation based on the assigned inventory item.
    /// </summary>
    private void Start()
    {
        if (InventoryItem != null)
        {
            if (_isSkinned)
            {
                _skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

                _initialMesh = _skinnedMeshRenderer.sharedMesh;
                _initialMaterials = _skinnedMeshRenderer.materials;

                _skinnedMeshRenderer.sharedMesh = InventoryItem.SkinnedMeshRenderer.sharedMesh;
                _skinnedMeshRenderer.sharedMaterials = InventoryItem.SkinnedMeshRenderer.sharedMaterials;
            }
            else
            {
                Transform currentTransform = transform;
                Instantiate(InventoryItem.ObjectPrefab, currentTransform);
            }
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
                CmdRemove();
            }
        }
    }

    /// <summary>
    /// Handles the destruction of this object on the server and propagates the necessary updates to all clients.
    /// </summary>
    [Command(requiresAuthority = false)]
    private void CmdRemove()
    {
        if (_isSkinned)
        {
            RpcRemove();
        }
        else
        {
            NetworkServer.Destroy(gameObject);
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Resets the skinned mesh renderer to its initial state on all clients.
    /// </summary>
    [ClientRpc]
    private void RpcRemove()
    {
        _skinnedMeshRenderer.sharedMesh = _initialMesh;
        _skinnedMeshRenderer.materials = _initialMaterials;
    }
}
