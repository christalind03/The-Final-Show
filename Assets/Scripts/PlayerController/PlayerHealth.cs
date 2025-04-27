using Mirror;
using UnityEngine;

/// <summary>
/// Represents the health system for a player, inheriting from <see cref="AbstractHealth"/>.
/// Additionally handles the callbacks for base and current health changes to update the UI accordingly.
/// </summary>
public class PlayerHealth : AbstractHealth
{
    private PlayerInterface _playerInterface;

    /// <summary>
    /// Initializes the player's interface to allow for the updating of the health interface.
    /// </summary>
    public override void OnStartAuthority()
    {
        _playerInterface = gameObject.GetComponent<PlayerInterface>();

        base.OnStartAuthority();
    }

    /// <summary>
    /// Called when <see cref="_baseValue"/> changes.
    /// Updates the players's health bar UI to reflect the current health value.
    /// </summary>
    /// <param name="previousValue">The previous base health value.</param>
    /// <param name="currentValue">The current base health value.</param>
    protected override void OnBaseHealth(float previousValue, float currentValue)
    {
        _playerInterface?.RefreshHealth(currentValue, _currentValue);
    }

    /// <summary>
    /// Called when <see cref="_currentValue"/> changes.
    /// Updates the players's health bar UI to reflect the current health value.
    /// </summary>
    /// <param name="previousValue">The previous current health value.</param>
    /// <param name="currentValue">The current current health value.</param>
    protected override void OnCurrentHealth(float previousValue, float currentValue)
    {
        _playerInterface?.RefreshHealth(_baseValue, currentValue);
    }

    /// <summary>
    /// Called on the server to trigger the death of a player.
    /// Additionally initiates the player to go into spectating mode and updates other clients.
    /// </summary>
    [Server]
    protected override void TriggerDeath()
    {
        ScoreBoard scoreboard = NetworkManager.FindObjectOfType<ScoreBoard>();
        PlayerVisibility playerVis = GetComponent<PlayerVisibility>();
        playerVis.CmdToggleVisibility(false);
        scoreboard.CmdUpdateDeathData(netId, 1, 1);
        Spectate();
        RpcTriggerDeath();
    }

    /// <summary>
    /// Initiates the local player to go into spectating mode.
    /// </summary>
    [ClientRpc]
    private void RpcTriggerDeath()
    {
        if (!isLocalPlayer || isServer) { return; }
        Spectate();
    }

    /// <summary>
    /// Updates the player's status into "Spectator" mode.
    /// </summary>
    private void Spectate()
    {
        gameObject.layer = 0;
        CameraController cameraController = gameObject.GetComponent<CameraController>();
        cameraController.alive = false;
        cameraController.Spectate();
    }

    /// <summary>
    /// Apply a knockback effect to the player via the player controller.
    /// </summary>
    /// <param name="vect">Scaled vector to move player along</param>
    [Command(requiresAuthority = false)]
    public override void ApplyKnockback(Vector3 vect)
    {
        // Due to the different implementation between enemy and player knockback, multiply the vector by 2 for a better feel
        gameObject.GetComponent<PlayerController>().RpcExternalKnockback(2 * vect); 
    }
}
