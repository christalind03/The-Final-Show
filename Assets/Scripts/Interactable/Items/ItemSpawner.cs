using UnityEngine;
using Mirror;
using System.Collections;

/// <summary>
/// Spawning of items -- instantiating and networking items at specified spawn points.
/// </summary>
public class ItemSpawner : NetworkBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Array of item prefabs that can be spawned. Each prefab must have a NetworkIdentity component.")]
    [SerializeField] private GameObject[] skinnedItemPrefabs;
    
    [Tooltip("Array of transforms representing spawn points where items will be instantiated.")]
    [SerializeField] private Transform[] spawnPoints;
    
    [Tooltip("If true, items will be spawned automatically when the server starts.")]
    [SerializeField] private bool spawnOnStart = true;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        Debug.Log("ItemSpawner Awake");
    }

    /// <summary>
    /// Called on the frame when the script is enabled just before any of the Update methods are called the first time.
    /// </summary>
    private void Start()
    {
        Debug.Log("ItemSpawner Start");
    }

    /// <summary>
    /// Called on the server when the object is spawned.
    /// Initializes the spawner and starts the spawning process if spawnOnStart is true.
    /// </summary>
    public override void OnStartServer()
    {
        Debug.Log("ItemSpawner OnStartServer");
        
        if (skinnedItemPrefabs == null || skinnedItemPrefabs.Length == 0)
        {
            Debug.LogError("No skinned item prefabs assigned!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return;
        }

        // Wait for the network to be ready
        StartCoroutine(WaitForNetworkReady());
    }

    /// <summary>
    /// Waits for the network to be ready before spawning items.
    /// </summary>
    private IEnumerator WaitForNetworkReady()
    {
        // Wait until the network is ready
        while (!NetworkServer.active)
        {
            yield return null;
        }

        if (spawnOnStart)
        {
            Debug.Log("Network is ready, attempting to spawn items");
            SpawnItems();
        }
    }

    /// <summary>
    /// Spawns items at each spawn point -- called on the server and will spawn a random item from the itemPrefabs array at each spawn point.
    /// </summary>
    [Server]
    public void SpawnItems()
    {
        Debug.Log($"ItemSpawner SpawnItems - Prefab count: {skinnedItemPrefabs.Length}, Spawn points: {spawnPoints.Length}");
        
        foreach (Transform spawnPoint in spawnPoints)
        {
            foreach (GameObject itemPrefab in skinnedItemPrefabs)
            {
                GameObject spawnedItem = Instantiate(itemPrefab, spawnPoint.position, spawnPoint.rotation);
                NetworkServer.Spawn(spawnedItem);
            }
        }
    }

    /// <summary>
    /// Called on clients when the object is spawned.
    /// </summary>
    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("ItemSpawner OnStartClient");
    }
} 