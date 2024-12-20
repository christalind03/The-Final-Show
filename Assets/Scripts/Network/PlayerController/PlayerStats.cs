using Mirror;
using UnityEngine;

/// <summary>
/// Manages a player's core stats, including attack, defense, and stamina.
/// </summary>
public class PlayerStats : NetworkBehaviour
{
    [SerializeField] private float _attack;
    [SerializeField] private float _defense;
    [SerializeField] private float _stamina;

    public Stat Attack;
    public Stat Defense;
    public Stat Stamina;

    /// <summary>
    /// Called when the player gains authority over this object.
    /// Initializes the player's stats with their respective base values.
    /// </summary>
    public override void OnStartAuthority()
    {
        Attack = new Stat(_attack);
        Defense = new Stat(_defense);
        Stamina = new Stat(_stamina);

        base.OnStartAuthority();
    }
}
