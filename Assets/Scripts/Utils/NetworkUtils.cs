using Mirror;
using UnityEngine;

public static class NetworkUtils
{
    // TODO: Document
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
