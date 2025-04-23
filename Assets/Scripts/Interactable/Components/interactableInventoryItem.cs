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
    [SerializeField] private InventoryItem[] _possibleItems;

    [SyncVar(hook = nameof(OnItemChanged))]
    private string _itemName;
    private InventoryItem _inventoryItem;
    public InventoryItem InventoryItem
    {
        get => _inventoryItem;
        set
        {
            if (isServer)
            {
                _itemName = value?.name ?? string.Empty;
            }
            _inventoryItem = value;
            if (value != null)
            {
                SetupVisuals();
            }
        }
    }

    private SkinnedMeshRenderer _skinnedMeshRenderer;
    private Mesh _initialMesh;
    private Material[] _initialMaterials;

    [Tooltip("Drop settings")]
    [SerializeField] private float _rotationSpeed = 10f;
    [SerializeField] private float _dropSpeed = 1f;

    [Tooltip("The angle measured in degress upwards from the horizontal")]
    [Range(0f, 90f)]
    [SerializeField] private float _dropAngle = 45f;

    private Rigidbody rb;
    private bool dropped = false;

    private void OnItemChanged(string oldName, string newName)
    {
        if (!string.IsNullOrEmpty(newName))
        {
            _inventoryItem = Resources.Load<InventoryItem>($"Items/{newName}");
            if (_inventoryItem != null)
            {
                SetupVisuals();
            }
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        if (_possibleItems != null && _possibleItems.Length > 0 && InventoryItem == null)
        {
            InventoryItem = _possibleItems[Random.Range(0, _possibleItems.Length)];
        }
    }

    private void SetupVisuals()
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
        dropped = false;
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
            // Reset the skinned mesh renderer on the server first
            if (_skinnedMeshRenderer != null)
            {
                _skinnedMeshRenderer.sharedMesh = _initialMesh;
                _skinnedMeshRenderer.materials = _initialMaterials;
            }
            
            // Then tell all clients to do the same
            RpcRemove();
        }
        else
        {
            // For non-skinned objects, just remove the child object
            RpcRemove();
        }
    }

    /// <summary>
    /// Resets the skinned mesh renderer to its initial state on all clients.
    /// Additionally prevents the user from duplicating items from mannequins.
    /// </summary>
    [ClientRpc]
    private void RpcRemove()
    {
        if (_isSkinned)
        {
            if (_skinnedMeshRenderer != null)
            {
                _skinnedMeshRenderer.sharedMesh = _initialMesh;
                _skinnedMeshRenderer.materials = _initialMaterials;
            }
        }
        else
        {
            // For non-skinned objects, destroy the child object
            if (transform.childCount > 0)
            {
                Destroy(transform.GetChild(0).gameObject);
            }
        }
        
        // Clear the inventory item reference
        _inventoryItem = null;
        _itemName = string.Empty;
    }

    /// <summary>
    /// Creates drop physics when a player drops an item
    /// </summary>
    public void Drop(Transform player)
    {
        rb = GetComponent<Rigidbody>();
        dropped = true;
        Vector3 launchDir = Quaternion.AngleAxis(_dropAngle, -player.right) * player.forward;
        rb.AddForce(_dropSpeed * launchDir, ForceMode.VelocityChange);
    }

    /// <summary>
    /// Creates the spinning effect after an item is dropped
    /// </summary>
    private void FixedUpdate()
    {
        if (!dropped) return;
        transform.Rotate(0, Time.deltaTime * _rotationSpeed, 0);
    }
}
