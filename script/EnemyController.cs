using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float reducedSpeed = 1.5f; // Speed when turning
    public float speedRecoveryTime = 1f; // Time to recover to full speed
    public float turnSpeed = 0.1f; // Speed of turning
    public int damage = 1;
    public int maxHealth = 1;
    public float obstacleAvoidanceDistance = 5f; // Distance to detect obstacles
    public float obstacleAvoidanceForce = 1f; // Force to avoid obstacles
    public float closeProximityDistance = 2f; // Distance to check for close proximity to other enemies

    private int currentHealth;
    private Transform playerTransform;

    public GameObject hitEffectPrefab;
    public GameObject deathEffectPrefab;

    private bool isActive = false;
    private bool isTurning = false;
    private float currentSpeed;
    private bool isFrozen = false;

    private void Start()
    {
        currentHealth = maxHealth;
        currentSpeed = moveSpeed;
    }

    private void Update()
    {
        if (isActive && playerTransform != null && !isFrozen)
        {
            FollowPlayer();
            CheckProximityToOtherEnemies();
        }
    }

    public void Initialize(Transform player)
    {
        playerTransform = player;
        isActive = true;
    }

    public void Freeze(bool freeze)
    {
        isFrozen = freeze;
        if (isFrozen)
        {
            currentSpeed = 0f;
        }
        else
        {
            currentSpeed = moveSpeed;
        }
    }

    private void FollowPlayer()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        Vector3 avoidanceDirection = AvoidObstacles(direction);
        transform.position += avoidanceDirection * currentSpeed * Time.deltaTime;

        if (!isTurning)
        {
            StartCoroutine(TurnAndSlowDown(avoidanceDirection));
        }

        transform.LookAt(playerTransform);
    }

    private Vector3 AvoidObstacles(Vector3 direction)
    {
        RaycastHit hit;
        Vector3 newDirection = direction;

        if (Physics.Raycast(transform.position, transform.forward, out hit, obstacleAvoidanceDistance))
        {
            if (hit.collider.CompareTag("Obstacle"))
            {
                Vector3 hitNormal = hit.normal;
                hitNormal.y = 0.0f;
                newDirection = (direction + hitNormal * obstacleAvoidanceForce).normalized;
            }
        }

        return newDirection;
    }

    private IEnumerator TurnAndSlowDown(Vector3 direction)
    {
        isTurning = true;
        currentSpeed = reducedSpeed;

        float angle = Vector3.Angle(transform.forward, direction);
        while (angle > 5f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), turnSpeed * Time.deltaTime);
            angle = Vector3.Angle(transform.forward, direction);
            yield return null;
        }

        currentSpeed = moveSpeed;
        isTurning = false;
    }

    private void CheckProximityToOtherEnemies()
    {
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        foreach (EnemyController enemy in enemies)
        {
            if (enemy != this)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closeProximityDistance)
                {
                    enemy.TakeDamage(enemy.maxHealth);
                    TakeDamage(maxHealth);
                }
            }
        }
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
            PlayerController player = collisionObject.GetComponent<PlayerController>();
            if (player != null)
            {
                Debug.Log("Enemy collided with player");
                player.TakeDamage(damage, true); // True indicates it's an enemy
                TakeDamage(maxHealth); // Ensure enemy takes damage
            }
        }
        else if (collisionObject.CompareTag("Obstacle"))
        {
            Obstacle obstacle = collisionObject.GetComponent<Obstacle>();
            if (obstacle != null)
            {
                Debug.Log("Enemy collided with obstacle");
                TakeDamage(obstacle.damage);
            }
            else
            {
                // If the obstacle doesn't have an Obstacle component, still apply damage
                TakeDamage(damage);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Enemy took {damage} damage, current health: {currentHealth}");

        if (hitEffectPrefab != null)
        {
            var hitEffect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(hitEffect, 2f); // Ensure the hit effect is destroyed after 2 seconds
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Enemy died");
        if (deathEffectPrefab != null)
        {
            var deathEffect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            Destroy(deathEffect, 2f); // Ensure the death effect is destroyed after 2 seconds
        }

        Destroy(gameObject);
    }

    public void UpdateSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
        currentSpeed = newSpeed;
    }
}
