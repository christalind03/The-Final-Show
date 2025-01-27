using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Handles the generation of dungeon layouts including the entrance, rooms, hallways, and exits.
/// Additionally manages the dungeon's navigation mesh and enemy spawning.
/// </summary>
[RequireComponent(typeof(NavMeshSurface))]
public class DungeonGenerator : NetworkBehaviour
{
    [SyncVar] private int _randomSeed;

    [Header("Dungeon Segments")]
    [SerializeField] private GameObject[] _entrancePrefabs;
    [SerializeField] private GameObject[] _exitPrefabs;
    [SerializeField] private GameObject[] _hallwayPrefabs;
    [SerializeField] private GameObject[] _roomPrefabs;

    [Header("Dungeon Size")]
    [Tooltip("The size of the dungeon is inclusive to the entrance and exit rooms.")]

    [Min(1), SerializeField] private int _minimumRooms;
    [Min(1), SerializeField] private int _maximumRooms;

    [SerializeField]
    [Tooltip("The percentage for spawning dungeon rooms instead of hallways")]
    private float _spawnRate;

    [SerializeField] private LayerMask _collisionLayer;

    [Header("Enemy Parameters")]
    [Min(1), SerializeField] private int _minimumEnemies;
    [Min(1), SerializeField] private int _maximumEnemies;

    private bool _isGenerated;
    private List<DungeonSegment> _connectableSegments;
    private List<DungeonSegment> _dungeonSegments;

    /// <summary>
    /// Ensures the dungeon generation parameters are valid when the inspector values change.
    /// </summary>
    protected override void OnValidate()
    {
        base.OnValidate();

        if (_maximumRooms < _minimumRooms)
        {
            _maximumRooms = _minimumRooms;
        }

        if (_maximumEnemies < _minimumEnemies)
        {
            _maximumEnemies = _minimumEnemies;
        }
    }

    /// <summary>
    /// Initializes dungeon generation on the client, generating the dungeon itself and its navigation mesh.
    /// </summary>
    public override void OnStartClient()
    {
        UnityEngine.Random.InitState(_randomSeed);

        _connectableSegments = new List<DungeonSegment>();
        _dungeonSegments = new List<DungeonSegment>();

        GenerateDungeon();
        GenerateNavigationMesh();

        if (isServer)
        {
            _isGenerated = true;
        }
    }

    /// <summary>
    /// Initializes the random seed for dungeon generation on the server.
    /// </summary>
    public override void OnStartServer()
    {
        _randomSeed = (int)DateTime.Now.Ticks;
    }

    /// <summary>
    /// Returns whether or not the dungeon has been fully generated.
    /// </summary>
    /// <returns><c>true</c> if the dungeon has finished generating; otherwise <c>false</c></returns>
    public bool IsGenerated()
    {
        return _isGenerated;
    }

