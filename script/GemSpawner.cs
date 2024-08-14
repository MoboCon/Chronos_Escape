using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemSpawner : MonoBehaviour
{
    public static GemSpawner Instance { get; private set; }

    public GameObject gemPrefab;
    public float spawnInterval = 5f; // Adjust the interval as needed
    public float spawnRadius = 10f;
    public int maxGems = 10;

    private List<GameObject> gems = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(SpawnGemsRoutine());
    }

    private IEnumerator SpawnGemsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (GameManager.Instance.IsGameActive() && gems.Count < maxGems)
            {
                SpawnGem();
            }
        }
    }

    private void SpawnGem()
    {
        Vector3 spawnPosition = GameManager.Instance.GetPlayerInstance().transform.position + Random.insideUnitSphere * spawnRadius;
        spawnPosition.y = 1f; // Ensure gems are spawned at a fixed height

        GameObject gem = Instantiate(gemPrefab, spawnPosition, Quaternion.identity);
        gems.Add(gem);
    }

    public void RemoveGem(GameObject gem)
    {
        if (gems.Contains(gem))
        {
            gems.Remove(gem);
            Destroy(gem);
        }
    }

    public void ClearAllGems()
    {
        foreach (GameObject gem in gems)
        {
            if (gem != null)
            {
                Destroy(gem);
            }
        }
        gems.Clear();
    }
}
