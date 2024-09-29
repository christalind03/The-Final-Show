using UnityEngine;

/// <summary>
/// An example component to showcase the implementation of the IInteractable interface.
/// </summary>
public class ExampleItem : MonoBehaviour, IInteractable
{
    /// <summary>
    /// Destroy the current gameObject this component is tied to after the player interacts with it.
    /// </summary>
    public void Interact()
    {
        Destroy(gameObject);
        Debug.Log($"Picked up {transform.name}");
    }
}
