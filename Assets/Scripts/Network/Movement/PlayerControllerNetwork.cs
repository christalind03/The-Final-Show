using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Mirror;


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
        if(!isLocalPlayer) {return;}
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

        CmdLook(_playerTransform.rotation, _followTransform.rotation);
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
        if(!isLocalPlayer) {return;}
        Debug.Log("Attack");
    }

    private void AlternateAttack(InputAction.CallbackContext context)
    {
        if(!isLocalPlayer) {return;}
        Debug.Log("Alternate Attack");
    }

    private void Drop(InputAction.CallbackContext context)
    {
        if(!isLocalPlayer) {return;}
        Drop();
    }

    private void Drop(Vector3? targetPosition = null)
    {
        GameObject droppedItem = _playerInventory.RemoveItem();

        if (_raycastHit.collider != null && droppedItem != null)
        {
            if(targetPosition.HasValue){
                CmdDrop(droppedItem, targetPosition.Value, true);
            }else{
                CmdDrop(droppedItem, new Vector3(0,0,0), false);
            }
        }
    }

    private void Interact(InputAction.CallbackContext context)
    {
        if(!isLocalPlayer) {return;}
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
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if(!isLocalPlayer) {return;}
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

    /// <summary>
    /// Server code for updating player's movement, as of right now the client calculates 
    /// the positions and gives it to the server. Not very safe if cheater that manipulate 
    /// the calculation if we care about security.  
    /// </summary>
    /// <param name="position">Position of the player</param>
    [Command]
    private void CmdMovePlayer(Vector3 position){
        _playerTransform.position = position;

        // Propagates the changes to all client
        RpcUpdatePlayerPosition(position);
    }

    /// <summary>
    /// Server code for updating player's rotation, similar to CmdMovePlayer but with rotation
    /// </summary>
    /// <param name="rotation">Player rotation</param>
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
    private void CmdDrop(GameObject dropItem, Vector3 targetPosition, bool hasValue){
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
    private void CmdInteract(GameObject hitObj){
        if(hitObj.TryGetComponent(out IInteractable interactableObj)){
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
    private void RpcUpdatePlayerLook(Quaternion rotationPlayer, Quaternion rotationFollow){
        if (isLocalPlayer) return;
        _playerTransform.rotation = rotationPlayer;
        _followTransform.rotation = rotationFollow;
    }

    /// <summary>
    /// Update the item that is being dropped in world view to all client
    /// </summary>
    /// <param name="dropItem">Item that is being dropped</param>
    /// <param name="dropPos">Location to be dropped at</param>
    [ClientRpc]
    private void RpcDrop(GameObject dropItem, Vector3 dropPos){
        // Since we are using a Host-Client network structure, there will be one player that acts as the server and they don't have to run the code again
        if(!isServer){
            dropItem.transform.position = dropPos;
            dropItem.SetActive(true);
        }
    }

    /// <summary>
    /// Update the object that is being interacted to all client
    /// </summary>
    /// <param name="hitObj">Item that is being interacted</param>
    [ClientRpc]
    private void RpcInteract(GameObject hitObj){
        // Same reasoning as RpcDrop with the added component of getting the component "IInteractable"
        if(!isServer && hitObj.TryGetComponent(out IInteractable interactableObj)){
            interactableObj.Interact();
        }
    }
}