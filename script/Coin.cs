using UnityEngine;

public class Coin : MonoBehaviour
{
    public int coinValue = 1; // Value of the coin
    public GameObject coinCollectEffectPrefab; // Reference to the coin collect effect prefab

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Add the coin value to the total coin count in GameManager
            GameManager.Instance.AddCoin(coinValue);

            // Instantiate the coin collect effect
            if (coinCollectEffectPrefab != null)
            {
                GameObject effect = Instantiate(coinCollectEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 0.5f); // Destroy the effect after 0.5 seconds
            }

            // Remove the coin from the CoinSpawner list and destroy the object
            CoinSpawner.Instance.RemoveCoin(gameObject);
        }
    }
}
