using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Dungeon Segments")]
    [SerializeField] private GameObject[] _entrancePrefabs;
    [SerializeField] private GameObject[] _exitPrefabs;
    [SerializeField] private GameObject[] _hallwayPrefabs;
    [SerializeField] private GameObject[] _roomPrefabs;

    [Header("Dungeon Size")]
    [Min(1)]
    [SerializeField] 
    private int _minimumRooms;
    
    [SerializeField] private int _maximumRooms;

    [SerializeField]
    [Tooltip("The percentage for spawning dungeon rooms instead of hallways")]
    private float _spawnRate;

    [Header("Additional Parameters")]
    [SerializeField] private LayerMask _collisionLayer;

    private List<DungeonSegment> _existingSegments;

    // TODO: Document
    private void Start()
    {
        // TODO: Start debugging/optimization timer

        _existingSegments = new List<DungeonSegment>();
        GenerateDungeon();

        // TODO: Stop debugging/optimization timer
    }

    // TODO: Document
    private void GenerateDungeon()
    {
        int numRooms = 0;
        int totalRooms = Random.Range(_minimumRooms, _maximumRooms + 1);
        int exitIndex = Random.Range(0, totalRooms + 1);

        while (numRooms < totalRooms)
        {
            numRooms++;

            if (_existingSegments.Count == 0)
            {
                GameObject segmentObject = SpawnSegmentFrom(_entrancePrefabs);

                if (segmentObject.TryGetComponent(out DungeonSegment dungeonSegment) && dungeonSegment.ContainsEntryPoints())
                {
                    _existingSegments.Add(dungeonSegment);
                }

                //numRooms++;
            }
            else
            {
                DungeonSegment existingSegment = null;
                Transform existingEntry = null;

                // Attempt to find an existing room with an open entrance.
                while (existingSegment == null && 0 < _existingSegments.Count)
                {
                    int tempIndex = Random.Range(0, _existingSegments.Count);
                    DungeonSegment tempSegment = _existingSegments[tempIndex];

                    // If the selected segment contains an available entry point, set it as the pre-existing segment.
                    // Otherwise, remove it from the existing segments list to avoid reusing it.
                    if (tempSegment.ContainsEntryPoints())
                    {
                        existingSegment = tempSegment;
                        existingEntry = tempSegment.SelectEntrancePoint();
                        break;
                    }
                    else
                    {
                        _existingSegments.Remove(tempSegment);
                    }
                }

                if (existingSegment != null && existingEntry != null)
                {
                    // Generate a dungeon segment
                    bool isHallway = Random.Range(0f, 1f) < _spawnRate;
                    GameObject dungeonObject = SpawnSegmentFrom(isHallway ? _hallwayPrefabs : _roomPrefabs);

                    DungeonSegment generatedSegment = null;
                    Transform generatedEntry = null;

                    if (dungeonObject.TryGetComponent(out DungeonSegment dungeonSegment))
                    {
                        generatedSegment = dungeonSegment;
                        generatedEntry = dungeonSegment.SelectEntrancePoint();
                    }

                    // If an generated segment or its corresponding entry could not be found, display and error and exit the dungeon generator.
                    if (generatedSegment != null && generatedEntry != null)
                    {
                        AlignSegments(existingSegment.transform, existingEntry, generatedSegment.transform, generatedEntry);

                        // If the generated segment contains intersections, destroy the object in hopes of it not occurring again.
                        if (ContainsIntersections(generatedSegment))
                        {
                            UnityUtils.LogWarning($"Collision detected between {generatedSegment.name} and {existingSegment.name}...");
                            Destroy(generatedSegment.gameObject);
                            continue;
                        }

                        // Remove the following entrance points to prevent them from being used again.
                        existingSegment.RemoveEntrancePoint(existingEntry);
                        generatedSegment.RemoveEntrancePoint(generatedEntry);

                        // Add the newly generated segment to the existing segments list, given that it contains valid entrance points.
                        if (generatedSegment.ContainsEntryPoints())
                        {
                            _existingSegments.Add(generatedSegment);
                        }

                        //numRooms++;
                    }
                    else
                    {
                        UnityUtils.LogError("Unable to locate generated segment and its corresponding entry point.");
                        break;
                    }
                }
                else
                {
                    UnityUtils.LogError("Unable to locate existing segment and its corresponding entry point.");
                    break;
                }
            }
        }
    }

    // TODO: Document
    private GameObject SpawnSegmentFrom(GameObject[] prefabArray)
    {
        int segmentIndex = Random.Range(0, prefabArray.Length);
        GameObject segmentObject = Instantiate(prefabArray[segmentIndex]);

        return segmentObject;
    }

    // TODO: Document
    private void AlignSegments(Transform existingSegment, Transform existingEntry, Transform generatedSegment, Transform generatedEntry)
    {
        // Ensure that the two entries are pointing in opposite directions
        float existingRotation = Mathf.Repeat(existingEntry.eulerAngles.y, 360); // Normalize positive values for rotations
        float ganeratedRotation = Mathf.Repeat(generatedEntry.eulerAngles.y, 360); // Since we're only having a 2D dungeon, only look at the y-axis
        float rotationalDifference = existingRotation - ganeratedRotation;

        if (Mathf.Abs(rotationalDifference) != 180f)
        {
            float rotationOffset = rotationalDifference > 180f ? 180f - rotationalDifference : 180f + rotationalDifference;
            generatedSegment.Rotate(0, rotationOffset, 0);
        }

        // Align the existing and generated segments' entry points
        Vector3 positionOffset = generatedSegment.position - generatedEntry.position;
        generatedSegment.position = positionOffset + existingEntry.position;

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
                    Debug.Log($"{dungeonSegment.name} intersects with {hitContact.transform.root.name}'s {hitContact.name}");
                    containsIntersections = true;
                    break;
                }
            }
        }

        return containsIntersections;
    }
}
