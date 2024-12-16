using UnityEngine;

public class CharacterColor : MonoBehaviour, IInteractable
{
    /// <summary>
    /// Change the player's character material to the current object's material.
    /// </summary>
    /// <param name="playerObject">The player interacting with the object</param>
    public void Interact(GameObject playerObject)
    {
        // Since the `Player` gameObject is a container for everything related to the player, we must retrieve the player's actual mesh by traversing the hierarchy.
        MeshRenderer objectRenderer = gameObject.GetComponent<MeshRenderer>();
        SkinnedMeshRenderer playerRenderer = playerObject.transform.GetChild(0).GetComponentInChildren<SkinnedMeshRenderer>();

        playerRenderer.material.SetColor("_BaseColor", objectRenderer.material.color);
    }
}
