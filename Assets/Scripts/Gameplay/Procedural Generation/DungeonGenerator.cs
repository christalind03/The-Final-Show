using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Dungeon Segments")]
    [SerializeField] private GameObject[] _entrancePrefabs;
    [SerializeField] private GameObject[] _exitPrefabs;
    [SerializeField] private GameObject[] _hallwayPrefabs;
    [SerializeField] private GameObject[] _roomPrefabs;

    [Header("Dungeon Size")]
    [Tooltip("The size of the dungeon is inclusive to the entrance and exit rooms.")]

    [Min(2)]
    [SerializeField]
    private int _minimumRooms;

    [SerializeField]
    private int _maximumRooms;

    [SerializeField]
    [Tooltip("The percentage for spawning dungeon rooms instead of hallways")]
    private float _spawnRate;

    [Header("Additional Parameters")]
    [SerializeField] private LayerMask _collisionLayer;

    private List<DungeonSegment> _existingSegments;

    // TODO: Document
    private void Start()
    {
        Stopwatch debuggingStopwatch = new Stopwatch();
        debuggingStopwatch.Start();

        _existingSegments = new List<DungeonSegment>();
        GenerateDungeon();

        debuggingStopwatch.Stop();

        UnityEngine.Debug.Log($"Completed dungeon generation in {debuggingStopwatch.ElapsedMilliseconds}ms");
    }

    // TODO: Document
    private void OnValidate()
    {
        if (_maximumRooms < _minimumRooms)
        {
            _maximumRooms = _minimumRooms;
        }
    }

    // TODO: Document, Refactor (heavily)
    private void GenerateDungeon()
    {
        // Since the maximum parameter is exclusive, ensure we add by one to make it inclusive
        int totalRooms = Random.Range(_minimumRooms, _maximumRooms + 1);
        int exitIndex = Random.Range(1, totalRooms + 1); // Entrance occupies the 0th index
        int roomIndex = 0;

        while (roomIndex < totalRooms)
        {
            if (_existingSegments.Count == 0)
            {
                SpawnEntrance();
                roomIndex++;
            }
            else
            {
                if (!TryExistingSegment(out DungeonSegment existingSegment, out Transform existingEntrance))
                {
                    UnityUtils.LogError("Unable to locate existing segment and its corresponding entry point.");
                    return;
                }

                bool isExit = roomIndex == exitIndex;
                bool isHallway = !isExit && _spawnRate <= Random.Range(0f, 1f);

                if (!TryGeneratedSegment(isExit, isHallway, out DungeonSegment generatedSegment, out Transform generatedEntrance))
                {
                    UnityUtils.LogError("Unable to locate generated segment and its corresponding entry point.");
                    return;
                }

                AlignSegments(generatedSegment.transform, generatedEntrance, existingEntrance);

                if (ContainsIntersections(generatedSegment))
                {
                    Destroy(generatedSegment.gameObject);
                    continue;
                }

                existingSegment.RemoveEntrancePoint(existingEntrance);
                generatedSegment.RemoveEntrancePoint(generatedEntrance);

                if (generatedSegment.ContainsEntryPoints())
                {
                    if (!isHallway)
                    {
                        roomIndex++;
                    }

                    _existingSegments.Add(generatedSegment);
                }
            }
        }
    }

    // TODO: Document
    private void SpawnEntrance()
    {
        int entranceIndex = Random.Range(0, _entrancePrefabs.Length);
        GameObject entranceObject = Instantiate(_entrancePrefabs[entranceIndex]);

        if (entranceObject.TryGetComponent(out DungeonSegment entranceSegment) && entranceSegment.ContainsEntryPoints())
        {
            _existingSegments.Add(entranceSegment);
        }
    }    

    // TODO: Document
    private bool TryExistingSegment(out DungeonSegment existingSegment, out Transform existingEntrance)
    {
        existingSegment = null;
        existingEntrance = null;

        while (existingSegment == null)
        {
            if (_existingSegments.Count == 0) { break; }

            int temporaryIndex = Random.Range(0, _existingSegments.Count);
            DungeonSegment temporarySegment = _existingSegments[temporaryIndex];

            if (temporarySegment.ContainsEntryPoints())
            {
                existingSegment = temporarySegment;
                existingEntrance = temporarySegment.SelectEntrancePoint();
                break;
            }
            
            _existingSegments.Remove(temporarySegment);
        }

        return existingSegment != null && existingEntrance != null;
    }

    // TODO: Document
    private bool TryGeneratedSegment(bool isExit, bool isHallway, out DungeonSegment generatedSegment, out Transform generatedEntrance)
    {
        generatedSegment = null;
        generatedEntrance = null;

        GameObject[] segmentPrefabs = RetrieveSegmentPrefabs(isExit, isHallway);

        int segmentIndex = Random.Range(0, segmentPrefabs.Length);
        GameObject segmentObject = Instantiate(segmentPrefabs[segmentIndex]);

        if (segmentObject.TryGetComponent(out DungeonSegment dungeonSegment))
        {
            generatedSegment = dungeonSegment;
            generatedEntrance = dungeonSegment.SelectEntrancePoint();
        }

        return generatedEntrance != null && generatedEntrance != null;
    }

    // TODO: Document
    private GameObject[] RetrieveSegmentPrefabs(bool isExit, bool isHallway)
    {
        if (isExit)
        {
            return _exitPrefabs;
        }
        else
        {
            if (isHallway)
            {
                return _hallwayPrefabs;
            }
            else
            {
                return _roomPrefabs;
            }
        }
    }

    // TODO: Document
    private void AlignSegments(Transform generatedSegment, Transform generatedEntrance, Transform existingEntrance)
    {
        // Ensure that the two entries are pointing in opposite directions
        float existingRotation = Mathf.Repeat(existingEntrance.eulerAngles.y, 360); // Normalize positive values for rotations
        float ganeratedRotation = Mathf.Repeat(generatedEntrance.eulerAngles.y, 360); // Since we're only having a 2D dungeon, only look at the y-axis
        float rotationalDifference = existingRotation - ganeratedRotation;

        if (Mathf.Abs(rotationalDifference) != 180f)
        {
            float rotationOffset = rotationalDifference > 180f ? 180f - rotationalDifference : 180f + rotationalDifference;
            generatedSegment.Rotate(0, rotationOffset, 0);
        }

        // Align the existing and generated segments' entry points
        Vector3 positionOffset = generatedSegment.position - generatedEntrance.position;
        generatedSegment.position = positionOffset + existingEntrance.position;

        // This is required to ensure that the collision detectors' positions on the generated segment update properly
        Physics.SyncTransforms();
    }

    // TODO: Document
    private bool ContainsIntersections(DungeonSegment dungeonSegment)
    {
        bool containsIntersections = false;
        Collider[] segmentValidators = dungeonSegment.RetrieveValidators();

        foreach (Collider segmentValidator in segmentValidators)
        {   
            Collider[] hitContacts = Physics.OverlapBox(segmentValidator.bounds.center, segmentValidator.bounds.extents, Quaternion.identity, _collisionLayer);

            foreach (Collider hitContact in hitContacts)
            {
                if (hitContact != segmentValidator)
                {
                    containsIntersections = true;
                    break;
                }
            }
        }

        return containsIntersections;
    }
}
