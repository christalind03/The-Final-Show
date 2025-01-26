using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

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
    [SerializeField] private GameObject[] _enemyPrefabs;

    private List<DungeonSegment> _connectableSegments;
    private List<DungeonSegment> _dungeonSegments;

    // TODO: Document
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

    // TODO: Document
    public override void OnStartClient()
    {
        UnityEngine.Random.InitState(_randomSeed);

        _connectableSegments = new List<DungeonSegment>();
        _dungeonSegments = new List<DungeonSegment>();

        GenerateDungeon();
        GenerateNavigationMesh();
        SpawnEnemies();
    }

    // TODO: Document
    public override void OnStartServer()
    {
        _randomSeed = (int)DateTime.Now.Ticks;
    }
    
    // TODO: Document
    private void GenerateDungeon()
    {
        int totalRooms = UnityEngine.Random.Range(_minimumRooms, _maximumRooms + 1); // Since the maximum parameter is exclusive, ensure we add by one to make it inclusive
        int exitIndex = UnityEngine.Random.Range(1, totalRooms); // Entrance occupies the 0th index
        int roomIndex = 0;

        while (roomIndex < totalRooms)
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

    // TODO: Document
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

    // TODO: Document
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

    // TODO: Document
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

    // TODO: Document
    private void BlockEntrances()
    {
        foreach (DungeonSegment dungeonSegment in _connectableSegments)
        {
            dungeonSegment.BlockEntrances();
        }
    }

    // TODO: Document
    private void GenerateNavigationMesh()
    {
        gameObject.GetComponent<NavMeshSurface>().BuildNavMesh();

        // Ensure the entrance room is added back to the scene hierarchy to maintain organization
        _dungeonSegments[0].transform.SetParent(transform);
    }

    // TODO: Document
    private void SpawnEnemies()
    {
        if (!isServer) { return; }

        // Retrieve the dungeon bounds
        Bounds[] segmentBounds = _dungeonSegments.Select(dungeonSegment => dungeonSegment.RetrieveBounds()).ToArray();
        Bounds dungeonBounds = UnityUtils.CalculateBounds(segmentBounds);
        Bounds entranceBounds = _dungeonSegments[0].RetrieveBounds();

        int totalEnemies = UnityEngine.Random.Range(_minimumEnemies, _maximumEnemies + 1);
        int enemyIndex = 0;

        while (enemyIndex < totalEnemies)
        {
            Vector3 startPosition = UnityUtils.RandomizePosition(dungeonBounds);

            if (NavMesh.SamplePosition(startPosition, out NavMeshHit closestHit, 1f, NavMesh.AllAreas))
            {
                // Prevent enemies from spawning inside the entrance room
                if (entranceBounds.Contains(closestHit.position)) { continue; }

                int prefabIndex = UnityEngine.Random.Range(0, _enemyPrefabs.Length);
                GameObject enemyPrefab = Instantiate(_enemyPrefabs[prefabIndex]);

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
}
