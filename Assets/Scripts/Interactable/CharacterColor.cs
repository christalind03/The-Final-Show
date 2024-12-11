using UnityEngine;

public class CharacterColor : MonoBehaviour, IInteractable
{
    /// <summary>
    /// Change the player's character material to the current object's material.
    /// </summary>
    /// <param name="playerObject">The player interacting with the object</param>
    public void Interact(GameObject playerObject)
    {
        // NOTE: After adding in assets for player characters, we'll need to convert the playerRenderer from MeshRenderer to SkinnedMeshRenderer
        MeshRenderer objectRenderer = gameObject.GetComponent<MeshRenderer>();
        MeshRenderer playerRenderer = playerObject.GetComponentInChildren<MeshRenderer>();

        playerRenderer.material.SetColor("_BaseColor", objectRenderer.material.color);
    }
}
