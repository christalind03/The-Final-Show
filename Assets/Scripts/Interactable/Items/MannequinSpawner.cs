using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
public class MannequinSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject[] mannequinPrefabs;
    [SerializeField] private bool spawnOnStart = true;

    public override void OnStartServer()
    {
        if (mannequinPrefabs == null || mannequinPrefabs.Length == 0)
        {
            Debug.LogError("No mannequin prefabs assigned!");
            return;
        }

        // Register the spawn handler
        NetworkClient.RegisterSpawnHandler(GetComponent<NetworkIdentity>().assetId, SpawnMannequin, UnSpawnMannequin);

        if (spawnOnStart)
        {
            SpawnRandomMannequin();
        }
    }

    [Server]
    public void SpawnRandomMannequin()
    {
        if (mannequinPrefabs.Length > 0)
        {
            GameObject randomMannequin = mannequinPrefabs[Random.Range(0, mannequinPrefabs.Length)];
            GameObject spawnedMannequin = Instantiate(randomMannequin, transform.position, transform.rotation);
            
            // Get all NetworkIdentity components in the spawned mannequin and its children
            NetworkIdentity[] identities = spawnedMannequin.GetComponentsInChildren<NetworkIdentity>();
            
            // Spawn all networked objects
            foreach (NetworkIdentity identity in identities)
            {
                NetworkServer.Spawn(identity.gameObject);
            }
            
            Debug.Log($"Spawned mannequin with {identities.Length} networked objects at {transform.position}");
        }
    }

    private GameObject SpawnMannequin(SpawnMessage msg)
    {
        if (mannequinPrefabs == null || mannequinPrefabs.Length == 0)
        {
            Debug.LogError("No mannequin prefabs assigned!");
            return null;
        }

        // Choose a random mannequin
        GameObject randomMannequin = mannequinPrefabs[Random.Range(0, mannequinPrefabs.Length)];
        GameObject spawnedMannequin = Instantiate(randomMannequin, msg.position, msg.rotation);

        // Spawn all networked objects in the mannequin
        NetworkIdentity[] identities = spawnedMannequin.GetComponentsInChildren<NetworkIdentity>();
        foreach (NetworkIdentity identity in identities)
        {
            NetworkServer.Spawn(identity.gameObject);
        }

        return spawnedMannequin;
    }

    private void UnSpawnMannequin(GameObject spawned)
    {
        Destroy(spawned);
    }
} 