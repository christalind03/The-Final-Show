using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// Controls the player's actions, interactions, and movement within the game world.
/// </summary>
// Ensure we have a CharacterController component as it is required to move the player.
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
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

    [Header("Hand Transform")]
    public Transform playerHandTransform; // Reference for the player's hand position

    [Header("Armor Components")]
    [SerializeField] private ArmorManager armorManager;


    private bool _isGrounded;
    private bool _isSprinting;

    private bool _canSprint;
    private float _xRotation; // Keep track of the current rotation of the camera and player on the x-axis.
    private float _yRotation; // Keep track of the current rotation of the camera and player on the y-axis.
    private Vector3 _playerVelocity; // Keep track of the current position of the camera and player on the y-axis.
    private RaycastHit _raycastHit;
    private PlayerControls _playerControls;
    private PlayerInventory _playerInventory;
    private VisualElement _uiDocument;

    /// <summary>
    /// Configure the cursor settings and initialize a new instance of PlayerControls when the script is first loaded.
    /// </summary>
    private void Awake()
    {
        _canSprint = true;
        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked; // Keep the cursor locked to the center of the game view.

        _uiDocument = GetComponent<UIDocument>().rootVisualElement;

        _playerControls = new PlayerControls();
        _playerInventory = new PlayerInventory(_uiDocument);
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
        _xRotation = Mathf.Clamp(_xRotation - lookInput.y, -45f, 45f); // Prevent the player from rotating their head backwards.

        // "Revert" _xRotation as the controls are inverted.
        // Without this, moving the controller down causes the camera to look up.
        _followTransform.rotation = Quaternion.Euler(-_xRotation, _yRotation, 0);

        // Since we have not yet implemented character models, we will only rotate the entire character on the y-axis.
        // This logic may change to display the character looking upwards once a character model is implemented.
        _playerTransform.rotation = Quaternion.Euler(0, _yRotation, 0);

        // Check to see if we're looking at anything of importance.
        Physics.Raycast(_cameraTransform.position, _cameraTransform.forward * _interactableDistance, out _raycastHit);
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
        // Rewritten for weapon class
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
        // Rewritten for weapon class
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
        Drop();
    }

    /// <summary>
    /// Drop an item from the player's inventory into the game world.
    /// </summary>
    /// <param name="targetPosition">The target position to set the dropped item to.</param>
    private void Drop(Vector3? targetPosition = null)
    {
        GameObject droppedItem = _playerInventory.RemoveItem();

        if (_raycastHit.collider != null && droppedItem != null)
        {
            // Determine how far the dropped item should be from the player
            Vector3 dropOffset = targetPosition ?? transform.position + transform.forward * 3;

            droppedItem.transform.position = dropOffset;
            droppedItem.SetActive(true);
        }
    }

    /// <summary>
    /// Handle the player's input for interacting with interactable objects.
    /// </summary>
    /// <param name="context">The input callback context to subscribe/unsubscribe to using the Input System.</param>
    private void Interact(InputAction.CallbackContext context)
    {
        Collider hitCollider = _raycastHit.collider;
        GameObject hitGameObject = hitCollider.gameObject;

        if (hitCollider != null && hitGameObject.TryGetComponent(out IInteractable interactableObj))
        {
            interactableObj.Interact();

            // TODO: Before adding an item to the inventory, we should check whether or not this item is equippable
            if (_playerInventory.HasItem())
            {
                Drop(hitGameObject.transform.position);
            }

            _playerInventory.AddItem(hitCollider.gameObject);
        }

        // Checks if item is a weapon and equips it: we can take this out later
        if (hitGameObject.TryGetComponent(out Weapon weapon))
        {
            EquipWeapon(weapon);
            Debug.Log($"{weapon.WeaponName} equipped.");
        }
        else if (hitCollider != null && hitCollider.TryGetComponent(out Armor armor))
        {
            armorManager.EquipArmor(armor);
            Debug.Log($"{armor.ArmorName} equipped.");
        }
        else
        {
            Debug.Log($"{hitGameObject.name} is neither a weapon nor armor.");
        }
    }

    /// <summary>
    /// Handle the player's input for jumping.
    /// </summary>
    /// <param name="context">The input callback context to subscribe/unsubscribe to using the Input System.</param>
    private void Jump(InputAction.CallbackContext context)
    {
        if (_isGrounded)
        {
            // Physics equation to calculate the initial jump velocity for reaching a specific height.
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

        // Unsubscribe from inventory slot selection for all slots.
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
    /// Connects Weapon class, equips weapon and unequips previous weapon if need be, positions weapon in hand
    /// </summary>
    public void EquipWeapon(Weapon weapon)
    {
        if (_currentWeapon != null)
        {
            DropWeapon();
        }

        _currentWeapon = weapon;
        _currentWeapon.Equip();

        // Make sure the bow is active
        weapon.gameObject.SetActive(true);

        // Attach the weapon to the player's hand
        weapon.transform.SetParent(playerHandTransform);

        // Reset local position and rotation to fit in the hand
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
    }

    /// <summary>
    /// Connects Weapon class, unequips weapon
    /// </summary>
    public void UnequipWeapon()
    {
        if (_currentWeapon != null)
        {
            _currentWeapon.Unequip();
            _currentWeapon = null;
        }
    }
    /// <summary>
    /// Handles dropping a player's weapon in the ingame world.
    /// </summary>
    /// <param name="context"></param>
    private void DropCurrentWeapon(InputAction.CallbackContext context)
    {
        DropWeapon();
    }

    /// <summary>
    /// The logic for dropping a player's weapon in the ingame world.
    /// </summary>
    private void DropWeapon()
    {
        if (_currentWeapon == null)
        {
            Debug.Log("No weapon to drop.");
            return;
        }

        // Detach the weapon from the player's hand
        GameObject droppedWeapon = _currentWeapon.gameObject;
        _currentWeapon.transform.SetParent(null);

        // Enable physics on the dropped weapon
        Rigidbody weaponRb = _currentWeapon.GetComponent<Rigidbody>();
        if (weaponRb != null)
        {
            weaponRb.isKinematic = false; // Enable physics
            weaponRb.AddForce(transform.forward * 2f, ForceMode.Impulse); // Push the weapon slightly forward
        }

        // Position the dropped weapon
        droppedWeapon.transform.position = transform.position + transform.forward * 2f;

        // Log the dropped weapon
        Debug.Log($"{_currentWeapon.WeaponName} has been dropped.");

        // Clear the current weapon reference
        _currentWeapon = null;
    }

}