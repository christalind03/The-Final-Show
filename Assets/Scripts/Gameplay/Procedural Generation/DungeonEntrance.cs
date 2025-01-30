using UnityEngine;

/// <summary>
/// Represents the entrance to a dungeon, providing functionality to block or unblock access.
/// </summary>
public class DungeonEntrance : MonoBehaviour
{
    [SerializeField] private GameObject _entranceBlocker;

    /// <summary>
    /// Disables the entrance blocker, if it exists.
    /// </summary>
    private void Awake()
    {
        _entranceBlocker?.SetActive(false);
    }

    /// <summary>
    /// Activates the entrance blocker, preventing access to the dungeon.
    /// </summary>
    public void BlockEntrance()
    {
        _entranceBlocker?.SetActive(true);
    }
}
