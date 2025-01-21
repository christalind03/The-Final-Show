using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Used to control who joined the server and register their name into a list so it can be referenced later
/// </summary>
public class PlayerManager : NetworkBehaviour
{
    private static Dictionary<uint, GameObject> playerRegistry = new Dictionary<uint, GameObject>();

    /// <summary>
    /// Registers the player into the list
    /// </summary>
    /// <param name="identity">the player's network identity</param>
    /// <param name="playerName">the player's name</param>
    public static void RegisterPlayer(uint netid, GameObject obj)
    {
        if (!playerRegistry.ContainsKey(netid))
        {
            playerRegistry.Add(netid, obj);
        }
    }

    /// <summary>
    /// Unregister the player from the list
    /// </summary>
    /// <param name="identity">the player's network identity</param>
    public static void UnregisterPlayer(uint netid)
    {
        if (playerRegistry.ContainsKey(netid))
        {
            playerRegistry.Remove(netid);
        }
    }

    /// <summary>
    /// Gets the player list for use
    /// </summary>
    /// <returns>The player dictionary</returns>
    public static Dictionary<uint, GameObject> GetObjectList()
    {
        return playerRegistry;
    }
    
    public static Dictionary<uint,string> GetPlayerNameList(){
        Dictionary<uint,string> names = new Dictionary<uint,string>();
        foreach(var entry in playerRegistry){
            names.Add(entry.Key, entry.Value.name);
        }
        return names;
    }
}
