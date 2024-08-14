using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float maxSpeed = 10f;
    public float acceleration = 2f;
    public float deceleration = 2f;
    public float rotateSpeed = 5f;
    public float rotationAngle = 90f;

    [Header("Health Settings")]
    public int maxHealth = 5;
    private int currentHealth;
    public GameObject healthObjectPrefab;
    private GameObject[] healthObjects;

    [Header("Effects")]
    public GameObject deathEffectPrefab;
    public GameObject trailEffectPrefab;
    public GameObject shieldEffectPrefab;

    private float currentSpeed;
    private Quaternion targetRotation;
    private Rigidbody rb;
    private GameManager gameManager;
    private GameObject trailEffectInstance;
    private GameObject shieldEffectInstance;
    private TrailRenderer trailRenderer;

    public System.Action onPlayerDied;

    public bool isShielded = false;
    public bool isMagnetActive = false;

    public int speedUpgradePrice = 100;
    public int rotationUpgradePrice = 150;
    public int healthUpgradePrice = 200;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        Debug.Assert(rb != null, "Rigidbody component is missing!");

        // Freeze position and rotation
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        currentSpeed = moveSpeed;
        targetRotation = transform.rotation;
        currentHealth = maxHealth;
        gameManager = FindObjectOfType<GameManager>();
        Debug.Assert(gameManager != null, "GameManager instance not found!");

        InitializeHealthObjects();
        UpdateHealthDisplay();

        if (trailEffectPrefab != null)
        {
            trailEffectInstance = Instantiate(trailEffectPrefab, transform.position, Quaternion.identity, transform);
            trailEffectInstance.transform.localPosition = Vector3.zero;
            trailRenderer = trailEffectInstance.GetComponent<TrailRenderer>();
            if (trailRenderer != null)
            {
                trailRenderer.transform.localPosition += new Vector3(0, 0.5f, 0); // Adjust the height of the trail renderer
                trailRenderer.enabled = true;  // Enable the trail renderer
            }
        }

        if (shieldEffectPrefab != null)
        {
            shieldEffectInstance = Instantiate(shieldEffectPrefab, transform.position, Quaternion.identity, transform);
            shieldEffectInstance.transform.localPosition = Vector3.zero;
            shieldEffectInstance.SetActive(false);
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();

        if (isMagnetActive)
        {
            AttractCoins();
        }
    }

    private void HandleMovement()
    {
        currentSpeed = Mathf.Clamp(currentSpeed + (acceleration * Time.deltaTime * (moveSpeed > currentSpeed ? 1 : -1)), moveSpeed, maxSpeed);
        rb.MovePosition(rb.position + transform.forward * currentSpeed * Time.deltaTime);
    }

    private void HandleRotation()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
            {
                targetRotation = Quaternion.Euler(transform.eulerAngles + Vector3.up * (touch.position.x < Screen.width / 2 ? -rotationAngle : rotationAngle));
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            targetRotation = Quaternion.Euler(transform.eulerAngles + Vector3.up * -rotationAngle);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            targetRotation = Quaternion.Euler(transform.eulerAngles + Vector3.up * rotationAngle);
        }

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Ignore all collisions to ensure the player is not moved by other objects
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Obstacle"))
        {
            // Handle damage or effects without moving the player
            if (collision.gameObject.CompareTag("Enemy"))
            {
                Obstacle obstacle = collision.gameObject.GetComponent<Obstacle>();
                if (obstacle != null)
                {
                    obstacle.TriggerHitEffect();
                    TakeDamage(obstacle.damage, true); // True indicates it's an enemy
                }
            }
            else if (collision.gameObject.CompareTag("Obstacle"))
            {
                Obstacle obstacle = collision.gameObject.GetComponent<Obstacle>();
                if (obstacle != null)
                {
                    obstacle.TriggerHitEffect();
                    TakeDamage(obstacle.damage, false); // False indicates it's an obstacle
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Coin"))
        {
            gameManager.AddCoin(1);
            Destroy(other.gameObject);
        }
        else if (other.gameObject.CompareTag("Gem"))
        {
            GemManager.Instance.AddGems(1);
            Destroy(other.gameObject);
        }
    }

    public void TakeDamage(int damage, bool isEnemy)
    {
        if (isShielded && isEnemy) return;

        currentHealth = Mathf.Max(currentHealth - damage, 0);
        UpdateHealthDisplay();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void InitializeHealthObjects()
    {
        if (healthObjects != null)
        {
            foreach (GameObject obj in healthObjects)
            {
                Destroy(obj);
            }
        }

        healthObjects = new GameObject[maxHealth];
        for (int i = 0; i < maxHealth; i++)
        {
            Vector3 healthPosition = transform.position + Vector3.up * 2 + Vector3.right * (i - maxHealth / 2.0f);
            healthObjects[i] = Instantiate(healthObjectPrefab, healthPosition, Quaternion.identity, transform);
        }
    }

    private void UpdateHealthDisplay()
    {
        for (int i = 0; i < healthObjects.Length; i++)
        {
            if (healthObjects[i] != null)
            {
                healthObjects[i].SetActive(i < currentHealth);
            }
        }
    }

    public void IncreaseHealth()
    {
        if (currentHealth < maxHealth)
        {
            currentHealth++;
            UpdateHealthDisplay();
        }
    }

    private void Die()
    {
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        onPlayerDied?.Invoke();
        enabled = false;
    }

    public void EnableShieldEffect(bool enable)
    {
        if (shieldEffectInstance != null)
        {
            shieldEffectInstance.SetActive(enable);
        }
    }

    private void AttractCoins()
    {
        Coin[] coins = FindObjectsOfType<Coin>();
        foreach (Coin coin in coins)
        {
            float distance = Vector3.Distance(transform.position, coin.transform.position);
            if (distance < 10f)
            {
                Vector3 direction = (transform.position - coin.transform.position).normalized;
                coin.transform.position = Vector3.MoveTowards(coin.transform.position, transform.position, 10f * Time.deltaTime);
            }
        }
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateHealthDisplay();
    }

    public int GetSpeedUpgradePrice()
    {
        return speedUpgradePrice;
    }

    public int GetRotationUpgradePrice()
    {
        return rotationUpgradePrice;
    }

    public int GetHealthUpgradePrice()
    {
        return healthUpgradePrice;
    }

    public void UpgradeSpeed()
    {
        if (moveSpeed < maxSpeed)
        {
            moveSpeed += 2f; // Adjust as needed
            maxSpeed += 2f;  // Adjust as needed
            // Update upgrade price or other properties if needed
            speedUpgradePrice += 50; // Increase price for next upgrade
        }
    }

    public void UpgradeRotation()
    {
        if (rotateSpeed < 6f)
        {
            rotateSpeed += 1f; // Adjust as needed
            // Update upgrade price or other properties if needed
            rotationUpgradePrice += 50; // Increase price for next upgrade
        }
    }

    public void UpgradeHealth()
    {
        if (maxHealth < 6)
        {
            maxHealth += 1; // Adjust as needed
            InitializeHealthObjects(); // Reinitialize health objects after upgrade
            healthUpgradePrice += 50; // Increase price for next upgrade
        }
    }
}
