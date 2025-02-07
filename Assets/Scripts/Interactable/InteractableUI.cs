using UnityEngine;

public class InteractableUI : AbstractBillboard
{
    [Header("Message to display")]
    [SerializeField] private string message;
    [SerializeField] private float _interactableDistance;
    private GameObject _player;
    private Canvas canvas;

    private void Start() {
        canvas = GetComponentInParent<Canvas>();    
    }
    
    protected override void LateUpdate()
    {
        base.LateUpdate();

        if(_player == null && NetworkUtils.RetrieveLocalPlayer() != null){
            _player = NetworkUtils.RetrieveLocalPlayer();
        }
        
        if(_player == null) return;

        if(Vector3.Distance(transform.position, _player.transform.position) > _interactableDistance){
            canvas.enabled = false;
        }else{
            canvas.enabled = true;
        }
    }
}
