using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using static Armor;

/// <summary>
/// Manages the equipping and unequipping of armor for an entity.
/// Also calculates damage reduction based on equipped armor.
/// </summary>
public class ArmorManager : NetworkBehaviour
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
    [Command]
    public void CmdEquipArmor(Armor armor)
    {
        if (armor == null)
        {
            Debug.LogError("Armor is null. Cannot equip.");
            return;
        }

        Transform equipPoint = null;

        // Determine the equip point based on the armor type
        switch (armor.Type)
        {
            case Armor.ArmorType.Head:
                equipPoint = headEquipPoint;
                if (_currentHeadArmor != null)
                {
                    _currentHeadArmor.CmdUnequip();
                }
                _currentHeadArmor = armor;
                break;

            case Armor.ArmorType.Chest:
                if (chestEquipPoint != null)
                {
                    equipPoint = chestEquipPoint;
                    if (_currentChestArmor != null)
                    {
                        _currentChestArmor.CmdUnequip();
                    }
                    _currentChestArmor = armor;
                }
                break;

            case Armor.ArmorType.Legs:
                if (legsEquipPoint != null)
                {
                    equipPoint = legsEquipPoint;
                    if (_currentLegArmor != null)
                    {
                        _currentLegArmor.CmdUnequip();
                    }
                    _currentLegArmor = armor;
                }
                break;

            default:
                Debug.LogError($"Unsupported armor type: {armor.Type}");
                return;
        }

        if (equipPoint == null)
        {
            Debug.LogWarning($"Equip point for {armor.Type} is null. Cannot visually attach armor.");
            return;
        }

        // Equip the armor visually and sync
        armor.transform.SetParent(equipPoint);
        armor.transform.localPosition = Vector3.zero;
        armor.transform.localRotation = Quaternion.identity;
        RpcEquipArmor(armor.Type);

        Debug.Log($"Equipped {armor.ArmorName} on {equipPoint.name}.");
    }

    [ClientRpc]
    private void RpcEquipArmor(Armor.ArmorType type)
    {
        Armor armorToEquip = null;
        Transform equipPoint = null;

        switch (type)
        {
            case Armor.ArmorType.Head:
                armorToEquip = _currentHeadArmor;
                equipPoint = headEquipPoint;
                break;

            case Armor.ArmorType.Chest:
                armorToEquip = _currentChestArmor;
                equipPoint = chestEquipPoint;
                break;

            case Armor.ArmorType.Legs:
                armorToEquip = _currentLegArmor;
                equipPoint = legsEquipPoint;
                break;
        }

        if (armorToEquip != null && equipPoint != null)
        {
            armorToEquip.transform.SetParent(equipPoint);
            armorToEquip.transform.localPosition = Vector3.zero;
            armorToEquip.transform.localRotation = Quaternion.identity;
            Debug.Log($"[Client] Equipped {armorToEquip.ArmorName} on {equipPoint.name}.");
        }
    }


    /// <summary>
    /// Unequips the armor of the specified type.
    /// </summary>
    /// <param name="armorType">The type of armor to unequip.</param>
    [Command]
    public void CmdUnequipArmor(Armor.ArmorType armorType)
    {
        switch (armorType)
        {
            case Armor.ArmorType.Head:
                if (_currentHeadArmor != null)
                {
                    _currentHeadArmor.CmdUnequip();
                    Debug.Log($"Unequipped {_currentHeadArmor.ArmorName} from the head.");
                    _currentHeadArmor = null;
                }
                break;

            case Armor.ArmorType.Chest:
                if (_currentChestArmor != null)
                {
                    _currentChestArmor.CmdUnequip();
                    Debug.Log($"Unequipped {_currentChestArmor.ArmorName} from the chest.");
                    _currentChestArmor = null;
                }
                break;

            case Armor.ArmorType.Legs:
                if (_currentLegArmor != null)
                {
                    _currentLegArmor.CmdUnequip();
                    Debug.Log($"Unequipped {_currentLegArmor.ArmorName} from the legs.");
                    _currentLegArmor = null;
                }
                break;

            default:
                Debug.LogError("Unsupported armor type.");
                break;
        }
    }

    [ClientRpc]
    private void RpcUnequipArmor(GameObject armorObject)
    {
        if (armorObject.TryGetComponent(out Armor armor))
        {
            armor.CmdUnequip();
            Debug.Log($"[Client] Unequipped {armor.ArmorName}.");
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