using UnityEngine;

public class DungeonEntrance : MonoBehaviour
{
    [SerializeField] private GameObject _entranceBlocker;

    // TODO: Document
    private void Awake()
    {
        _entranceBlocker?.SetActive(false);
    }

    // TODO: Document
    public void BlockEntrance()
    {
        _entranceBlocker?.SetActive(true);
    }
}
