using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Mirror;
using System.Security.Cryptography;


[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
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
    public GameObject playerHand;

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

    private Animator _playerAnimator;
    private int _animatorMovementX;
    private int _animatorMovementZ;

    /// <summary>
    /// Configure the cursor settings and initialize a new instance of PlayerControls when the script is first loaded.
    /// </summary>
    private void Awake()
    {
        _canSprint = true;
        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;

        _uiDocument = GetComponent<UIDocument>().rootVisualElement;

        _playerControls = new PlayerControls();
        _playerInventory = new PlayerInventory(_uiDocument);

        // To access the animator, we must retrieve the child gameObject that is rendering the player's mesh.
        // This should be the first child of the current gameObject, `BaseCharacter`
        _playerAnimator = transform.GetChild(0).GetComponent<Animator>();
        _animatorMovementX = Animator.StringToHash("Movement X");
        _animatorMovementZ = Animator.StringToHash("Movement Z");
    }

    /// <summary>
    /// Enable the PlayerControls actions and action maps when the component is enabled.
    /// </summary>
    private void OnEnable()
    {
        _playerControls.Enable();

        _playerControls.Player.Attack.performed += Attack;
        _playerControls.Player.AlternateAttack.performed += AlternateAttack;
        _playerControls.Player.Drop.performed += Drop;
        _playerControls.Player.Interact.performed += Interact;
        _playerControls.Player.Jump.performed += Jump;

        // Subscribe to inventory slot selection for all slots.
        _playerControls.Inventory.CycleSlots.performed += _playerInventory.SelectSlot;
        InputActionMap inventoryActions = _playerControls.asset.FindActionMap("Inventory");

        foreach (InputAction inventorySlot in inventoryActions)
        {
            inventorySlot.performed += _playerInventory.SelectSlot;
        }
    }

    /// <summary>
    /// Handle the logic for continuous input such as the camera and movement.
    /// </summary>
    private void Update()
    {
        if (!isLocalPlayer) { return; }

        // Check to see if the player is on the ground or not.
        _isGrounded = Physics.Raycast(_playerTransform.position, Vector3.down, 1f) && _playerVelocity.y <= 0f;

        // Update the player's y-axis position to account for gravity
        _playerVelocity.y += _gravity * Time.deltaTime;
        _playerController.Move(_playerVelocity * Time.deltaTime);

        HandleLook();
        HandleMovement();
        HandleSprint();
    }

    /// <summary>
    /// Handle the player's camera and character rotation based on the player's input.
    /// </summary>
    private void HandleLook()
    {
        Vector2 lookInput = _cameraSensitivity * Time.deltaTime * _playerControls.Player.Look.ReadValue<Vector2>();

        // Since the axes in which we move our input device are opposite in Unity, we must swap them to ensure correct behavior.
        // For example, moving the mouse up and/or down corresponds to side-to-side mouse movement in Unity, so we need to adjust for this.
        _yRotation += lookInput.x;
        _xRotation = Mathf.Clamp(_xRotation - lookInput.y, -45f, 45f);

        // "Revert" _xRotation as the controls are inverted.
        // Without this, moving the controller down causes the camera to look up.
        _followTransform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);

        // Since we have not yet implemented character models, we will only rotate the entire character on the y-axis.
        // This logic may change to display the character looking upwards once a character model is implemented.
        _playerTransform.rotation = Quaternion.Euler(0, _yRotation, 0);

        // Check to see if we're looking at anything of importance.
        Physics.Raycast(_cameraTransform.position, _cameraTransform.forward * _interactableDistance, out _raycastHit);

        CmdLook(_playerTransform.rotation, _followTransform.rotation);
    }

    /// <summary>
    /// Handle the player's movement on the x-axis and z-axis.
    /// </summary>
    private void HandleMovement()
    {
        float totalSpeed = _isSprinting ? _sprintSpeed : _walkSpeed;

        // Ensure we always move relative to the direction we are looking at.
        Vector2 moveInput = _playerControls.Player.Movement.ReadValue<Vector2>();
        Vector3 moveDirection = _playerTransform.forward * moveInput.y + _playerTransform.right * moveInput.x;

        _playerVelocity.y += _gravity * Time.deltaTime;

        _playerController.Move(totalSpeed * Time.deltaTime * moveDirection);
        _playerController.Move(_playerVelocity * Time.deltaTime);
        CmdMovePlayer(_playerTransform.position);

        // Display the player's movement animation.
        // Since the player can move in four different directions, we have animations associated with each direction.
        // To prevent repeated if-else statements, we instead have a ternary operator to trigger the correct animation with its respective direction.
        _playerAnimator.SetFloat(_animatorMovementX, moveInput.x == 0 ? 0 : totalSpeed * Mathf.Sign(moveInput.x), 0.15f, Time.deltaTime); // 0.15 is an arbitrary dampening value to transition between different animations.
        _playerAnimator.SetFloat(_animatorMovementZ, moveInput.y == 0 ? 0 : totalSpeed * Mathf.Sign(moveInput.y), 0.15f, Time.deltaTime);
    }

    /// <summary>
    /// Set the _isSpriting variable to true if the player is moving forward while the sprint button is pressed.
    /// </summary>
    private void HandleSprint()
    {
        // Only sprint if the we are moving forward.
        Vector2 moveInput = _playerControls.Player.Movement.ReadValue<Vector2>();
        bool isForward = 0 < moveInput.y;

        _isSprinting = _canSprint && isForward && 0 < _staminaPoints.CurrentValue && _playerControls.Player.Sprint.IsPressed();

        // Decrease/Increase stamina based on whether or not we are currently sprinting.
        if (_isSprinting)
        {
            _staminaPoints.Decrease(_staminaCost * Time.deltaTime);
        }

        if (!_isSprinting && _staminaPoints.CurrentValue < _staminaPoints.BaseValue)
        {
            _staminaPoints.Increase(_staminaRestoration * Time.deltaTime);
        }

        // If the current stamina is zero, wait until the stamina bar fills up again.
        if (_staminaPoints.CurrentValue == 0f)
        {
            _canSprint = false;
            _isSprinting = false;

            StartCoroutine(SprintCooldown());
        }
    }

    /// <summary>
    /// Handle the player's input for attacking.
    /// </summary>
    /// <param name="context">The input callback context to subscribe/unsubscribe to using the Input System.</param>
    private void Attack(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) { return; }
        if (_currentWeapon != null)
        {
            _currentWeapon.Attack();
        }
    }

    /// <summary>
    /// Handle the player's input for the alternate attack on any given weapon.
    /// </summary>
    /// <param name="context">The input callback context to subscribe/unsubscribe to using the Input System.</param>
    private void AlternateAttack(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) { return; }
        if (_currentWeapon != null)
        {
            _currentWeapon.AlternateAttack();
        }
    }

    /// <summary>
    /// Handle the player's input for dropping an object from their inventory.
    /// </summary>
    /// <param name="context">The input callback context to subscribe/unsubscribe to using the Input System.</param>
    private void Drop(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) { return; }
        Drop();
    }

    /// <summary>
    /// Drop an item from the player's inventory into the game world.
    /// </summary>
    /// <param name="targetPosition">The target position to set the dropped item to.</param>
    private void Drop(Vector3? targetPosition = null)
    {
        GameObject droppedItem = _playerInventory.RemoveItem();
        
        if(droppedItem.GetComponent<MeleeWeapon>() != null || droppedItem.GetComponent<RangedWeapon>() != null){
            UnequipWeapon();
        }
        else if(droppedItem.GetComponent<Armor>() != null){
            armorManager.UnequipArmor(droppedItem);
        }

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

    /// <summary>
    /// Handle the player's input for interacting with interactable objects.
    /// </summary>
    /// <param name="context">The input callback context to subscribe/unsubscribe to using the Input System.</param>
    private void Interact(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) { return; }
        Collider hitCollider = _raycastHit.collider;
        GameObject hitGameObject = hitCollider.gameObject;

        if (hitCollider != null && hitGameObject.GetComponent<IInteractable>() != null)
        {
            // Check to see if we can add this item to our inventory.
            if (hitGameObject.GetComponent<IInventoryItem>() != null)
            {
                if(hitGameObject.transform.parent == null){
                    if (_playerInventory.HasItem())
                    {
                        Drop(hitGameObject.transform.position);
                    }

                    if (hitGameObject.TryGetComponent(out Weapon weapon))
                    {
                        CmdEquipWeapon(weapon);
                        _playerInventory.AddItem(hitGameObject);
                        CmdInteract(hitGameObject);
                    }
                    else if (hitCollider != null && hitCollider.TryGetComponent(out Armor armor))
                    {
                        armorManager.CmdEquipArmor(armor.gameObject);
                        _playerInventory.AddItem(hitGameObject);
                        CmdInteract(hitGameObject);
                    }
                    else
                    {
                        Debug.Log($"{hitGameObject.name} is neither a weapon nor armor.");
                    }                    
                }

            }
        }
    }

    /// <summary>
    /// Handle the player's input for jumping.
    /// </summary>
    /// <param name="context">The input callback context to subscribe/unsubscribe to using the Input System.</param>
    private void Jump(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) { return; }
        if (_isGrounded)
        {
            _playerVelocity.y = (float)Math.Sqrt(-2f * _gravity * _jumpHeight);
        }
    }

    /// <summary>
    /// Disable the PlayerControls actions and action maps when the component is disabled.
    /// </summary>
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

    /// <summary>
    /// Handles the cooldown period after sprinting.
    /// </summary>
    /// <returns>An IEnumerator for coroutine execution.</returns>
    private IEnumerator SprintCooldown()
    {
        yield return new WaitForSeconds(_staminaCooldown);
        _canSprint = true;
    }

    /// <summary>
    /// Equips a weapon to the player's hand and synchronizes with the server
    /// </summary>
    /// <param name="weapon">The weapon to equip</param>
    [Command]
    public void CmdEquipWeapon(Weapon weapon)
    {

        // Set the new weapon
        _currentWeapon = weapon;

        // Assign authority to the new weapon
        NetworkIdentity weaponIdentity = weapon.GetComponent<NetworkIdentity>();
        if (weaponIdentity != null)
        {
            weaponIdentity.AssignClientAuthority(connectionToClient);
        }

        _currentWeapon.Interact(playerHand);

        // Make sure the weapon is active
        weapon.gameObject.SetActive(true);

        // Make sure the Rigidbody is set to kinematic to avoid physics issues while holding it
        Rigidbody weaponRb = weapon.GetComponent<Rigidbody>();
        if (weaponRb != null)
        {
            weaponRb.isKinematic = true; // Disable physics on the weapon while it is being held
        }

        RpcEquipWeapon(weapon);
    }

    /// <summary>
    /// Unequips the currently equipped weapon and synchronizes with the server
    /// </summary>
    
    public void UnequipWeapon()
    {
        if (_currentWeapon != null)
        {
            _currentWeapon.CmdUnequip();
            _currentWeapon = null;
        }
    }

    /// <summary>
    /// Server code for updating player's movement, as of right now the client calculates 
    /// the positions and gives it to the server. Not very safe if cheater that manipulate 
    /// the calculation if we care about security.  
    /// </summary>
    /// <param name="position">Position of the player </param>
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
    /// <param name="rotationPlayer">Player rotation</param>
    /// <param name="rotationFollow">Follow camera rotation</param>
    [Command]
    private void CmdLook(Quaternion rotationPlayer, Quaternion rotationFollow){
        _playerTransform.rotation = rotationPlayer;
        _followTransform.rotation = rotationFollow;

        // Propagates the changes to all client
        RpcUpdatePlayerLook(_playerTransform.rotation, _followTransform.rotation);
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
            interactableObj.Interact(gameObject);
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
        if (isLocalPlayer) { return; }
        _playerTransform.position = position;
    }

    /// <summary>
    /// Update player rotation to all clients
    /// </summary>
    /// <param name="rotationPlayer">Player rotation</param>
    /// <param name="rotationFollow">Follow Camera rotation</param>
    [ClientRpc]
    private void RpcUpdatePlayerLook(Quaternion rotationPlayer, Quaternion rotationFollow){
        if (isLocalPlayer) { return; }
        _playerTransform.rotation = rotationPlayer;
        _followTransform.rotation = rotationFollow;
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
            interactableObj.Interact(gameObject);
        }
    }

    /// <summary>
    /// Updates the equipped weapon on all clients
    /// </summary>
    /// <param name="weapon">The weapon being equipped</param>
    [ClientRpc]
    private void RpcEquipWeapon(Weapon weapon)
    {
        // Set the current weapon on all clients
        _currentWeapon = weapon;
    }
}