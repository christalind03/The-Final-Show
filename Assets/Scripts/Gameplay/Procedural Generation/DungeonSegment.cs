using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Represents a segment of the dungeon, which may contain entrances and collision detectors.
/// </summary>
public class DungeonSegment : MonoBehaviour
{
    [SerializeField] private List<DungeonEntrance> _dungeonEntrances;
    [SerializeField] private List<Collider> _collisionDetectors;

    /// <summary>
    /// Checks whether the dungeon segment contains entrance points.
    /// </summary>
    /// <returns><c>true</c> if the segment contains one or more entrances; otherwise <c>false</c></returns>
    public bool ContainsEntryPoints()
    {
        return 0 < _dungeonEntrances.Count;
    }

    /// <summary>
    /// Selects a random entrance point from the available entrances in the dungeon segment.
    /// </summary>
    /// <returns>The <see cref="Transform"/> of the randomly selected entrance point, or null if no entrances are available</returns>
    public Transform SelectEntrancePoint()
    {
        if (0 < _dungeonEntrances.Count)
        {
            int entranceIndex = Random.Range(0, _dungeonEntrances.Count);
            return _dungeonEntrances[entranceIndex].transform;
        }

        return null;
    }

    /// <summary>
    /// Removes a specific entrance point from the list of entrances in the dungeon segment.
    /// </summary>
    /// <param name="entrancePoint">The <see cref="Transform"/> of the entrance point to be removed</param>
    public void RemoveEntrancePoint(Transform entrancePoint)
    {
        DungeonEntrance firstOccurrence = _dungeonEntrances.FirstOrDefault(dungeonEntrance => dungeonEntrance.transform == entrancePoint);

        if (firstOccurrence != null)
        {
            _dungeonEntrances.Remove(firstOccurrence);
        }
    }

    /// <summary>
    /// Blocks all entrance points in the dungeon segment.
    /// </summary>
    public void BlockEntrances()
    {
        foreach (DungeonEntrance dungeonEntrance in _dungeonEntrances)
        {
            dungeonEntrance.BlockEntrance();
        }
    }

    /// <summary>
    /// Retrieves the combined bounds of all collision detectors in the dungeon segment.
    /// </summary>
    /// <returns>The <see cref="Bounds"/> that encompass all the collision detectors in the segment</returns>
    public Bounds RetrieveBounds()
    {
        Bounds[] collisionBounds = _collisionDetectors.Select(collisionDetector => collisionDetector.bounds).ToArray();
        return UnityUtils.CalculateBounds(collisionBounds);
    }

    /// <summary>
    /// Retrieves the list of collision detectors used for validating collisions within the dungeon segment.
    /// </summary>
    /// <returns>A list of <see cref="Collider"/> components used for collision validation</returns>
    public List<Collider> RetrieveValidators()
    {
        return _collisionDetectors;
    }
}
