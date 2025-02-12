using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InteractableUI : AbstractBillboard
{
    [Header("Message to display")]
    [SerializeField] private string message;

    [Header("References")]
    [SerializeField] private float _interactableDistance;
    [SerializeField] private Text _infoText;
    [SerializeField] private Canvas canvas;
    private GameObject _player;
    private Camera _camera;
    private RaycastHit _raycastHit;

    private void Start() {  
        _infoText.text = message;
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
