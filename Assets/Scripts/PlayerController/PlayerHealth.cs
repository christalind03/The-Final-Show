using Mirror;
using UnityEngine;

public class PlayerHealth : AbstractHealth
{
    [Header("Player References")]
    [SerializeField] private GameObject _playerBody;

    private PlayerInterface _playerInterface;

    // TODO: Document
    public override void OnStartAuthority()
    {
        _playerInterface = gameObject.GetComponent<PlayerInterface>();

        base.OnStartAuthority();
    }

    // TODO: Document
    protected override void OnBaseHealth(float previousValue, float currentValue)
    {
        _playerInterface?.RefreshHealth(_currentValue, currentValue);
    }

    // TODO: Document
    protected override void OnCurrentHealth(float previousValue, float currentValue)
    {
        _playerInterface?.RefreshHealth(currentValue, _baseValue);
    }

    // TODO: Document
    [Server]
    protected override void TriggerDeath()
    {
        Spectate();
        RpcTriggerDeath();
    }

    // TODO: Document
    [ClientRpc]
    private void RpcTriggerDeath()
    {
        if (!isLocalPlayer || isServer) { return; }
        Spectate();
    }

    // TODO: Document
    private void Spectate()
    {
        _playerBody.layer = 0;
        CameraController cameraController = gameObject.GetComponent<CameraController>();
        cameraController.alive = false;
        cameraController.Spectate();
    }
}
