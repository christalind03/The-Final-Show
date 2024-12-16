//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Mirror;

///// <summary>
///// Represents an armor piece that can be equipped by an entity.
///// Gives defensive capabilities and special effects.
///// </summary>
//public class Armor : NetworkBehaviour, IInventoryItem
//{
//    public enum ArmorType
//    {
//        Head,
//        Chest,
//        Legs
//    }

//    [Header("Armor Properties")]
//    [SerializeField] private string armorName;
//    [SerializeField] private float defense;
//    [SerializeField] private string specialEffect;
//    [SerializeField] private ArmorType armorType;

//    private bool isEquipped;

//    // Getters
//    public string ArmorName => armorName;
//    public float Defense => defense;
//    public string SpecialEffect => specialEffect;
//    public bool IsEquipped => isEquipped;
//    public ArmorType Type => armorType;

//    /// <summary>
//    /// Equips the armor piece by attaching it to the specified equip point
//    /// </summary>
//    /// <param name="gameObject">gameObject where the equip point can be found</param>
//    public void Interact(GameObject gameObject)
//    {
//        Transform equipPoint = gameObject.transform.Find("headEquipPoint");
//        transform.SetParent(equipPoint);
//        transform.localPosition = Vector3.zero;
//        transform.localRotation = Quaternion.identity;
//        isEquipped = true;
//    }

//    /// <summary>
//    /// Unequips the armor piece and detaches it from its parent
//    /// </summary>
//    [Command]
//    public void CmdUnequip(){
//        NetworkIdentity armorIdentity = GetComponent<NetworkIdentity>();
//        if (armorIdentity != null)
//        {
//            armorIdentity.RemoveClientAuthority();
//        }
//        transform.SetParent(null);
//        RpcUnequip();
//    }

//    /// <summary>
//    /// Unequips the armor piece to reflect on all clients
//    /// </summary>
//    [ClientRpc] 
//    private void RpcUnequip(){
//        transform.SetParent(null);
//    }

//    /// <summary>
//    /// Reduces incoming damage based on the armor's defense value.
//    /// </summary>
//    /// <param name="incomingDamage">The original damage value</param>
//    /// <returns>The reduced damage value</returns>
//    public float ModifyDamage(float incomingDamage)
//    {
//        float reducedDamage = Mathf.Max(incomingDamage - defense, 0);
//        Debug.Log($"{armorName} reduced damage from {incomingDamage} to {reducedDamage}.");
//        return reducedDamage;
//    }

//    // Apply the special effect of the armor
//    private void ApplyEffect()
//    {
//        if (!string.IsNullOrEmpty(specialEffect))
//        {
//            Debug.Log($"{specialEffect} effect applied by {armorName}.");
//        }
//    }

//    // Remove the special effect of the armor
//    private void RemoveEffect()
//    {
//        if (!string.IsNullOrEmpty(specialEffect))
//        {
//            Debug.Log($"{specialEffect} effect removed by {armorName}.");
//        }
//    }
//}