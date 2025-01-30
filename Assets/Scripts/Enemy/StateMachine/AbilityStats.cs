using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AbilityStats
{
    [SerializeField] private float _abilityCooldown;
    [SerializeField] private float _abilityDuration;

    public float AbilityCooldown
    {
        get { return _abilityCooldown; }
        private set { _abilityCooldown = value; }
    }

    public float AbilityDuration
    {
        get { return _abilityDuration; }
        private set { _abilityDuration = value; }
    }
}
