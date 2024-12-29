using Mirror;
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
}
