using Mirror;
using UnityEngine;

/// <summary>
/// Spawns randomized items from an array at a spawn point
/// </summary>
public class ItemSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject[] itemPrefabs; // List of item prefabs
    [SerializeField] private Transform spawnPoint; // Where the item should appear

    /// <summary>
    /// Spawns items
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();

        if (itemPrefabs.Length == 0)
        {
            Debug.LogWarning("ItemSpawner: No items assigned!");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("ItemSpawner: Spawn point not set!");
            return;
        }

        SpawnRandomItem();
    }

    /// <summary>
    /// Spawns a random item from the list of item prefabs at the spawn point
    /// </summary>
    [Server]
    private void SpawnRandomItem()
    {
        int randomIndex = Random.Range(0, itemPrefabs.Length);
        GameObject spawnedItem = Instantiate(itemPrefabs[randomIndex], spawnPoint.position, spawnPoint.rotation);

        if (spawnedItem.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity))
        {
            NetworkServer.Spawn(spawnedItem);
            Debug.Log($"ItemSpawner: Spawned {spawnedItem.name} at {spawnPoint.position}");
        }
        else
        {
            Debug.LogError($"ItemSpawner: {spawnedItem.name} is missing a NetworkIdentity component!");
        }
    }
}

