using Mirror;
using UnityEngine;

/// <summary>
/// Represents the health system for an enemy, inheriting from <see cref="AbstractHealth"/>.
/// Additionally handles the callbacks for base and current health changes to update the UI accordingly.
/// </summary>
public class EnemyHealth : AbstractHealth
{
    [Header("User Interface")]
    [SerializeField] private StatusBar _healthBar; // Since we may have multiple StatusBar components, we have to manually set the correct reference.
    [SerializeField] private GameObject _scriptPrefab;

    /// <summary>
    /// Called when <see cref="_baseValue"/> changes.
    /// Updates the enemy's health bar UI to reflect the current health value.
    /// </summary>
    /// <param name="previousValue">The previous base health value.</param>
    /// <param name="currentValue">The current base health value.</param>
    protected override void OnBaseHealth(float previousValue, float currentValue)
    {
        _healthBar.Refresh(currentValue, CurrentValue);
    }

    /// <summary>
    /// Called when <see cref="_currentValue"/> changes.
    /// Updates the enemy's health bar UI to reflect the current health value.
    /// </summary>
    /// <param name="previousValue">The previous current health value.</param>
    /// <param name="currentValue">The current current health value.</param>
    protected override void OnCurrentHealth(float previousValue, float currentValue)
    {
        _healthBar.Refresh(BaseValue, currentValue);
    }

    /// <summary>
    /// Handles the death of an enemy by spawning a script object and destroying the gameObject.
    /// </summary>
    [Server]
    protected override void TriggerDeath()
    {
        RpcSpawnScript();
        Destroy(gameObject);
    }

    [ClientRpc]
    protected void RpcSpawnScript()
    {
        if (_scriptPrefab != null)
        {
            Instantiate(_scriptPrefab, gameObject.transform.position, Quaternion.identity);
        }
        else
        {
            Debug.Log("No script prefab found");
        }
    }

}