    /// <summary>
    /// Generates the dungeon layout by spawning the entrance, rooms, hallways, and exit segments.
    /// </summary>
    private void GenerateDungeon()
    {
        int totalRooms = UnityEngine.Random.Range(_minimumRooms, _maximumRooms + 1); // Since the maximum parameter is exclusive, ensure we add by one to make it inclusive
        int exitIndex = UnityEngine.Random.Range(1, totalRooms); // Entrance occupies the 0th index

        for (int roomIndex = 0; roomIndex < totalRooms;)
        {
            if (_connectableSegments.Count <= 0)
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
                bool isHallway = !isExit && _spawnRate <= UnityEngine.Random.Range(0f, 1f);

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
                    _connectableSegments.Add(generatedSegment);
                }

                _dungeonSegments.Add(generatedSegment);

                if (!isHallway) { roomIndex++; }
            }
        }

        BlockEntrances();
    }

    /// <summary>
    /// Spawns the entrance segment of the dungeon.
    /// </summary>
    private void SpawnEntrance()
    {
        int entranceIndex = UnityEngine.Random.Range(0, _entrancePrefabs.Length);
        GameObject entranceObject = Instantiate(_entrancePrefabs[entranceIndex], transform);

        if (entranceObject.TryGetComponent(out DungeonSegment entranceSegment) && entranceSegment.ContainsEntryPoints())
        {
            _connectableSegments.Add(entranceSegment);
            _dungeonSegments.Add(entranceSegment);
        }
    }

    /// <summary>
    /// Attempts to find an existing connectable segment and its entrance point.
    /// </summary>
    /// <param name="existingSegment">The currently existing segment</param>
    /// <param name="existingEntrance">The currently existing segment's entrance</param>
    /// <returns><c>true</c> if a valid segment was found; otherwise <c>false</c></returns>
    private bool TryExistingSegment(out DungeonSegment existingSegment, out Transform existingEntrance)
    {
        existingSegment = null;
        existingEntrance = null;

        while (existingSegment == null)
        {
            if (_connectableSegments.Count == 0) { break; }

            int temporaryIndex = UnityEngine.Random.Range(0, _connectableSegments.Count);
            DungeonSegment temporarySegment = _connectableSegments[temporaryIndex];

            if (temporarySegment.ContainsEntryPoints())
            {
                existingSegment = temporarySegment;
                existingEntrance = temporarySegment.SelectEntrancePoint();
                break;
            }

            _connectableSegments.Remove(temporarySegment);
        }

        return existingSegment != null && existingEntrance != null;
    }

    /// <summary>
    /// Attempts to generate a new dungeon segment.
    /// </summary>
    /// <param name="isExit">Indicates if this segment should be an exit</param>
    /// <param name="isHallway">Indicates if this segment should be a hallway</param>
    /// <param name="generatedSegment">The newly generated segment</param>
    /// <param name="generatedEntrance">The newly generated segment's entrance</param>
    /// <returns><c>true</c> if a segment was generated successfully; otherwise <c>false</c></returns>
    private bool TryGeneratedSegment(bool isExit, bool isHallway, out DungeonSegment generatedSegment, out Transform generatedEntrance)
    {
        generatedSegment = null;
        generatedEntrance = null;

        GameObject[] segmentPrefabs = RetrieveSegmentPrefabs(isExit, isHallway);

        int segmentIndex = UnityEngine.Random.Range(0, segmentPrefabs.Length);
        GameObject segmentObject = Instantiate(segmentPrefabs[segmentIndex], transform);

        if (segmentObject.TryGetComponent(out DungeonSegment dungeonSegment))
        {
            generatedSegment = dungeonSegment;
            generatedEntrance = dungeonSegment.SelectEntrancePoint();
        }

        return generatedEntrance != null && generatedEntrance != null;
    }

    /// <summary>
    /// Retrieves the appropriate list of segment prefabs based on whether it is an exit, hallway, or a room.
    /// </summary>
    /// <param name="isExit">Indicates if the segment is an exit</param>
    /// <param name="isHallway">Indicates if the segment is a hallway</param>
    /// <returns>An array of prefabs for the requested segment type</returns>
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

    /// <summary>
    /// Aligns the entrance points of two dungeon segments so that they are correctly connected.
    /// </summary>
    /// <param name="generatedSegment">The generated dungeon segment to be aligned</param>
    /// <param name="generatedEntrance">The entrance of the generated dungeon segment</param>
    /// <param name="existingEntrance">The entrance point of the existing dungeon segment</param>
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

    /// <summary>
    /// Checks if the specified dungeon segment contains intersections with other segments.
    /// </summary>
    /// <param name="dungeonSegment">The segment to check for intersections</param>
    /// <returns><c>true</c> if the segment contains intersections; otherwise <c>false</c></returns>
    private bool ContainsIntersections(DungeonSegment dungeonSegment)
    {
        bool containsIntersections = false;
        List<Collider> segmentValidators = dungeonSegment.RetrieveValidators();

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

    /// <summary>
    /// Blocks all entrances that remain in the <see cref="_connectableSegments"/> list.
    /// </summary>
    private void BlockEntrances()
    {
        foreach (DungeonSegment dungeonSegment in _connectableSegments)
        {
            dungeonSegment.BlockEntrances();
        }
    }

    /// <summary>
    /// Generates the navigation mesh for the dungeon using Unity's NavMesh system.
    /// </summary>
    private void GenerateNavigationMesh()
    {
        gameObject.GetComponent<NavMeshSurface>().BuildNavMesh();

        // Ensure the entrance room is added back to the scene hierarchy to maintain organization
        _dungeonSegments[0].transform.SetParent(transform);
    }

    /// <summary>
    /// Spawns enemies in the dungeon after it has been generated.
    /// </summary>
    /// <param name="enemyPrefabs">The array of enemy prefabs to be randomly spawned in the dungeon</param>
    public void SpawnEnemies(GameObject[] enemyPrefabs)
    {
        if (!isServer || !_isGenerated) { return; }

        UnityEngine.Debug.Log("Spawning enemies...");

        // Retrieve the dungeon bounds
        Bounds entranceBounds = _dungeonSegments[0].RetrieveBounds();
        Bounds dungeonBounds = UnityUtils.CalculateBounds(RetrieveSegmentBounds());

        int totalEnemies = UnityEngine.Random.Range(_minimumEnemies, _maximumEnemies + 1);

        for (int enemyIndex = 0; enemyIndex < totalEnemies;)
        {
            Vector3 startPosition = UnityUtils.RandomizePosition(dungeonBounds);

            if (NavMesh.SamplePosition(startPosition, out NavMeshHit closestHit, 1f, NavMesh.AllAreas))
            {
                // Prevent enemies from spawning inside the entrance room
                if (entranceBounds.Contains(closestHit.position)) { continue; }

                int prefabIndex = UnityEngine.Random.Range(0, enemyPrefabs.Length);
                GameObject enemyPrefab = Instantiate(enemyPrefabs[prefabIndex]);

                if (enemyPrefab.TryGetComponent(out NavMeshAgent navMeshAgent))
                {
                    navMeshAgent.Warp(closestHit.position);
                    navMeshAgent.enabled = true;
                }

                NetworkServer.Spawn(enemyPrefab);
                enemyIndex++;
            }
        }
    }

    /// <summary>
    /// Retrieves the bounds of all dungeon segments.
    /// </summary>
    /// <returns>An array of bounds for each dungeon segment</returns>
    private Bounds[] RetrieveSegmentBounds()
    {
        return _dungeonSegments.Select(dungeonSegment => dungeonSegment.RetrieveBounds()).ToArray();
    }
}
