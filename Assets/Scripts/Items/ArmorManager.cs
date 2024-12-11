// ArmorManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
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

    [SyncVar(hook = nameof(OnHeadArmorChanged))]
    private GameObject currentHeadArmor;

    [SyncVar(hook = nameof(OnChestArmorChanged))]
    private GameObject currentChestArmor;

    [SyncVar(hook = nameof(OnLegArmorChanged))]
    private GameObject currentLegArmor;

    /// <summary>
    /// Equips the armor and unequips the currently equipped armor of the same type
    /// </summary>
    /// <param name="armor">The armor to equip</param>
    [Command]
    public void CmdEquipArmor(GameObject armorObject)
    {
        if (armorObject == null) return;

        Armor armor = armorObject.GetComponent<Armor>();
        if (armor == null) return;

        Transform equipPoint = null;
        switch (armor.Type)
        {
            case ArmorType.Head:
                if (currentHeadArmor != null)
                {
                    RpcUnequipArmor(currentHeadArmor);
                }
                currentHeadArmor = armorObject;
                equipPoint = headEquipPoint;
                break;
            case ArmorType.Chest:
                if (currentChestArmor != null)
                {
                    RpcUnequipArmor(currentChestArmor);
                }
                currentChestArmor = armorObject;
                equipPoint = chestEquipPoint;
                break;
            case ArmorType.Legs:
                if (currentLegArmor != null)
                {
                    RpcUnequipArmor(currentLegArmor);
                }
                currentLegArmor = armorObject;
                equipPoint = legsEquipPoint;
                break;
        }

        if (equipPoint != null)
        {
            armorObject.GetComponent<Armor>().CmdEquip(equipPoint);
        }
    }

    /// <summary>
    /// Synchronizes unequipping of an armor piece across all clients
    /// </summary>
    /// <param name="armorObject"></param>
    [ClientRpc]
    private void RpcUnequipArmor(GameObject armorObject)
    {
        if (armorObject.TryGetComponent(out Armor armor))
        {
            armor.CmdUnequip();
        }
    }

    /// <summary>
    /// Updates the visual representation of equipped head armor when changed
    /// </summary>
    /// <param name="oldArmor"></param>
    /// <param name="newArmor"></param>
    private void OnHeadArmorChanged(GameObject oldArmor, GameObject newArmor)
    {
        UpdateArmorVisual(newArmor, headEquipPoint);
    }
    /// <summary>
    /// Updates the visual representation of equipped chest armor when changed
    /// </summary>
    /// <param name="oldArmor"></param>
    /// <param name="newArmor"></param>
    private void OnChestArmorChanged(GameObject oldArmor, GameObject newArmor)
    {
        UpdateArmorVisual(newArmor, chestEquipPoint);
    }

    /// <summary>
    /// Updates the visual representation of equipped leg armor when changed
    /// </summary>
    /// <param name="oldArmor"></param>
    /// <param name="newArmor"></param>
    private void OnLegArmorChanged(GameObject oldArmor, GameObject newArmor)
    {
        UpdateArmorVisual(newArmor, legsEquipPoint);
    }

    /// <summary>
    /// Updates the parent transform and position of an armor piece visually
    /// </summary>
    /// <param name="armorObject"></param>
    /// <param name="equipPoint"></param>
    private void UpdateArmorVisual(GameObject armorObject, Transform equipPoint)
    {
        if (armorObject == null || equipPoint == null) return;

        Armor armor = armorObject.GetComponent<Armor>();
        if (armor != null)
        {
            armor.transform.SetParent(equipPoint);
            armor.transform.localPosition = Vector3.zero;
            armor.transform.localRotation = Quaternion.identity;
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

        if (currentHeadArmor != null)
        {
            Armor headArmor = currentHeadArmor.GetComponent<Armor>();
            if (headArmor != null)
                reducedDamage = headArmor.ModifyDamage(reducedDamage);
        }

        if (currentChestArmor != null)
        {
            Armor chestArmor = currentChestArmor.GetComponent<Armor>();
            if (chestArmor != null)
                reducedDamage = chestArmor.ModifyDamage(reducedDamage);
        }

        if (currentLegArmor != null)
        {
            Armor legArmor = currentLegArmor.GetComponent<Armor>();
            if (legArmor != null)
                reducedDamage = legArmor.ModifyDamage(reducedDamage);
        }

        return reducedDamage;
    }
}
