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

    [Header("Manuscript Settings")]
    [SerializeField] private GameObject _scriptPrefab;
    [Min(1), SerializeField] private int _minScripts = 1;
    [Min(1), SerializeField] private int _maxScripts = 1;

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
        SpawnScripts();
        Destroy(gameObject);
    }

    /// <summary>
    /// Spawns a random number of scripts with evenly distributed rotations about the Y-axis
    /// </summary>
    protected void SpawnScripts()
    {
        int numScripts = Random.Range(_minScripts, _maxScripts + 1);
        float curRotation = Random.Range(0f, 360f);
        float degIncrement = 360f / numScripts;
        for (int i = 0; i < numScripts; i++)
        {
            // Instantiates the object with the current rotation and one unit up from the enemy's position
            GameObject script = Instantiate(_scriptPrefab, gameObject.transform.position + Vector3.up, Quaternion.AngleAxis(curRotation, Vector3.up));
            NetworkServer.Spawn(script);
            curRotation += degIncrement;
        }
    }
}
