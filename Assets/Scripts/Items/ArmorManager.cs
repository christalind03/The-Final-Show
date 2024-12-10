using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the equipping and unequipping of armor for an entity.
/// Also calculates damage reduction based on equipped armor.
/// </summary>
public class ArmorManager : MonoBehaviour
{
    [Header("Armor Equip Points")]
    [SerializeField] private Transform headEquipPoint;
    [SerializeField] private Transform chestEquipPoint;
    [SerializeField] private Transform legsEquipPoint;

    private Armor _currentHeadArmor;
    private Armor _currentChestArmor;
    private Armor _currentLegArmor;

    /// <summary>
    /// Equips the armor and unequips the currently equipped armor of the same type.
    /// </summary>
    /// <param name="armor">The armor to equip.</param>
    public void EquipArmor(Armor armor)
    {
        switch (armor.Type) // armor.Type references the ArmorType enum in Armor.cs
        {
            case Armor.ArmorType.Head:
                if (_currentHeadArmor != null)
                {
                    _currentHeadArmor.Unequip();
                }
                _currentHeadArmor = armor;
                armor.Equip(headEquipPoint);
                Debug.Log($"Equipped {armor.ArmorName} on the head.");
                break;

            case Armor.ArmorType.Chest:
                if (_currentChestArmor != null)
                {
                    _currentChestArmor.Unequip();
                }
                _currentChestArmor = armor;
                armor.Equip(chestEquipPoint);
                Debug.Log($"Equipped {armor.ArmorName} on the chest.");
                break;

            case Armor.ArmorType.Legs:
                if (_currentLegArmor != null)
                {
                    _currentLegArmor.Unequip();
                }
                _currentLegArmor = armor;
                armor.Equip(legsEquipPoint);
                Debug.Log($"Equipped {armor.ArmorName} on the legs.");
                break;

            default:
                Debug.LogError("Unsupported armor type.");
                break;
        }
    }

    /// <summary>
    /// Unequips the armor of the specified type.
    /// </summary>
    /// <param name="armorType">The type of armor to unequip.</param>
    public void UnequipArmor(Armor.ArmorType armorType)
    {
        switch (armorType)
        {
            case Armor.ArmorType.Head:
                if (_currentHeadArmor != null)
                {
                    _currentHeadArmor.Unequip();
                    Debug.Log($"Unequipped {_currentHeadArmor.ArmorName} from the head.");
                    _currentHeadArmor = null;
                }
                break;

            case Armor.ArmorType.Chest:
                if (_currentChestArmor != null)
                {
                    _currentChestArmor.Unequip();
                    Debug.Log($"Unequipped {_currentChestArmor.ArmorName} from the chest.");
                    _currentChestArmor = null;
                }
                break;

            case Armor.ArmorType.Legs:
                if (_currentLegArmor != null)
                {
                    _currentLegArmor.Unequip();
                    Debug.Log($"Unequipped {_currentLegArmor.ArmorName} from the legs.");
                    _currentLegArmor = null;
                }
                break;

            default:
                Debug.LogError("Unsupported armor type.");
                break;
        }
    }

    /// <summary>
    /// Modifies incoming damage based on the currently equipped armor.
    /// </summary>
    /// <param name="incomingDamage">The original damage value.</param>
    /// <returns>The reduced damage value.</returns>
    public float ApplyArmorDefense(float incomingDamage)
    {
        float reducedDamage = incomingDamage;

        if (_currentHeadArmor != null)
            reducedDamage = _currentHeadArmor.ModifyDamage(reducedDamage);
        if (_currentChestArmor != null)
            reducedDamage = _currentChestArmor.ModifyDamage(reducedDamage);
        if (_currentLegArmor != null)
            reducedDamage = _currentLegArmor.ModifyDamage(reducedDamage);

        return reducedDamage;
    }
}
