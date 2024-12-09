using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Properties")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Dependencies")]
    [SerializeField] private ArmorManager armorManager;
    public bool IsInvulnerable { get; set; } = false;
    private void Start()
    {
        currentHealth = maxHealth;
    }

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

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        Destroy(gameObject);
    }
}
