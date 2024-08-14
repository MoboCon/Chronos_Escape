using UnityEngine;

public class Gem : MonoBehaviour
{
    public int gemValue = 1; // Value of the gem
    public GameObject gemCollectEffectPrefab; // Reference to the gem collect effect prefab

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Add the gem value to the total gem count in GemManager
            GemManager.Instance.AddGems(gemValue);

            // Instantiate the gem collect effect
            if (gemCollectEffectPrefab != null)
            {
                GameObject effect = Instantiate(gemCollectEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 0.5f); // Destroy the effect after 0.5 seconds
            }

            // Remove the gem from the GemSpawner list and destroy the object
            GemSpawner.Instance.RemoveGem(gameObject);
        }
    }
}
