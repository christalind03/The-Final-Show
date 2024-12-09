using UnityEngine;

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

    public string ArmorName => armorName;
    public float Defense => defense;
    public string SpecialEffect => specialEffect;
    public bool IsEquipped => isEquipped;
    public ArmorType Type => armorType;


    public void Equip(Transform equipPoint)
    {
        // Attach to the equip point
        transform.SetParent(equipPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        Debug.Log($"{armorName} equipped on {equipPoint.name}.");
    }

    public void Unequip()
    {
        transform.SetParent(null);
        Debug.Log($"{armorName} unequipped.");
    }

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
