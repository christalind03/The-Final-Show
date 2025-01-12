using Mirror;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SafeZone : NetworkBehaviour
{
    public List<GameObject> SafePlayers {  get; private set; }
    public bool ContainsPlayers
    {
        get { return 0 < SafePlayers.Count; }
    }

    // TODO: Document
    public override void OnStartServer()
    {
        SafePlayers = new List<GameObject>();
    }

    // TODO: Document
    private void OnTriggerEnter(Collider otherCollider)
    {
        if (!isServer) { return; }

        if (otherCollider.CompareTag("Player"))
        {
            Debug.Log($"{otherCollider.name} has entered the safe zone.");
            SafePlayers.Add(otherCollider.gameObject);
        }
    }

    // TODO: Document
    private void OnTriggerExit(Collider otherCollider)
    {
        if (!isServer) { return; }

        if (otherCollider.CompareTag("Player"))
        {
            Debug.Log($"{otherCollider.name} has left the safe zone.");
            SafePlayers.Remove(otherCollider.gameObject);
        }
    }
}
