using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackStats
{
    [SerializeField] private float _attackCooldown = 1;
    [SerializeField] private float _attackDamage;

    public float AttackCooldown 
    { 
        get 
        { 
            return _attackCooldown;
        } 
        private set 
        { 
            _attackCooldown = value; 
        } 
    }

    public float AttackDamage
    {
        get
        {
            return _attackDamage;
        }
        private set
        {
            _attackDamage = value;
        }
    }
}
