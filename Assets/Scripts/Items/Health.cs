using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the health system for an entity in the game.
/// Handles damage, health reduction, and death behavior.
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Health Properties")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Dependencies")]
    /// <summary>
    /// Reference to the <see cref="ArmorManager"/> to calculate damage reduction.
    /// </summary>
    [SerializeField] private ArmorManager armorManager;
    public bool IsInvulnerable { get; set; } = false;
    private void Start()
    {
        currentHealth = maxHealth;
    }
    /// <summary>
    /// Applies damage to the entity. Factors in armor defense.
    /// </summary>
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
    private void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        Destroy(gameObject);
    }
}
