using Mirror;
using UnityEngine;

/// <summary>
/// Manages a player's core stats, including attack, defense, and stamina.
/// </summary>
[RequireComponent(typeof(PlayerInterface))]
public class PlayerStats : NetworkBehaviour
{
    [Header("Default Values")]
    [SerializeField] private float _attack;
    [SerializeField] private float _defense;
    [SerializeField] private float _stamina;
    [SerializeField] private float _criticalStrikeChance;

    public Stat Attack;
    public Stat Defense;
    public Stat Stamina;
    public Stat CriticalStrikeChance;

    private PlayerInterface _playerInterface;

    /// <summary>
    /// Called when the player gains authority over this object.
    /// Initializes the player's stats with their respective base values.
    /// </summary>
    public override void OnStartAuthority()
    {
        Attack = new Stat(_attack);
        Defense = new Stat(_defense);
        Stamina = new Stat(_stamina);
        CriticalStrikeChance = new Stat(_criticalStrikeChance);

        _playerInterface = gameObject.GetComponent<PlayerInterface>();

        Attack.OnBaseChange += (float previousValue, float currentValue) => _playerInterface.RefreshAttack(currentValue);
        Defense.OnBaseChange += (float previousValue, float currentValue) => _playerInterface.RefreshDefense(currentValue);
        Stamina.OnBaseChange += (float previousValue, float currentValue) => RefreshStamina(true, previousValue, currentValue);
        Stamina.OnCurrentChange += (float previousValue, float currentValue) => RefreshStamina(false, previousValue, currentValue);
        CriticalStrikeChance.OnBaseChange += (float previousValue, float currentValue) => _playerInterface?.RefreshCriticalStrikeChance(currentValue);

        base.OnStartAuthority();
    }

    /// <summary>
    /// Refreshes the player's stamina interface based on the changes in stamina values.
    /// </summary>
    /// <param name="isBaseChange">Indicates whether the change is related to the base stamina value.</param>
    /// <param name="previousValue">The stamina value before the change.</param>
    /// <param name="currentValue">The stamina value after the change.</param>
    private void RefreshStamina(bool isBaseChange, float previousValue, float currentValue)
    {
        if (isBaseChange)
        {
            _playerInterface?.RefreshStamina(currentValue, Stamina.CurrentValue);
        }
        else
        {
            _playerInterface?.RefreshStamina(Stamina.BaseValue, currentValue);
        }
    }
}
