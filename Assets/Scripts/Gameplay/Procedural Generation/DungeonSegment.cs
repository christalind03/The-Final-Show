using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonSegment : MonoBehaviour
{
    [SerializeField] private List<DungeonEntrance> _dungeonEntrances;
    [SerializeField] private List<Collider> _collisionDetectors;

    // TODO: Document
    public bool ContainsEntryPoints()
    {
        return 0 < _dungeonEntrances.Count;
    }

    // TODO: Document
    public Transform SelectEntrancePoint()
    {
        if (0 < _dungeonEntrances.Count)
        {
            int entranceIndex = Random.Range(0, _dungeonEntrances.Count);
            return _dungeonEntrances[entranceIndex].transform;
        }

        return null;
    }

    // TODO: Document
    public void RemoveEntrancePoint(Transform entrancePoint)
    {
        DungeonEntrance firstOccurrence = _dungeonEntrances.FirstOrDefault(dungeonEntrance => dungeonEntrance.transform == entrancePoint);

        if (firstOccurrence != null)
        {
            _dungeonEntrances.Remove(firstOccurrence);
        }
    }

    // TODO: Document
    public void BlockEntrances()
    {
        foreach (DungeonEntrance dungeonEntrance in _dungeonEntrances)
        {
            dungeonEntrance.BlockEntrance();
        }
    }

    // TODO: Document
    public Bounds RetrieveBounds()
    {
        Bounds[] collisionBounds = _collisionDetectors.Select(collisionDetector => collisionDetector.bounds).ToArray();
        return UnityUtils.CalculateBounds(collisionBounds);
    }

    // TODO: Document
    public List<Collider> RetrieveValidators()
    {
        return _collisionDetectors;
    }
}
