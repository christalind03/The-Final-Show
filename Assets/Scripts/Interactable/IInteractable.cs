using UnityEngine;

/// <summary>
/// The interface in which all interactable objects should derive from.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Executes the interaction logic for any given interactable object.
    /// </summary>
    /// <param name="playerObject">The player interacting with the object</param>
    public void Interact(GameObject playerObject);
}
