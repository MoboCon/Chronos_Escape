using UnityEngine;

public class GameSetup : MonoBehaviour
{
    public GameObject enemyPrefab1;
    public GameObject enemyPrefab2;
    public GameObject enemyPrefab3;
    public GameObject enemyPrefab4;
    public GameObject enemyPrefab5;
    public GameObject enemyPrefab6;

    void Start()
    {
        // Example for Terrain 0
        GameManager.Instance.AddEnemyPrefabForTerrain(0, enemyPrefab1);
        GameManager.Instance.AddEnemyPrefabForTerrain(0, enemyPrefab2);

        // Example for Terrain 1
        GameManager.Instance.AddEnemyPrefabForTerrain(1, enemyPrefab3);
        GameManager.Instance.AddEnemyPrefabForTerrain(1, enemyPrefab4);

        // Example for Terrain 2
        GameManager.Instance.AddEnemyPrefabForTerrain(2, enemyPrefab5);
        GameManager.Instance.AddEnemyPrefabForTerrain(2, enemyPrefab6);

        // Add more enemy prefabs for additional terrains as needed
    }
}
