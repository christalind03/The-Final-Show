using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Mirror;
using Org.BouncyCastle.Utilities.IO.Pem;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;
using Unity.VisualScripting.Antlr3.Runtime.Misc;


[RequireComponent(typeof(CharacterController))]
public class PlayerControllerNetwork : NetworkBehaviour
{
    [Header("Camera Parameters")]
    [SerializeField] private float _cameraSensitivity;
    [SerializeField] private float _interactableDistance;

    [Header("Movement Parameters")]
    [SerializeField] private float _gravity;
    [SerializeField] private float _jumpHeight;
    [SerializeField] private float _sprintSpeed;
    [SerializeField] private float _walkSpeed;

    [Header("Stat Parameters")]
    [SerializeField] private Stat _staminaPoints;
    [SerializeField] private float _staminaCost;
    [SerializeField] private float _staminaRestoration;
    [SerializeField] private float _staminaCooldown;

    [Header("Player Parameters")]
    [Tooltip("The scene's main camera.")]
    [SerializeField] private Transform _cameraTransform;

    [Tooltip("An empty object hidden within the Player object to control the camera's rotation.")]
    [SerializeField] private Transform _followTransform;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private CharacterController _playerController;


    [Header("Weapon Parameters")]
    [SerializeField] private Weapon _currentWeapon;
    public Transform playerHandTransform;

    [Header("Armor Components")]
    [SerializeField] private ArmorManager armorManager;
    private bool _isGrounded;
    private bool _isSprinting;

    private bool _canSprint;
    private float _xRotation;
    private float _yRotation;
    private Vector3 _playerVelocity;
    private RaycastHit _raycastHit;
    private PlayerControls _playerControls;
    private PlayerInventory _playerInventory;
    private VisualElement _uiDocument;

    private void Awake()
    {
        _canSprint = true;
        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;

        _uiDocument = GetComponent<UIDocument>().rootVisualElement;

        _playerControls = new PlayerControls();
        _playerInventory = new PlayerInventory(_uiDocument);
    }

    private void OnEnable()
    {
        _playerControls.Enable();

        _playerControls.Player.Attack.performed += Attack;
        _playerControls.Player.AlternateAttack.performed += AlternateAttack;
        _playerControls.Player.Drop.performed += Drop;
        _playerControls.Player.Interact.performed += Interact;
        _playerControls.Player.Jump.performed += Jump;

        _playerControls.Inventory.CycleSlots.performed += _playerInventory.SelectSlot;
        InputActionMap inventoryActions = _playerControls.asset.FindActionMap("Inventory");

        foreach (InputAction inventorySlot in inventoryActions)
        {
            inventorySlot.performed += _playerInventory.SelectSlot;
        }
    }

    private void Update()
    {
        if (!isLocalPlayer) { return; }
        _isGrounded = Physics.Raycast(_playerTransform.position, Vector3.down, 1f) && _playerVelocity.y <= 0f;

        _playerVelocity.y += _gravity * Time.deltaTime;
        _playerController.Move(_playerVelocity * Time.deltaTime);

        HandleLook();
        HandleMovement();
        HandleSprint();
    }

    private void HandleLook()
    {
        Vector2 lookInput = _cameraSensitivity * Time.deltaTime * _playerControls.Player.Look.ReadValue<Vector2>();

        _yRotation += lookInput.x;
        _xRotation = Mathf.Clamp(_xRotation - lookInput.y, -45f, 45f);


        _followTransform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);

        _playerTransform.rotation = Quaternion.Euler(0, _yRotation, 0);

        Physics.Raycast(_cameraTransform.position, _cameraTransform.forward * _interactableDistance, out _raycastHit);

