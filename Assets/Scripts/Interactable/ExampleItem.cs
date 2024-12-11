using UnityEngine;

/// <summary>
/// An example component to showcase the implementation of the IInteractable interface.
/// </summary>
public class ExampleItem : MonoBehaviour, IInventoryItem
{
    /// <summary>
    /// Disable the current gameObject this component is tied to after the player interacts with it.
    /// </summary>
    /// <param name="playerObject">The player interacting with the object</param>
    public void Interact(GameObject playerObject)
    {
        // 
    }
}