using Mirror;
using UnityEngine;

public class EnemyHealth : AbstractHealth
{
    // TODO: Document
    protected override void OnBaseHealth(float previousValue, float currentValue)
    {
        Debug.Log($"[EnemyHealth] {gameObject.name} base health changed from {previousValue} to {currentValue}");
    }

    // TODO: Document
    protected override void OnCurrentHealth(float previousValue, float currentValue)
    {
        Debug.Log($"[EnemyHealth] {gameObject.name} current health changed from {previousValue}/{_baseValue} to {currentValue}/{_baseValue}");
    }

    // TODO: Document
    [Server]
    protected override void TriggerDeath()
    {
        Destroy(gameObject);
    }
}
