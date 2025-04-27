using Mirror;
using System.Collections.Generic;
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

    private List<uint> _sourceNetids = new List<uint>();
    private uint _latestSource;
    private EnemyStateMachine _enemyController;

    protected override void Start()
    {
        base.Start();
        _enemyController = GetComponent<EnemyStateMachine>();
    }

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
    /// Decreases the current value of the stat by the specified amount and adds the player who dealt damage to the list of sourceNetids.
    /// If the current value is below zero, then call <c>TriggerDeath()</c>.
    /// </summary>
    /// <param name="decreaseValue">The amount to decrease the current value by.</param>
    [Command(requiresAuthority = false)]
    public void CmdDamageSource(float decreaseValue, uint sourceId)
    {
        if (TryGetComponent(out PlayerStats playerStats))
        {
            float reducedDamage = Mathf.Max(decreaseValue - playerStats.Defense.BaseValue, 0);
            CurrentValue -= reducedDamage;
        }
        else
        {
            CurrentValue -= decreaseValue;
        }
        // Add the player's netid if it isn't already in the list
        if (!_sourceNetids.Contains(sourceId))
        {
            _sourceNetids.Add(sourceId);
        }
        _latestSource = sourceId;
        if (CurrentValue <= 0f) { TriggerDeath(); }
        // If the enemy has no target, make it aggro on the player who did damage
        _enemyController.ExternalAggro(sourceId);
    }

    /// <summary>
    /// Handles the death of an enemy by awarding assists and kills, spawning script objects, and destroying the gameObject.
    /// </summary>
    [Server]
    protected override void TriggerDeath()
    {
        ScoreBoard scoreboard = NetworkManager.FindObjectOfType<ScoreBoard>();
        _sourceNetids.Remove(_latestSource);
        foreach (var source in _sourceNetids)
        {
            scoreboard.CmdUpdateAssistData(source, 1, 1);
        }
        scoreboard.CmdUpdateKillData(_latestSource, 1, 1);
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

    /// <summary>
    /// Apply a knockback effect to the enemy via the enemy controller.
    /// </summary>
    /// <param name="vect">Scaled vector to move enemy along</param>
    [Command(requiresAuthority = false)]
    public override void ApplyKnockback(Vector3 vect)
    {
        _enemyController.ExternalKnockback(vect);
    }
}
