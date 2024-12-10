using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents an armor piece that can be equipped by an entity.
/// Gives defensive capabilities and special effects.
/// </summary>
public class Armor : MonoBehaviour
{
    public enum ArmorType
    {
        Head,
        Chest,
        Legs
    }

    [Header("Armor Properties")]
    [SerializeField] private string armorName;
    [SerializeField] private float defense;
    [SerializeField] private string specialEffect;
    [SerializeField] private ArmorType armorType;

    private bool isEquipped;

    // Getters
    public string ArmorName => armorName;
    public float Defense => defense;
    public string SpecialEffect => specialEffect;
    public bool IsEquipped => isEquipped;
    public ArmorType Type => armorType;

    /// <summary>
    /// Equips the armor to the specified equip point.
    /// </summary>
    /// <param name="equipPoint">The transform representing the equip point.</param>
    public void Equip(Transform equipPoint)
    {
        // Attach to the equip point
        transform.SetParent(equipPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        Debug.Log($"{armorName} equipped on {equipPoint.name}.");
    }

    /// <summary>
    /// Unequips the armor by detaching it from its equip point.
    /// </summary>
    public void Unequip()
    {
        transform.SetParent(null);
        Debug.Log($"{armorName} unequipped.");
    }

    /// <summary>
    /// Reduces incoming damage based on the armor's defense value.
    /// </summary>
    /// <param name="incomingDamage">The original damage value.</param>
    /// <returns>The reduced damage value.</returns>
    public float ModifyDamage(float incomingDamage)
    {
        float reducedDamage = Mathf.Max(incomingDamage - defense, 0);
        Debug.Log($"{armorName} reduced damage from {incomingDamage} to {reducedDamage}.");
        return reducedDamage;
    }

    // Apply the special effect of the armor
    private void ApplyEffect()
    {
        if (!string.IsNullOrEmpty(specialEffect))
        {
            Debug.Log($"{specialEffect} effect applied by {armorName}.");
        }
    }

    // Remove the special effect of the armor
    private void RemoveEffect()
    {
        if (!string.IsNullOrEmpty(specialEffect))
        {
            Debug.Log($"{specialEffect} effect removed by {armorName}.");
        }
    }
}
