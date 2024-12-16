using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Inventory Item", menuName = "Inventory Item")]
public class InventoryItem : ScriptableObject, IInteractable
{
    [Header("Inventory Item Data")]
    [SerializeField] private GameObject _objectPrefab;

    public GameObject ObjectPrefab { get { return _objectPrefab; } }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="playerObject"></param>
    public void Interact(GameObject playerObject)
    {
        throw new System.NotImplementedException();
    }
}
