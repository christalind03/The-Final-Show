using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryItem : MonoBehaviour
{
    private Weapon weapon;

    private void Awake()
    {
        // Attempt to find a Weapon component on this GameObject
        weapon = GetComponent<Weapon>();
        if (weapon == null)
        {
            Debug.LogWarning($"No Weapon component found on {gameObject.name}");
        }
    }

    public void Equip()
    {
        if (weapon != null)
        {
            weapon.Equip();
        }
    }

    public void Unequip()
    {
        if (weapon != null)
        {
            weapon.Unequip();
        }
    }

    public void Attack()
    {
        if (weapon != null)
        {
            weapon.Attack();
        }
    }

    public void AlternateAttack()
    {
        if (weapon != null)
        {
            weapon.AlternateAttack();
        }
    }
}