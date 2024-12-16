//using UnityEngine;
//using Mirror;
//using UnityEngine.XR;
//using UnityEngine.UIElements;

///// <summary>
///// An abstract base class for melee weapon types in the game
///// </summary>
//public abstract class Weapon : NetworkBehaviour, IInventoryItem
//{
//    public string WeaponName;
//    public float Damage;
//    public string DamageType;
//    public float Cooldown;
//    public bool IsEquipped;

//    /// <summary>
//    /// Equips the weapon
//    /// </summary>
//    public void Interact(GameObject gameObject)
//    {
//        IsEquipped = true;
//        Transform handTransform = gameObject.transform.Find("HandTransform");
//        if (handTransform != null)
//        {
//            transform.SetParent(handTransform);
//            transform.localPosition = Vector3.zero;
//            transform.localRotation = Quaternion.identity; // Set the correct rotation for the weapon when held
//            transform.localScale = Vector3.one; // Reset the scale to 1,1,1 to make sure it's visible
//        }
//    }

//    /// <summary>
//    /// Unequips the weapon on server
//    /// </summary>
//    [Command]
//    public virtual void CmdUnequip()
//    {
//        NetworkIdentity weaponIdentity = GetComponent<NetworkIdentity>();
//        if (weaponIdentity != null)
//        {
//            weaponIdentity.RemoveClientAuthority();
//        }
//        IsEquipped = false;
//        transform.SetParent(null);
//        RpcUnequip();
//    }

//    /// <summary>
//    /// Updates the unequipped weapon state on all clients
//    /// </summary>
//    [ClientRpc]
//    public virtual void RpcUnequip(){
//        transform.SetParent(null);
//    }

//    /// <summary>
//    /// Executes the primary attack for the weapon
//    /// </summary>
//    public abstract void Attack();

//    /// <summary>
//    ///  Executes the alternate (secondary) attack action for the weapon
//    /// </summary>
//    public abstract void AlternateAttack();
//}