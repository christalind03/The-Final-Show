using Mirror;
using UnityEngine;

public class EnemyHealth : AbstractHealth
{
    [Header("User Interface")]
    [SerializeField] private StatusBar _healthBar;

    // TODO: Document
    protected override void OnBaseHealth(float previousValue, float currentValue)
    {
        _healthBar.Refresh(currentValue, CurrentValue);
    }

    // TODO: Document
    protected override void OnCurrentHealth(float previousValue, float currentValue)
    {
        _healthBar.Refresh(BaseValue, currentValue);
    }

    // TODO: Document
    [Server]
    protected override void TriggerDeath()
    {
        Destroy(gameObject);
    }
}
