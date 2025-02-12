using System.Collections.Generic;
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
    private Dictionary<string, Sprite> _keyDictionary;

    private void Start() {  
        _keyDictionary = new Dictionary<string, Sprite>();
        for (char c = 'A'; c <= 'Z'; c++)
        {
            _keyDictionary[c.ToString()] = Resources.Load<Sprite>($"Sprites/Keys/Letters/keyboard_{c.ToString().ToLower()}");
        }

        _infoText.text = message;
        _interactImage.sprite = _keyDictionary.GetValueOrDefault(InputControlPath.ToHumanReadableString(Interact.action.bindings[0].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice));
    }
    
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
