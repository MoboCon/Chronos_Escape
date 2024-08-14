using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    public static CoinSpawner Instance { get; private set; }

    public GameObject coinPrefab;
    public float spawnInterval = 3f;
    public float spawnRadius = 10f;
    public int maxCoins = 20;

    private List<GameObject> coins = new List<GameObject>();

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
        StartCoroutine(SpawnCoins());
    }

    private IEnumerator SpawnCoins()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (GameManager.Instance.IsGameActive() && coins.Count < maxCoins)
            {
                SpawnCoin();
            }
        }
    }

    private void SpawnCoin()
    {
        Vector3 spawnPosition = GameManager.Instance.GetPlayerInstance().transform.position + Random.insideUnitSphere * spawnRadius;
        spawnPosition.y = 1f; // Ensure coins are spawned at a fixed height

        GameObject coin = Instantiate(coinPrefab, spawnPosition, Quaternion.identity);
        coins.Add(coin);
    }

    public void RemoveCoin(GameObject coin)
    {
        if (coins.Contains(coin))
        {
            coins.Remove(coin);
            Destroy(coin);
        }
    }

    public void ClearAllCoins()
    {
        foreach (GameObject coin in coins)
        {
            if (coin != null)
            {
                Destroy(coin);
            }
        }
        coins.Clear();
    }
}
