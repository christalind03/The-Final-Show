using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A static utility class containing helper methods for network-related tasks.
/// </summary>
public static class NetworkUtils
{
    /// <summary>
    /// Retrieves the <see cref="GameObject"/> representing the local player in the scene.
    /// This is done by searching through all player controllers within the current scene and returns the one with a <see cref="NetworkIdentity"/> that is marked as the local player.
    /// </summary>
    /// <returns>The <see cref="GameObject"/> of the local player, if found; otherwise returns <c>null</c>.</returns>
    public static GameObject RetrieveLocalPlayer()
    {
        PlayerController[] playerControllers = GameObject.FindObjectsOfType<PlayerController>();

        for (int i = 0; i < playerControllers.Length; i++)
        {
            NetworkIdentity networkIdentity = playerControllers[i].GetComponent<NetworkIdentity>();

            if (networkIdentity.isLocalPlayer)
            {
                return networkIdentity.gameObject;
            }
        }

        return null;
    }

    /// <summary>
    /// Retrieves a list of all clients' GameObjects connected to the server.
    /// This method filters out clients that do not have an assigned identity.
    /// </summary>
    /// <returns>A list of <see cref="GameObject"/> representing all players connected to the server.</returns>
    public static List<GameObject> RetrievePlayers()
    {
        return NetworkServer.connections.Values
            .Where(clientConnection => clientConnection.identity != null)
            .Select(clientConnection => clientConnection.identity.gameObject)
            .ToList();
    }

    /// <summary>
    /// Waits until the current client or server is ready, based on the connection status, and executes the provided callback.
    /// </summary>
    /// <param name="onReady">The action to execute once the client or server is ready</param>
    /// <returns>An <see cref="IEnumerator"/> for coroutine execution.</returns>
    public static IEnumerator WaitUntilReady(Action<NetworkIdentity> onReady)
    {
        yield return new WaitUntil(() =>
        {
            if (NetworkServer.active)
            {
                // On the server, check if the connectionToClient is ready
                return NetworkClient.connection?.identity != null && NetworkClient.connection.identity.connectionToClient.isReady;
            }
            else if (NetworkClient.active)
            {
                // On the client, check if the connection is established
                return NetworkClient.connection?.identity != null && NetworkClient.isConnected && NetworkClient.ready;
            }
            return false;
        });

        onReady?.Invoke(NetworkClient.connection.identity);
    }
}
