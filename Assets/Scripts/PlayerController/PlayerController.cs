using Cinemachine;
using Mirror;
using Steamworks;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages a player's movement and interactions within a networked, multiplayer environment using the Mirror framework.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(CombatController))]
[RequireComponent(typeof(PlayerInventory))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerController : NetworkBehaviour
{
    [SyncVar] public string playerName = null;

    [Header("Camera Parameters")]
    [SerializeField] public float _cameraSensitivity;
    [SerializeField] private float _interactableDistance;

    [Header("Movement Parameters")]
    [SerializeField] private float _gravity;
    [SerializeField] private float _jumpHeight;
    [SerializeField] private float _aimSpeed;
    [SerializeField] private float _sprintSpeed;
    [SerializeField] private float _walkSpeed;

    [Header("Stamina Parameters")]
    [SerializeField] private float _staminaCost;
    [SerializeField] private float _staminaRestoration;
    [SerializeField] private float _staminaCooldown;

    [Header("Player References")]
    [SerializeField] private CinemachineVirtualCamera _aimCamera;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Transform _followTransform;
    [SerializeField] private Transform _playerTransform;

    private bool _isAiming;
    private bool _isGrounded;
    private bool _isSprinting;
    private bool _canJump;
    private bool _canSprint;

    private float _xRotation;
    private float _yRotation;
    private Vector3 _playerVelocity;
    public PlayerInput playerInput;
    
    private RaycastHit _raycastHit;
    private CombatController _combatController;
    private PlayerInventory _playerInventory;
    private PlayerStats _playerStats;
    private PlayerInterface _playerInterface;
    private SettingsMenu _settings;
    private ScoreBoard _scoreboard;

    private AudioManager _audioManager;
    private Animator _playerAnimator;
    private int _animatorIsAiming;
    private int _animatorIsJumping;
    private int _animatorMovementX;
    private int _animatorMovementZ;

    /// <summary>
    /// Configure the cursor settings and initialize a new instance of PlayerControls when the script is first loaded.
    /// </summary>
    public override void OnStartAuthority()
    {
        _audioManager = gameObject.GetComponent<AudioManager>();
        CameraController cameraController = gameObject.GetComponent<CameraController>();

        if (cameraController.alive)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        _canJump = true;
        _canSprint = true;

        playerInput = gameObject.GetComponent<PlayerInput>();
        _combatController = gameObject.GetComponent<CombatController>();
        _playerInventory = gameObject.GetComponent<PlayerInventory>();
        _playerStats = gameObject.GetComponent<PlayerStats>();
        _settings = gameObject.GetComponentInChildren<SettingsMenu>();
        _playerInterface = gameObject.GetComponent<PlayerInterface>();

        // To access the animator, we must retrieve the child gameObject that is rendering the player's mesh.
        // This should be the first child of the current gameObject, `BaseCharacter`
        _playerAnimator = transform.GetChild(0).GetComponent<Animator>();
        _animatorIsAiming = Animator.StringToHash("Is Aiming");
        _animatorIsJumping = Animator.StringToHash("Is Jumping");
        _animatorMovementX = Animator.StringToHash("Movement X");
        _animatorMovementZ = Animator.StringToHash("Movement Z");

        if (!Application.isEditor && SteamManager.Initialized)
        {
            playerName = SteamFriends.GetPersonaName();
            gameObject.name = playerName;
            CmdUpdateName(playerName);
        }
        else
        {
            CmdUpdateName(gameObject.name);
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _scoreboard = NetworkManager.FindObjectOfType<ScoreBoard>();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        EnableControls();
    }

    /// <summary>
    /// Enable the PlayerControls actions and action maps when the component is enabled.
    /// </summary>
    private void EnableControls()
    {
        // _playerControls.Enable();
        playerInput.actions.Enable();

        playerInput.actions["Attack"].performed += Attack;
        playerInput.actions["Drop"].performed += Drop;
        playerInput.actions["Interact"].performed += Interact;
        playerInput.actions["Jump"].performed += Jump;
        playerInput.actions["ScoreBoard"].performed += ScoreBoard;
        playerInput.actions["Settings"].performed += Settings;

        // Subscribe to inventory slot selection for all slots.
        playerInput.actions["Cycle Slots"].performed += _playerInventory.SelectSlot;
        InputActionMap inventoryActions = playerInput.actions.FindActionMap("Inventory");

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
        if (!NetworkClient.ready) { return; }

        if (_characterController.enabled && playerInput != null)
        {
            // Check to see if the player is on the ground or not.
            _isGrounded = Physics.Raycast(_playerTransform.position, Vector3.down, 1f) && _playerVelocity.y <= 0f;

            // Update the player's y-axis position to account for gravity
            _playerVelocity.y += _gravity * Time.deltaTime;
            _characterController.Move(_playerVelocity * Time.deltaTime);

            HandleLook();
            HandleMovement();
            HandleSprint();
        }
    }

    /// <summary>
    /// Display boundaries of the player's melee weapon, if equipped, in the scene view.
    /// </summary>
    /// <remarks>
    /// This method is executed in the Unity Editor to provide visual aids for components in the scene.
    /// It uses a try-catch block due to <c>OnDrawGizmos</c> running during the editor time, and exceptions could interfere with the editor's functioning.
    /// </remarks>
    private void OnDrawGizmos()
    {
        try
        {
            InventoryItem inventoryItem = _playerInventory.GetItem();

            if (inventoryItem is MeleeWeapon meleeWeapon)
            {
                // Draw the overlapping sphere used to detect targets
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, meleeWeapon.AttackRange);

                // Draw lines to visually represent the boundaries of the attack cone
                Vector3 forwardLeft = Quaternion.Euler(0, -meleeWeapon.AttackAngle / 2, 0) * transform.forward * meleeWeapon.AttackRange;
                Vector3 forwardRight = Quaternion.Euler(0, meleeWeapon.AttackAngle / 2, 0) * transform.forward * meleeWeapon.AttackRange;

                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, transform.position + forwardLeft);
                Gizmos.DrawLine(transform.position, transform.position + forwardRight);
            }
        }
        catch (Exception) { }
    }

    /// <summary>
    /// Handle the player's camera and character rotation based on the player's input.
    /// </summary>
    private void HandleLook()
    {
        Vector2 lookInput = _cameraSensitivity * Time.deltaTime * playerInput.actions["Look"].ReadValue<Vector2>();

        // Check to see if the player is aiming (via AlternateAttack) and if the current item in the player's inventory is a RangedWeapon.
        // If both conditions are true, activate the aim camera; otherwise, deactivate it.
        _isAiming = playerInput.actions["Alternate Attack"].IsPressed();

        if (_isAiming && _playerInventory.GetItem() is RangedWeapon)
        {
            // Need to somehow handle attacking while aiming
            _aimCamera.Priority = 2;
            _playerAnimator.SetBool(_animatorIsAiming, true);
        }
        else
        {
            _aimCamera.Priority = 0;
            _playerAnimator.SetBool(_animatorIsAiming, false);
        }


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
        Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out _raycastHit, _interactableDistance);
        CmdLook(_followTransform.rotation, _aimCamera.Priority);
    }

    /// <summary>
    /// Handle the player's movement on the x-axis and z-axis.
    /// </summary>
    private void HandleMovement()
    {
        float totalSpeed = _isAiming
            ? _aimSpeed
            : _isSprinting
                ? _sprintSpeed
                : _walkSpeed;

        // Ensure we always move relative to the direction we are looking at.
        Vector2 moveInput = playerInput.actions["Movement"].ReadValue<Vector2>();
        Vector3 moveDirection = _playerTransform.forward * moveInput.y + _playerTransform.right * moveInput.x;

        _playerVelocity.y += _gravity * Time.deltaTime;

        _characterController.Move(totalSpeed * Time.deltaTime * moveDirection);
        _characterController.Move(_playerVelocity * Time.deltaTime);

        // Display the player's movement animation.
        // Since the player can move in four different directions, we have animations associated with each direction.
        // To prevent repeated if-else statements, we instead have a ternary operator to trigger the correct animation with its respective direction.
        _playerAnimator.SetFloat(_animatorMovementX, moveInput.x == 0 ? 0 : totalSpeed * Mathf.Sign(moveInput.x), 0.1f, Time.deltaTime); // 0.1f is an arbitrary dampening value to transition between different animations.
        _playerAnimator.SetFloat(_animatorMovementZ, moveInput.y == 0 ? 0 : totalSpeed * Mathf.Sign(moveInput.y), 0.1f, Time.deltaTime);

        // Update the audio clip being played based on the player's movement.
        if (moveInput == Vector2.zero)
        {
            _audioManager.CmdStop("Footsteps_Running");
            _audioManager.CmdStop("Footsteps_Walking");
        }
        else
        {
            if (_isSprinting)
            {
                _audioManager.CmdStop("Footsteps_Walking");
                _audioManager.CmdPlay("Footsteps_Running");
            }
            else
            {
                _audioManager.CmdStop("Footsteps_Running");
                _audioManager.CmdPlay("Footsteps_Walking");
            }
        }
    }

    /// <summary>
    /// Set the _isSpriting variable to true if the player is moving forward while the sprint button is pressed.
    /// </summary>
    private void HandleSprint()
    {
        Stat playerStamina = _playerStats.Stamina;

        // Only sprint if the we are moving forward.
        Vector2 moveInput = playerInput.actions["Movement"].ReadValue<Vector2>();
        bool isForward = 0 < moveInput.y;

        _isSprinting = _canSprint && isForward && 0 < playerStamina.CurrentValue && playerInput.actions["Sprint"].IsPressed();

        // Decrease/Increase stamina based on whether or not we are currently sprinting.
        if (_isSprinting)
        {
            playerStamina.Decrease(_staminaCost * Time.deltaTime);
        }

        if (!_isSprinting && playerStamina.CurrentValue < playerStamina.BaseValue)
        {
            playerStamina.Increase(_staminaRestoration * Time.deltaTime);
        }

        // If the current stamina is zero, wait until the stamina bar fills up again.
        if (playerStamina.CurrentValue == 0f)
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

        InventoryItem inventoryItem = _playerInventory.GetItem();

        if (inventoryItem != null && inventoryItem is Weapon playerWeapon)
        {
            _combatController.Attack(playerWeapon);
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
        if (!isLocalPlayer) { return; }

        InventoryItem droppedItem = _playerInventory.RemoveItem();

        if (droppedItem != null)
        {
            // 3 is an arbitrary value representing how far away the dropped item should be from the player.
            Vector3 droppedPosition = targetPosition ?? transform.position + transform.forward + transform.up * 1.5f;

            CmdDrop(droppedItem, droppedPosition);
        }
    }

    /// <summary>
    /// Handle the player's input for interacting with interactable objects.
    /// </summary>
    /// <param name="context">The input callback context to subscribe/unsubscribe to using the Input System.</param>
    private void Interact(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) { return; }

        GameObject hitObject = _raycastHit.collider?.gameObject;

        if (hitObject != null)
        {
            if (hitObject.TryGetComponent(out SkinnedMeshRenderer skinnedMeshRenderer))
            {
                CmdInteract(hitObject);
            }
            else
            {
                CmdInteract(hitObject.transform.root.gameObject);
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
        if (_canJump)
        {
            StartCoroutine(Jump());
        }
    }

    /// <summary>
    /// Adjust the player's current height and display animations.
    /// </summary>
    /// <remarks>
    /// In the case any jump parameters within the PlayerController script or in the animation controller are changed, this function may need to be updated.
    /// This is not the most effiicent or optimal solution in regards to timing an animation to sync with the action itself, but this will do for now.
    /// </remarks>
    /// <returns>An <c>IEnumerator</c> for coroutine execution.</returns>
    private IEnumerator Jump()
    {
        _canJump = false;
        _playerAnimator.SetBool(_animatorIsJumping, true);

        yield return new WaitForSeconds(0.45f); // Arbitrary number to account for the wind-up time on the jump animation.

        if (_isGrounded)
        {
            _playerVelocity.y = (float)Math.Sqrt(-2f * _gravity * _jumpHeight);
        }

        yield return new WaitForSeconds(0.65f); // Arbitrary number to account for the air time on the jump animation.

        _playerAnimator.SetBool(_animatorIsJumping, false);
        _canJump = true;
    }

    /// <summary>
    /// When the player press Tab, the player list will open/close based on the current display status
    /// </summary>
    private void ScoreBoard(InputAction.CallbackContext context)
    {
        _playerInterface.RefreshScoreBoard();
        _playerInterface.ToggleScoreBoardVisibility();
    }

    private void Settings(InputAction.CallbackContext context){
        // if (Application.isEditor) return;
        if(_settings.OpenMenu()){
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    /// <summary>
    /// When the client no longer has authority over this object, ensure the cursor is visible and disables the controls.
    /// </summary>
    private void OnDisable() 
    {
        if (!isLocalPlayer) { return; }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        DisableControls();
    }

    public override void OnStopAuthority()
    {
        if (!isLocalPlayer) { return; }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        DisableControls();
    }

    /// <summary>
    /// Disable the PlayerControls actions and action maps when the component is disabled.
    /// </summary>
    private void DisableControls()
    {
        playerInput.actions.Disable();

        playerInput.actions["Attack"].performed -= Attack;
        playerInput.actions["Drop"].performed -= Drop;
        playerInput.actions["Interact"].performed -= Interact;
        playerInput.actions["Jump"].performed -= Jump;
        playerInput.actions["ScoreBoard"].performed -= ScoreBoard;
        playerInput.actions["Settings"].performed -= Settings;

        playerInput.actions["Cycle Slots"].performed -= _playerInventory.SelectSlot;
        InputActionMap inventoryActions = playerInput.actions.FindActionMap("Inventory");

        foreach (InputAction inventorySlot in inventoryActions)
        {
            inventorySlot.performed -= _playerInventory.SelectSlot;
        }
    }

    /// <summary>
    /// Handles the cooldown period after sprinting.
    /// </summary>
    /// <returns>An <c>IEnumerator</c> for coroutine execution.</returns>
    private IEnumerator SprintCooldown()
    {
        yield return new WaitForSeconds(_staminaCooldown);
        _canSprint = true;
    }

    /// <summary>
    /// Server code for updating player's rotation
    /// </summary>
    /// <param name="rotationPlayer">Player rotation</param>
    /// <param name="rotationFollow">Follow camera rotation</param>
    [Command(requiresAuthority = false)]
    private void CmdLook(Quaternion rotationFollow, int aimCameraPriority)
    {
        _followTransform.rotation = rotationFollow;
        _aimCamera.Priority = aimCameraPriority;

        // Propagates the changes to all clients
        RpcUpdatePlayerLook(_followTransform.rotation, aimCameraPriority);
    }

    /// <summary>
    /// Instantiate a gameObject in world space containing the logic to display an <c>InventoryItem</c>.
    /// </summary>
    /// <param name="droppedItem">The item that is being dropped</param>
    /// <param name="droppedPosition">Position that the item should be dropped at</param>
    [Command]
    private void CmdDrop(InventoryItem droppedItem, Vector3 droppedPosition)
    {
        float curRotation = UnityEngine.Random.Range(0f, 360f);
        GameObject droppedObject = Instantiate(Resources.Load<GameObject>("Items/Interactable Inventory Item"), droppedPosition, Quaternion.AngleAxis(curRotation, Vector3.up));
        droppedObject.GetComponent<InteractableInventoryItem>().InventoryItem = droppedItem;
        droppedObject.GetComponent<InteractableInventoryItem>().Drop(gameObject.transform);

        NetworkServer.Spawn(droppedObject);

        // Ensure the inventory item is rendered across all clients.
        RpcDrop(droppedItem, droppedObject);
    }

    /// <summary>
    /// Interact with the GameObject if it has IInteractable script
    /// </summary>
    /// <param name = "hitObject">The object that is being interacted</param>
    [Command]
    private void CmdInteract(GameObject hitObject)
    {
        if (hitObject != null && hitObject.TryGetComponent(out IInteractable interactableComponent))
        {
            interactableComponent.Interact(gameObject);
        }

        // Propagates the changes to all clients
        RpcInteract(hitObject);
    }

    /// <summary>
    /// Update player rotation to all clients
    /// </summary>
    /// <param name="rotationPlayer">Player rotation</param>
    /// <param name="rotationFollow">Follow Camera rotation</param>
    [ClientRpc]
    private void RpcUpdatePlayerLook(Quaternion rotationFollow, int aimCamPrio)
    {
        if (isLocalPlayer) { return; }
        _followTransform.rotation = rotationFollow;
        _aimCamera.Priority = aimCamPrio;
    }

    /// <summary>
    /// Set the <c>InventoryItem</c> to be displayed within world space.
    /// </summary>
    /// <param name="droppedItem">Item that is being dropped</param>
    /// <param name="droppedObject">GameObject containing the logic to display the <c>InventoryItem</c></param>
    [ClientRpc]
    private void RpcDrop(InventoryItem droppedItem, GameObject droppedObject)
    {
        // Since we are using a Host-Client network structure, there will be one player that acts as the server and they don't have to run the code again
        if (!isServer)
        {
            droppedObject.GetComponent<InteractableInventoryItem>().InventoryItem = droppedItem;
        }
    }

    /// <summary>
    /// Update the object that is being interacted to all clients
    /// </summary>
    /// <param name="hitObject">Item that is being interacted</param>
    [ClientRpc]
    private void RpcInteract(GameObject hitObject)
    {
        // Same reasoning as RpcDrop with the added component of getting the component "IInteractable"
        if (!isServer && hitObject != null && hitObject.TryGetComponent(out IInteractable interactableComponent))
        {
            interactableComponent.Interact(gameObject);
        }
    }

    /// <summary>
    /// Updates the player's name on the server side
    /// </summary>
    /// <param name="newName">player's name</param>
    [Command]
    private void CmdUpdateName(string newName)
    {
        if (_scoreboard == null) { return; }
        if (!Application.isEditor)
        {
            gameObject.name = newName;
        }

        _scoreboard.nameReady = true;
    }

    /// <summary>
    /// Allow other GameObjects on the server to tell clients to move their character around
    /// </summary>
    /// <param name="vect">Vector to move the player by</param>
    [ClientRpc]
    public void RpcExternalMove(Vector3 vect)
    {
        if (!isLocalPlayer) { return; } // only move the local player
        _characterController.Move(vect);
    }
}