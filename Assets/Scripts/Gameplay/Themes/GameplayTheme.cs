using UnityEngine;

[CreateAssetMenu(fileName = "New Gameplay Theme", menuName = "Gameplay Theme")]
public class GameplayTheme : ScriptableObject
{
    public string Theme;

    [Header("Enemy Prefabs")]
    public GameObject[] EnemyPrefabs;
    public GameObject BossPrefab;

    [Header("Boss Room Prefab")]
    public GameObject BossRoomPrefab;

    [Header("Defeat Room Prefab")]
    public GameObject DefeatRoomPrefab;

    [Header("Dungeon Prefabs")]
    public GameObject[] EntrancePrefabs;
    public GameObject[] ExitPrefabs;
    public GameObject[] HallwayPrefabs;
    public GameObject[] RoomPrefabs;
}
