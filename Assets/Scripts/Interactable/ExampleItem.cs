using UnityEngine;

public class ExampleItem : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Destroy(gameObject);
        Debug.Log($"Picked up {transform.name}");
    }
}
