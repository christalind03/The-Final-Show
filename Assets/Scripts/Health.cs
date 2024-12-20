using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the health system for an entity in the game.
/// Handles damage, health reduction, and death behavior.
/// </summary>
public class Health : NetworkBehaviour
{
    [Header("Health Properties")]
    public float MaxHealth = 100f;

    [SyncVar(hook = nameof(OnHealthChanged))]
    private float currentHealth;

    //[Header("Dependencies")]
    //[SerializeField] private ArmorManager armorManager;
    public bool IsInvulnerable { get; set; } = false;

    public float CurrentHealth => currentHealth;

    public override void OnStartAuthority()
    {
        currentHealth = MaxHealth;

        base.OnStartAuthority();
    }

    public void AddHealth(float totalHealed)
    {
        currentHealth += totalHealed;
        currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);
    }

    /// <summary>
    /// Applies damage to the entity. Factors in armor defense.
    /// </summary>
    [Command(requiresAuthority = false)]
    public void CmdRemoveHealth(float totalDamage)
    {
        if (IsInvulnerable) return;

        currentHealth -= totalDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth); // Helps to ensure health stays in valid bounds
        Debug.Log($"{gameObject.name} took {totalDamage} damage. Remaining health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Handles the death of the entity.
    /// </summary>
    [Server]
    private void Die()
    {
        if (gameObject.tag == "Player")
        {
            Spectate(); 
        }
        else
        {
            Destroy(gameObject);
        }
        
        Debug.Log($"{gameObject.name} has died.");
        RpcOnDeath();
    }

    /// <summary>
    /// Called on all clients when health changes.
    /// </summary>
    /// <param name="oldHealth">The old health value.</param>
    /// <param name="newHealth">The new health value.</param>
    private void OnHealthChanged(float oldHealth, float newHealth)
    {
        Debug.Log($"{gameObject.name} health changed: {oldHealth} -> {newHealth}");
        // We can add in our visual health updates like health bars here
    }

    /// <summary>
    /// In the case the object is a player, automatically switch cameras upon death.
    /// </summary>
    [ClientRpc]
    private void RpcOnDeath()
    {
        if (!isLocalPlayer || isServer) { return; }
        if (gameObject.tag == "Player") { Spectate(); }

        Debug.Log($"{gameObject.name} death broadcasted to all clients.");
    }

    // TODO: Document
    private void Spectate()
    {
        gameObject.transform.Find("Capsule").gameObject.layer = 0;
        CameraController cameraController = gameObject.GetComponent<CameraController>();
        cameraController.alive = false;
        cameraController.Spectate();
    }
}