using System.Collections.Generic;
using UnityEngine;

public class DungeonSegment : MonoBehaviour
{
    [SerializeField] private List<Transform> _entrancePoints;
    [SerializeField] private Collider[] _collisionDetectors;

    // TODO: Document
    public bool ContainsEntryPoints()
    {
        return 0 < _entrancePoints.Count;
    }

    // TODO: Document
    public Transform SelectEntrancePoint()
    {
        if (0 < _entrancePoints.Count)
        {
            return _entrancePoints[Random.Range(0, _entrancePoints.Count)];
        }
       
        return null;
    }

    // TODO: Document
    public void RemoveEntrancePoint(Transform entrancePoint)
    {
        if (_entrancePoints.Contains(entrancePoint))
        {
            _entrancePoints.Remove(entrancePoint);
        }
    }

    // TODO: Document
    public Collider[] RetrieveValidators()
    {
        return _collisionDetectors;
    }
}
