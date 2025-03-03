using UnityEngine;

[CreateAssetMenu(fileName = "New Gameplay Theme", menuName = "Gameplay Theme")]
public class GameplayTheme : ScriptableObject
{
    public string Theme;
    public GameObject[] EnemyPrefabs;
    public GameObject BossPrefab;
}
