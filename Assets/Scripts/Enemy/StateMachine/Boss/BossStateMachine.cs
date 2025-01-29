using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

[RequireComponent(typeof(FieldOfView), typeof(NavMeshAgent))]
public class BossStateMachine : EnemyStateMachine
{
    // has all attributes of EnemyStateMachine PLUS
    [Header("Ability Parameters")]
    [SerializeField] protected List<AbilityStats> _abilityStats;
    protected List<bool> _canUseAbility;
    protected bool _usingAbility;


    /// <summary>
    /// Calls base.Awake() then initializes the _canUseAbility list with true
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        _canUseAbility = new List<bool>();
        int numAbilities = _abilityStats.Count;
        for (int i = 0; i < numAbilities; i++)
        {
            _canUseAbility.Add(true);
        }
        _usingAbility = false;
    }

    /// <summary>
    /// Used to control how often the boss can use ability 0
    /// </summary>
    [Server]
    protected IEnumerator Ability0Cooldown()
    {
        yield return new WaitForSeconds(_abilityStats[0].AbilityCooldown);
        _canUseAbility[0] = true;
    }

    /// <summary>
    /// Used to control how long ability 0 lasts
    /// </summary>
    /// <returns></returns>
    [Server]
    protected IEnumerator Ability0Duration()
    {
        yield return new WaitForSeconds(_abilityStats[0].AbilityDuration);
        _usingAbility = false;
    }

}
