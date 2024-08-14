using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public int damage = 1;

    public void TriggerHitEffect()
    {
        // Implement hit effect logic here
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleCollision(other.gameObject);
    }

    private void HandleCollision(GameObject collisionObject)
    {
        if (collisionObject.CompareTag("Player"))
        {
            PlayerController playerController = collisionObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                TriggerHitEffect();
                playerController.TakeDamage(damage, false); // False indicates it's an obstacle
            }
        }
        else if (collisionObject.CompareTag("Enemy"))
        {
            EnemyController enemyController = collisionObject.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                TriggerHitEffect();
                enemyController.TakeDamage(damage);
            }
        }
    }
}
