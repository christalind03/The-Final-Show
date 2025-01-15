using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Used to control who joined the server and register their name into a list so it can be referenced later
/// </summary>
public class PlayerManager : NetworkBehaviour
{
    private static Dictionary<string, GameObject> playerRegistry = new Dictionary<string, GameObject>();

    /// <summary>
    /// Registers the player into the list
    /// </summary>
    /// <param name="identity">the player's network identity</param>
    /// <param name="playerName">the player's name</param>
    public static void RegisterPlayer(string name, GameObject obj)
    {
        if (!playerRegistry.ContainsKey(name))
        {
            playerRegistry.Add(name, obj);
        }
    }

    /// <summary>
    /// Unregister the player from the list
    /// </summary>
    /// <param name="identity">the player's network identity</param>
    public static void UnregisterPlayer(string name)
    {
        if (playerRegistry.ContainsKey(name))
        {
            playerRegistry.Remove(name);
        }
    }

    /// <summary>
    /// Gets the player list for use
    /// </summary>
    /// <returns>The player dictionary</returns>
    public static Dictionary<string, GameObject> GetPlayerNameList()
    {
        return playerRegistry;
    }
}
