using Mirror;
using UnityEngine;

public class PlayerHealth : AbstractHealth
{
    [Header("Player References")]
    [SerializeField] private GameObject _playerBody;

    // TODO: Document
    protected override void OnBaseHealth(float previousValue, float currentValue)
    {
        Debug.Log($"[EntityHealth] {gameObject.name} base health changed from {previousValue} to {currentValue}");
    }

    // TODO: Document
    protected override void OnCurrentHealth(float previousValue, float currentValue)
    {
        Debug.Log($"[EntityHealth] {gameObject.name} current health changed from {previousValue}/{_baseValue} to {currentValue}/{_baseValue}");
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
