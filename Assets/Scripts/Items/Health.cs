using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Represents the health system for an entity in the game.
/// Handles damage, health reduction, and death behavior.
/// </summary>
public class Health : NetworkBehaviour
{
    [Header("Health Properties")]
    public float maxHealth = 100f;

    [SyncVar(hook = nameof(OnHealthChanged))]
    private float currentHealth;

    [Header("Dependencies")]
    [SerializeField] private ArmorManager armorManager;
    public bool IsInvulnerable { get; set; } = false;
    private void Start()
    {
        currentHealth = maxHealth;
    }
    /// <summary>
    /// Applies damage to the entity. Factors in armor defense.
    /// </summary>
    [Server]
    public void TakeDamage(float damage)
    {
        if (IsInvulnerable) return;

        // Use ArmorManager to calculate reduced damage
        float reducedDamage = armorManager != null ? armorManager.ApplyArmorDefense(damage) : damage;

        currentHealth -= reducedDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Helps to ensure health stays in valid bounds
        Debug.Log($"{gameObject.name} took {damage} damage. Remaining health: {currentHealth}");

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
        Debug.Log($"{gameObject.name} has died.");
        Destroy(gameObject);
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
    /// Called on all clients when the entity dies.
    /// </summary>
    [ClientRpc]
    private void RpcOnDeath()
    {
        Debug.Log($"{gameObject.name} death broadcasted to all clients.");
    }
}