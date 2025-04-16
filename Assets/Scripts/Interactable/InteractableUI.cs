using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InteractableUI : AbstractBillboard
{
    [Header("Message to display")]
    [SerializeField] private string message;

    [Header("References")]
    [SerializeField] private float _interactableDistance;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Text _infoText;
    [SerializeField] private Image _interactImage;
    [SerializeField] private InputActionReference Interact;
    private GameObject _player;
    private Camera _camera;
    private RaycastHit _raycastHit;
    private InteractableUISprites interactableSprite;

    /// <summary>
    /// Loads the different ui information that needs to be displayed
    /// </summary>
    private void Start() {  
        interactableSprite = NetworkManager.FindObjectOfType<InteractableUISprites>();
        _infoText.text = message;
        _interactImage.sprite = interactableSprite._keyDictionary.GetValueOrDefault(
            InputControlPath.ToHumanReadableString(
                Interact.action.bindings[0].effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice
            ));

        if (_interactImage.sprite == null)
        {
            _interactImage.sprite = Resources.Load<Sprite>("Sprites/Keys/Symbols/Keyboard_question");
        }
        InputSystem.onActionChange += OnActionChange;
    }

    /// <summary>
    /// Disable the callback
    /// </summary>
    private void OnDestroy(){
        InputSystem.onActionChange -= OnActionChange;
    }

    /// <summary>
    /// Updates the picture when rebind happens
    /// </summary>
    /// <param name="obj">callback obj</param>
    /// <param name="change">type of change</param>
    private void OnActionChange(object obj, InputActionChange change)
    {
        if (change == InputActionChange.BoundControlsChanged && obj is InputActionAsset asset){
            if (Interact.action.bindings.Count > 0){
                string path = InputControlPath.ToHumanReadableString(
                    Interact.action.bindings[0].effectivePath,
                    InputControlPath.HumanReadableStringOptions.OmitDevice
                );
                _interactImage.sprite = interactableSprite._keyDictionary.GetValueOrDefault(path);
                if (_interactImage.sprite == null)
                {
                    _interactImage.sprite = Resources.Load<Sprite>("Sprites/Keys/Symbols/Keyboard_question");
                }
            }
        }
    }
    
    /// <summary>
    /// Performs lateupdate from AbstractBillboard and also displays interact UI base on look direction and distance
    /// </summary>
    protected override void LateUpdate()
    {
        base.LateUpdate();

        if(_player == null && NetworkUtils.RetrieveLocalPlayer() != null){
            _player = NetworkUtils.RetrieveLocalPlayer();
            _camera = _player.GetComponentInChildren<Camera>();
        }
        
        if(_player == null || _camera == null) return;
        
        Physics.Raycast(_camera.transform.position, _camera.transform.forward, out _raycastHit, _interactableDistance);
        if(_raycastHit.collider != null && _raycastHit.collider.gameObject.GetComponentInChildren<InteractableUI>() != null){
            InteractableUI hitUI = _raycastHit.collider.gameObject.GetComponentInChildren<InteractableUI>();
            if(hitUI == this){
                canvas.enabled = true;
            }
        }else{
            canvas.enabled = false;
        }
    }
}
