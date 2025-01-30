using Mirror;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SafeZone : MonoBehaviour
{
    public List<GameObject> SafePlayers = new List<GameObject>();
    public bool ContainsPlayers
    {
        get { return 0 < SafePlayers.Count; }
    }

    /// <summary>
    /// Triggered when another object enters this object's trigger collider.
    /// If the other collider belongs to a player, then it adds the player to the list of safe players.
    /// </summary>
    /// <param name="otherCollider">The collider of the object entering the trigger</param>
    private void OnTriggerEnter(Collider otherCollider)
    {
        //if (!isServer) { return; }

        if (otherCollider.CompareTag("Player"))
        {
            SafePlayers.Add(otherCollider.gameObject);
        }
    }

    /// <summary>
    /// Triggered when another object exits this object's trigger collider.
    /// If the other collider belongs to a player, then it removes the player from the list of safe players.
    /// </summary>
    /// <param name="otherCollider">The collider of the object exiting the trigger</param>
    private void OnTriggerExit(Collider otherCollider)
    {
        //if (!isServer) { return; }

        if (otherCollider.CompareTag("Player"))
        {
            SafePlayers.Remove(otherCollider.gameObject);
        }
    }
}
