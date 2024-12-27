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

    public Stat Attack;
    public Stat Defense;
    public Stat Stamina;

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

        _playerInterface = gameObject.GetComponent<PlayerInterface>();

        Attack.OnBaseChange += (float previousValue, float currentValue) => _playerInterface.RefreshAttack(currentValue);
        Defense.OnBaseChange += (float previousValue, float currentValue) => _playerInterface.RefreshDefense(currentValue);
        Stamina.OnBaseChange += (float previousValue, float currentValue) => RefreshStamina(true, previousValue, currentValue);
        Stamina.OnCurrentChange += (float previousValue, float currentValue) => RefreshStamina(false, previousValue, currentValue);

        base.OnStartAuthority();
    }

    // TODO: Document
    private void RefreshStamina(bool isBaseChange, float previousValue, float currentValue)
    {
        if (isBaseChange)
        {
            _playerInterface?.RefreshStamina(Stamina.CurrentValue, currentValue);
        }
        else
        {
            _playerInterface?.RefreshStamina(currentValue, Stamina.BaseValue);
        }
    }
}