        CmdLook(_playerTransform.rotation);
    }

    private void HandleMovement()
    {
        float totalSpeed = _isSprinting ? _sprintSpeed : _walkSpeed;

        Vector2 moveInput = _playerControls.Player.Movement.ReadValue<Vector2>();
        Vector3 moveDirection = _playerTransform.forward * moveInput.y + _playerTransform.right * moveInput.x;

        _playerVelocity.y += _gravity * Time.deltaTime;

        _playerController.Move(totalSpeed * Time.deltaTime * moveDirection);
        _playerController.Move(_playerVelocity * Time.deltaTime);
        CmdMovePlayer(_playerTransform.position);
    }

    private void HandleSprint()
    {
        Vector2 moveInput = _playerControls.Player.Movement.ReadValue<Vector2>();
        bool isForward = 0 < moveInput.y;

        _isSprinting = _canSprint && isForward && 0 < _staminaPoints.CurrentValue && _playerControls.Player.Sprint.IsPressed();

        if (_isSprinting)
        {
            _staminaPoints.Decrease(_staminaCost * Time.deltaTime);
        }

        if (!_isSprinting && _staminaPoints.CurrentValue < _staminaPoints.BaseValue)
        {
            _staminaPoints.Increase(_staminaRestoration * Time.deltaTime);
        }

        if (_staminaPoints.CurrentValue == 0f)
        {
            _canSprint = false;
            _isSprinting = false;

            StartCoroutine(SprintCooldown());
        }
    }

    private void Attack(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) { return; }
        if (_currentWeapon != null)
        {
            _currentWeapon.Attack();
        }
    }

    private void AlternateAttack(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) { return; }
        if (_currentWeapon != null)
        {
            _currentWeapon.AlternateAttack();
        }
    }

    private void Drop(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) { return; }
        Drop();
    }

    private void Drop(Vector3? targetPosition = null)
    {
        GameObject droppedItem = _playerInventory.RemoveItem();

        if (_raycastHit.collider != null && droppedItem != null)
        {
            if (targetPosition.HasValue)
            {
                CmdDrop(droppedItem, targetPosition.Value, true);
            }
            else
            {
                CmdDrop(droppedItem, new Vector3(0, 0, 0), false);
            }
        }
    }

    private void Interact(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) { return; }
        Collider hitCollider = _raycastHit.collider;
        GameObject hitGameObject = hitCollider.gameObject;

        if (hitCollider != null && hitGameObject.GetComponent<IInteractable>() != null)
        {
            if (_playerInventory.HasItem())
            {
                Drop(hitGameObject.transform.position);
            }

            _playerInventory.AddItem(hitGameObject);
            CmdInteract(hitGameObject);
        }

        if (hitGameObject.TryGetComponent(out Weapon weapon))
        {
            CmdEquipWeapon(weapon);
            Debug.Log($"{weapon.WeaponName} equipped.");
        }
        else if (hitCollider != null && hitCollider.TryGetComponent(out Armor armor))
        {
            armorManager.CmdEquipArmor(armor);
            Debug.Log($"{armor.ArmorName} equipped.");
        }
        else
        {
            Debug.Log($"{hitGameObject.name} is neither a weapon nor armor.");
        }
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) { return; }
        if (_isGrounded)
        {
            _playerVelocity.y = (float)Math.Sqrt(-2f * _gravity * _jumpHeight);
        }
    }

    private void OnDisable()
    {
        _playerControls.Disable();

        _playerControls.Player.Attack.performed -= Attack;
        _playerControls.Player.AlternateAttack.performed -= AlternateAttack;
        _playerControls.Player.Drop.performed -= Drop;
        _playerControls.Player.Interact.performed -= Interact;
        _playerControls.Player.Jump.performed -= Jump;

        _playerControls.Inventory.CycleSlots.performed -= _playerInventory.SelectSlot;
        InputActionMap inventoryActions = _playerControls.asset.FindActionMap("Inventory");

        foreach (InputAction inventorySlot in inventoryActions)
        {
            inventorySlot.performed -= _playerInventory.SelectSlot;
        }
    }

    private IEnumerator SprintCooldown()
    {
        yield return new WaitForSeconds(_staminaCooldown);
        _canSprint = true;
    }

    [Command]
    public void CmdEquipWeapon(Weapon weapon)
    {
        // If there is a currently equipped weapon, drop it and remove authority
        if (_currentWeapon != null)
        {
            CmdDropWeapon(); // Drop the currently equipped weapon

            if (_currentWeapon.TryGetComponent<NetworkIdentity>(out NetworkIdentity currentWeaponIdentity))
            {
                currentWeaponIdentity.RemoveClientAuthority(); // Remove authority from the old weapon
            }
        }

        // Set the new weapon
        _currentWeapon = weapon;

        // Assign authority to the new weapon
        NetworkIdentity weaponIdentity = weapon.GetComponent<NetworkIdentity>();
        if (weaponIdentity != null)
        {
            weaponIdentity.AssignClientAuthority(connectionToClient);
        }

        _currentWeapon.CmdEquip();

        // Make sure the bow is active
        weapon.gameObject.SetActive(true);

        // Attach the weapon to the player's hand
        weapon.transform.SetParent(playerHandTransform);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.Euler(0, -90, 0); // Set the correct rotation for the weapon when held
        weapon.transform.localScale = Vector3.one; // Reset the scale to 1,1,1 to make sure it's visible

        // Make sure the Rigidbody is set to kinematic to avoid physics issues while holding it
        Rigidbody weaponRb = weapon.GetComponent<Rigidbody>();
        if (weaponRb != null)
        {
            weaponRb.isKinematic = true; // Disable physics on the weapon while it is being held
        }

        Debug.Log($"{weapon.WeaponName} has been equipped and attached to the player's hand.");
        RpcEquipWeapon(weapon);
    }

    [Command]
    public void CmdUnequipWeapon()
    {
        if (_currentWeapon != null)
        {
            RpcUnequipWeapon(_currentWeapon);
            _currentWeapon.CmdUnequip();
            _currentWeapon = null;
        }
    }


    [Command]
    private void CmdDropWeapon()
    {
        if (_currentWeapon == null)
        {
            Debug.Log("No weapon to drop.");
            return;
        }
        else
        {
            // Calculate the drop position
            Vector3 dropPosition = transform.position + transform.forward * 2;

            // Detach the weapon
            _currentWeapon.IsEquipped = false;
            _currentWeapon.transform.SetParent(null);
            _currentWeapon.transform.position = dropPosition;

            // Enable physics for the weapon
            Rigidbody weaponRb = _currentWeapon.GetComponent<Rigidbody>();
            if (weaponRb != null)
            {
                weaponRb.isKinematic = false;
            }

            // Remove authority if possible
            if (_currentWeapon.TryGetComponent<NetworkIdentity>(out NetworkIdentity weaponIdentity))
            {
                if (NetworkServer.active)
                {
                    weaponIdentity.RemoveClientAuthority();
                }
                else
                {
                    Debug.LogWarning("Cannot remove client authority as the server is not active.");
                }
            }

            // Propagate the drop to all clients
            RpcDropWeapon(dropPosition);

            Debug.Log($"{_currentWeapon.WeaponName} has been dropped.");

            // Clear the weapon reference
            _currentWeapon = null;
            }
    }

    /// <summary>
    /// Server code for updating player's movement, as of right now the client calculates 
    /// the positions and gives it to the server. Not very safe if cheater that manipulate 
    /// the calculation if we care about security.  
    /// </summary>
    /// <param name="position">Position of the player</param>
    [Command]
    private void CmdMovePlayer(Vector3 position)
    {
        _playerTransform.position = position;

        // Propagates the changes to all client
        RpcUpdatePlayerPosition(position);
    }

    /// <summary>
    /// Server code for updating player's rotation, similar to CmdMovePlayer but with rotation
    /// </summary>
    /// <param name="rotation">Player rotation</param>
    [Command]
    private void CmdLook(Quaternion rotation)
    {
        _playerTransform.rotation = rotation;

        // Propagates the changes to all client
        RpcUpdatePlayerLook(_playerTransform.rotation);
    }

    /// <summary>
    /// Calculate the drop position and set the drop item to that position and activate it. 
    /// </summary>
    /// <param name="dropItem">The item that is being dropped</param>
    /// <param name="targetPosition">Position that the item should be dropped at</param>
    /// <param name="hasValue">Bool for make sure if targetPosition exist since Commands can't have nullable Vector3</param>
    [Command]
    private void CmdDrop(GameObject dropItem, Vector3 targetPosition, bool hasValue)
    {
        Vector3 dropOffset = hasValue ? targetPosition : transform.position + transform.forward * 3;
        dropItem.transform.position = dropOffset;
        dropItem.SetActive(true);

        // Propagates the changes to all client
        RpcDrop(dropItem, dropOffset);
    }

    /// <summary>
    /// Interact with the GameObject if it has IInteractable script
    /// </summary>
    /// <param name="hitObj">The object that is being interacted</param>
    [Command]
    private void CmdInteract(GameObject hitObj)
    {
        if (hitObj.TryGetComponent(out IInteractable interactableObj))
        {
            interactableObj.Interact();
        }

        // Propagates the changes to all client
        RpcInteract(hitObj);
    }

    /// <summary>
    /// Update player position to all clients
    /// </summary>
    /// <param name="position">Player position</param>
    [ClientRpc]
    private void RpcUpdatePlayerPosition(Vector3 position)
    {
        // Local player will not run this code since they've already calculated their own position in HandleMovement
        if (isLocalPlayer) return;
        _playerTransform.position = position;
    }

    /// <summary>
    /// Update player rotation to all clients
    /// </summary>
    /// <param name="rotation">Player rotation</param>
    [ClientRpc]
    private void RpcUpdatePlayerLook(Quaternion rotation)
    {
        if (isLocalPlayer) return;
        _playerTransform.rotation = rotation;
    }

    /// <summary>
    /// Update the item that is being dropped in world view to all client
    /// </summary>
    /// <param name="dropItem">Item that is being dropped</param>
    /// <param name="dropPos">Location to be dropped at</param>
    [ClientRpc]
    private void RpcDrop(GameObject dropItem, Vector3 dropPos)
    {
        // Since we are using a Host-Client network structure, there will be one player that acts as the server and they don't have to run the code again
        if (!isServer)
        {
            dropItem.transform.position = dropPos;
            dropItem.SetActive(true);
        }
    }

    /// <summary>
    /// Update the object that is being interacted to all client
    /// </summary>
    /// <param name="hitObj">Item that is being interacted</param>
    [ClientRpc]
    private void RpcInteract(GameObject hitObj)
    {
        // Same reasoning as RpcDrop with the added component of getting the component "IInteractable"
        if (!isServer && hitObj.TryGetComponent(out IInteractable interactableObj))
        {
            interactableObj.Interact();
        }
    }

    [ClientRpc]
    private void RpcEquipWeapon(Weapon weapon)
    {
        // Set the current weapon on all clients
        _currentWeapon = weapon;

        // Attach to the player's hand visually
        weapon.transform.SetParent(playerHandTransform);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;

        Debug.Log($"[Client] Weapon {weapon.WeaponName} equipped.");
    }

    [ClientRpc]
    private void RpcUnequipWeapon(Weapon weapon)
    {
        // Detach on all clients visually
        weapon.transform.SetParent(null);

        Debug.Log($"[Client] Weapon {weapon.WeaponName} unequipped.");

    }

    [ClientRpc]
    private void RpcDropWeapon(Vector3 dropPosition)
    {
        if (_currentWeapon == null)
        {
            Debug.LogWarning("RpcDropWeapon called but _currentWeapon is null.");
            return;
        }

        // Update weapon position on all clients
        _currentWeapon.transform.SetParent(null);
        _currentWeapon.transform.position = dropPosition;

        Rigidbody weaponRb = _currentWeapon.GetComponent<Rigidbody>();
        if (weaponRb != null)
        {
            weaponRb.isKinematic = false; // Ensure physics is applied
        }

        Debug.Log($"[Client] Weapon dropped at position: {dropPosition}");
    }
}