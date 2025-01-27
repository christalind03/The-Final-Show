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

}
