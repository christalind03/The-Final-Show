using Mirror;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    [SerializeField] private float _attack;
    [SerializeField] private float _defense;
    [SerializeField] private float _stamina;

    public Stat Attack;
    public Stat Defense;
    public Stat Stamina;

    public override void OnStartAuthority()
    {
        Attack = new Stat(_attack);
        Defense = new Stat(_defense);
        Stamina = new Stat(_stamina);

        base.OnStartAuthority();
    }
}
