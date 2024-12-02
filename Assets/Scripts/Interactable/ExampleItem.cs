using UnityEngine;
/// Commented out to modify to work with inventory
///// <summary>
///// An example component to showcase the implementation of the IInteractable interface.
///// </summary>
//public class ExampleItem : MonoBehaviour, IInteractable
//{
//    /// <summary>
//    /// Disable the current gameObject this component is tied to after the player interacts with it.
//    /// </summary>
//    public void Interact()
//    {
//        gameObject.SetActive(false);
//        Debug.Log($"Picked up {transform.name}");
//    }
//}
public class ExampleItem : MonoBehaviour, IInteractable
{
    public PlayerInventory playerInventory;

    public void Interact()
    {
        // Add this item to the inventory
        PlayerInventoryItem inventoryItem = GetComponent<PlayerInventoryItem>();
        if (inventoryItem != null)
        {
            playerInventory.AddItemToSlot("Slot 1", inventoryItem); // Add to a specific slot
            Debug.Log($"Picked up {transform.name} and added to inventory.");
            gameObject.SetActive(false); // Disable the item in the scene
        }
        else
        {
            Debug.LogError($"{name} does not have a PlayerInventoryItem component!"); //was having issues with GameObject and PlayerInventoryItem
        }
    }
}