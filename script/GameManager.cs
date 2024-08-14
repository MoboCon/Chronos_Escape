using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }

    public List<GameObject> playerPrefabs;
    public List<Transform> spawnPoints;

    public CoinSpawner coinSpawner;
    public GemSpawner gemSpawner; // Reference to GemSpawner

    public Camera mainMenuCamera;
    public Camera gameCamera;

    public List<GameObject> terrainPrefabs;
    public List<Sprite> terrainImages;
    public int[] terrainPrices;
    public bool[] terrainLockedStatus;
    public Transform terrainSpawnPoint;

    public List<GameObject> powerUpPrefabs;

    public float enemySpawnRadius = 10f;
    public float enemySpawnInterval = 5f;
    public float powerUpSpawnRadius = 10f;
    public float powerUpSpawnInterval = 10f;
    public float spawnRadius = 5f;
    public float spawnInterval = 5f;
    public float minSpawnDistanceFromPlayer = 2f;

    private CameraControl cameraControl;
    private bool isGamePaused = false;
    private bool isGameActive = false;
    private bool isPowerUpActive = false;
    private Coroutine activePowerUpCoroutine;

    private float gameTime = 0f;
    private float elapsedTimeForSpeedIncrease = 0f;
    private int selectedPlayerIndex = 0;
    private int coinCount = 0;
    private int continuePanelCounter = 0;
    private int scoreMultiplier = 1;
    private int currentTerrainIndex = 0;

    private float enemySpeedIncreaseInterval = 50f;
    private float enemySpeedIncreaseAmount = 0.5f;
    private float maxEnemySpeed = 9f;
    private float currentEnemySpeed = 3f;

    private int selectedTerrainIndex = -1;
    private GameObject currentTerrainInstance;
    private int defaultTerrainIndex = 0;

    private GameObject playerInstance;
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private List<GameObject> spawnedPowerUps = new List<GameObject>();
    private bool doubleCoinsActive = false;
    private int storedScore = 0;

    // Separate enemy lists for each terrain
    public List<GameObject> enemyPrefabsForTerrain1;
    public List<GameObject> enemyPrefabsForTerrain2;
    public List<GameObject> enemyPrefabsForTerrain3;
    // Add more lists for additional terrains if needed

    public int CoinCount
    {
        get { return coinCount; }
        private set
        {
            coinCount = value;
            UIManager.Instance.UpdateCoinText(coinCount);
        }
    }

    public int GemCount
    {
        get { return GemManager.Instance.GetGemsCount(); }
    }

    public bool SpendGems(int amount)
    {
        return GemManager.Instance.SpendGems(amount);
    }

    public bool AddGems(int amount)
    {
        GemManager.Instance.AddGems(amount);
        return true;
    }

    public bool AddCoins(int amount)
    {
        CoinCount += amount;
        return true;
    }

    public bool SpendCoins(int amount)
    {
        if (coinCount >= amount)
        {
            CoinCount -= amount;
            return true;
        }
        return false;
    }

    public bool IsGameActive()
    {
        return isGameActive;
    }

    public bool IsPowerUpActive()
    {
        return isPowerUpActive;
    }

    public GameObject GetPlayerInstance()
    {
        return playerInstance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        cameraControl = FindObjectOfType<CameraControl>();
        if (cameraControl != null)
        {
            cameraControl.enabled = false;
        }
        else
        {
            Debug.LogError("CameraControl not found in the scene!");
        }
    }

    private void Start()
    {
        InitializeCoinCount();
        InitializeGemsCount();
        UpdateTerrainUI();

        if (selectedTerrainIndex == -1)
        {
            selectedTerrainIndex = defaultTerrainIndex;
        }
        SpawnSelectedTerrain();
    }

    private void InitializeCoinCount()
    {
        coinCount = PlayerPrefs.GetInt("CoinCount", 0);
        UIManager.Instance.UpdateCoinText(coinCount);
    }

    private void InitializeGemsCount()
    {
        GemManager.Instance.LoadGems();
    }

    private void Update()
    {
        if (!isGamePaused && isGameActive)
        {
            gameTime += Time.deltaTime * scoreMultiplier;
            elapsedTimeForSpeedIncrease += Time.deltaTime;
            UIManager.Instance.scoreText.text = "Score: " + Mathf.FloorToInt(gameTime).ToString();
            HandleEnemySpeedIncrease();
        }
    }

    private void HandleEnemySpeedIncrease()
    {
        if (elapsedTimeForSpeedIncrease >= enemySpeedIncreaseInterval && currentEnemySpeed < maxEnemySpeed)
        {
            currentEnemySpeed = Mathf.Min(currentEnemySpeed + enemySpeedIncreaseAmount, maxEnemySpeed);
            elapsedTimeForSpeedIncrease = 0;

            foreach (var enemy in spawnedEnemies)
            {
                if (enemy != null)
                {
                    var enemyController = enemy.GetComponent<EnemyController>();
                    if (enemyController != null)
                    {
                        enemyController.UpdateSpeed(currentEnemySpeed);
                    }
                }
            }
        }
    }

    private IEnumerator SpawnEnemies()
    {
        while (!isGamePaused)
        {
            yield return new WaitForSeconds(enemySpawnInterval);
            if (!isGamePaused && playerInstance != null)
            {
                SpawnEnemyNearPlayer();
            }
        }
    }

    private IEnumerator SpawnPowerUps()
    {
        while (!isGamePaused)
        {
            yield return new WaitForSeconds(powerUpSpawnInterval);
            if (!isGamePaused && playerInstance != null)
            {
                SpawnPowerUpNearPlayer();
            }
        }
    }

    public void StartGame()
    {
        ResetGame();

        UIManager.Instance.startButton.gameObject.SetActive(false);

        UIManager.Instance.mainMenuCanvas.gameObject.SetActive(false);
        UIManager.Instance.gameCanvas.gameObject.SetActive(true);
        UIManager.Instance.gameOverCanvas.gameObject.SetActive(false);
        UIManager.Instance.levelCanvas.gameObject.SetActive(false);

        if (gameCamera != null)
        {
            gameCamera.gameObject.SetActive(true);
            mainMenuCamera.gameObject.SetActive(false);
        }

        if (selectedTerrainIndex == -1)
        {
            selectedTerrainIndex = defaultTerrainIndex;
        }
        SpawnSelectedTerrain();

        if (playerInstance == null)
        {
            SpawnPlayer();
        }

        if (cameraControl != null)
        {
            cameraControl.enabled = true;
            cameraControl.SetTarget(playerInstance.transform);
        }

        isGamePaused = false;
        isGameActive = true;
        Time.timeScale = 1f;
        StartCoroutine(SpawnEnemies());
        StartCoroutine(SpawnPowerUps());
    }

    public void GameOver()
    {
        isGamePaused = true;
        isGameActive = false;
        storedScore = Mathf.FloorToInt(gameTime);
        ClearAllSpawnedObjects();
        StartCoroutine(ShowGameOverCanvasWithDelay(1f));
    }

    private IEnumerator ShowGameOverCanvasWithDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        Time.timeScale = 0f;

        int currentScore = Mathf.FloorToInt(gameTime);
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);

        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            PlayerPrefs.SetInt("BestScore", bestScore);
        }

        UIManager.Instance.bestScoreText.text = "Best Score: " + bestScore;
        UIManager.Instance.currentScoreText.text = "Your Score: " + currentScore;

        UIManager.Instance.gameCanvas.gameObject.SetActive(false);
        UIManager.Instance.gameOverCanvas.gameObject.SetActive(true);

        if (cameraControl != null)
        {
            cameraControl.enabled = false;
        }

        if (continuePanelCounter < 2)
        {
            UIManager.Instance.ShowPlaySceneContinuePanel();
            continuePanelCounter++;
        }
        else
        {
            UIManager.Instance.HidePlaySceneContinuePanel();
            continuePanelCounter = 0;
        }
    }

    private void ResetGame()
    {
        gameTime = 0f;
        elapsedTimeForSpeedIncrease = 0f;
        isGamePaused = false;
        isPowerUpActive = false;
        scoreMultiplier = 1;
        currentEnemySpeed = 3f;

        if (activePowerUpCoroutine != null)
        {
            StopCoroutine(activePowerUpCoroutine);
        }

        if (playerInstance != null)
        {
            Destroy(playerInstance);
        }

        if (currentTerrainInstance != null)
        {
            Destroy(currentTerrainInstance);
        }

        ClearAllSpawnedObjects();

        if (cameraControl != null)
        {
            cameraControl.enabled = true;
        }

        Time.timeScale = 1f;

        SpawnSelectedTerrain();
    }

    public void BackToMainMenu()
    {
        UIManager.Instance.gameOverCanvas.gameObject.SetActive(false);
        UIManager.Instance.mainMenuCanvas.gameObject.SetActive(true);
        UIManager.Instance.levelCanvas.gameObject.SetActive(false);

        if (playerInstance != null)
        {
            Destroy(playerInstance);
        }

        if (currentTerrainInstance != null)
        {
            Destroy(currentTerrainInstance);
        }

        ClearAllSpawnedObjects();

        if (gameCamera != null)
        {
            gameCamera.gameObject.SetActive(false);
            mainMenuCamera.gameObject.SetActive(true);
        }

        UIManager.Instance.startButton.gameObject.SetActive(true);

        Time.timeScale = 1f;

        SpawnSelectedTerrain();
    }

    public void GiveUpGame()
    {
        isGamePaused = false;
        isGameActive = false;
        Time.timeScale = 1f;
        UIManager.Instance.mainMenuCanvas.gameObject.SetActive(true);
        UIManager.Instance.gameCanvas.gameObject.SetActive(false);
        UIManager.Instance.pauseCanvas.gameObject.SetActive(false);
        UIManager.Instance.levelCanvas.gameObject.SetActive(false);
        Destroy(playerInstance);

        if (currentTerrainInstance != null)
        {
            Destroy(currentTerrainInstance);
        }

        ClearAllSpawnedObjects();

        if (gameCamera != null)
        {
            gameCamera.gameObject.SetActive(false);
            mainMenuCamera.gameObject.SetActive(true);
        }

        UIManager.Instance.startButton.gameObject.SetActive(true);

        StopCoroutine(SpawnEnemies());
        StopCoroutine(SpawnPowerUps());

        SpawnSelectedTerrain();
    }

    public void PauseGame()
    {
        isGamePaused = true;
        Time.timeScale = 0f;
        UIManager.Instance.gameCanvas.gameObject.SetActive(false);
        UIManager.Instance.pauseCanvas.gameObject.SetActive(true);
    }

    public void ContinueGame()
    {
        isGamePaused = false;
        Time.timeScale = 1f;
        UIManager.Instance.gameCanvas.gameObject.SetActive(true);
        UIManager.Instance.pauseCanvas.gameObject.SetActive(false);
    }

    public void ContinueAfterAd()
    {
        UIManager.Instance.HidePlaySceneContinuePanel();
        RestoreGameAfterContinue();
    }

    public void ContinueAfterGems()
    {
        if (GemManager.Instance.SpendGems(10))
        {
            UIManager.Instance.HidePlaySceneContinuePanel();
            RestoreGameAfterContinue();
        }
        else
        {
            Debug.Log("Not enough gems to continue.");
        }
    }

    private void RestoreGameAfterContinue()
    {
        if (spawnPoints.Count > 0)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            playerInstance.transform.position = spawnPoint.position;
            playerInstance.transform.rotation = spawnPoint.rotation;
        }

        gameTime = storedScore;

        if (playerInstance != null)
        {
            PlayerController playerController = playerInstance.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = true;
                playerController.ResetHealth();
            }
        }

        if (cameraControl != null)
        {
            cameraControl.enabled = true;
            cameraControl.SetTarget(playerInstance.transform);
        }

        isGamePaused = false;
        isGameActive = true;
        Time.timeScale = 1f;
        UIManager.Instance.gameCanvas.gameObject.SetActive(true);
        UIManager.Instance.gameOverCanvas.gameObject.SetActive(false);

        StartCoroutine(SpawnEnemies());
        StartCoroutine(SpawnPowerUps());
    }

    public void OpenShop()
    {
        UIManager.Instance.OpenShop();
    }

    public void CloseShop()
    {
        UIManager.Instance.CloseShop();
    }

    public void SetSelectedPlayer(int index)
    {
        if (index >= 0 && index < playerPrefabs.Count)
        {
            selectedPlayerIndex = index;
            Debug.Log("Selected player index set to: " + selectedPlayerIndex);
        }
        else
        {
            Debug.LogError("Invalid player index: " + index);
        }
    }

    public void OpenLevelCanvas()
    {
        UIManager.Instance.mainMenuCanvas.gameObject.SetActive(false);
        UIManager.Instance.levelCanvas.gameObject.SetActive(true);
        UpdateTerrainUI();
    }

    public void BackToLevelMenu()
    {
        UIManager.Instance.levelCanvas.gameObject.SetActive(false);
        UIManager.Instance.mainMenuCanvas.gameObject.SetActive(true);
    }

    public void UpdateTerrainUI()
    {
        if (terrainPrefabs == null || terrainPrefabs.Count == 0 || currentTerrainIndex < 0 || currentTerrainIndex >= terrainPrefabs.Count)
        {
            Debug.LogError("Terrain prefabs list is not properly initialized or currentTerrainIndex is out of range.");
            return;
        }

        if (UIManager.Instance.terrainNameText == null || UIManager.Instance.terrainPriceText == null || UIManager.Instance.terrainImage == null)
        {
            Debug.LogError("Terrain UI components are not assigned.");
            return;
        }

        UIManager.Instance.coinTextLevelCanvas.text = CoinCount.ToString();

        if (terrainLockedStatus[currentTerrainIndex])
        {
            UIManager.Instance.buyTerrainButton.gameObject.SetActive(true);
            UIManager.Instance.selectTerrainButton.gameObject.SetActive(false);
        }
        else
        {
            UIManager.Instance.buyTerrainButton.gameObject.SetActive(false);
            UIManager.Instance.selectTerrainButton.gameObject.SetActive(true);
            UIManager.Instance.selectTerrainButton.GetComponentInChildren<Text>().text = (selectedTerrainIndex == currentTerrainIndex) ? "Selected" : "Select";
        }

        UIManager.Instance.terrainNameText.text = terrainPrefabs[currentTerrainIndex].name;
        UIManager.Instance.terrainPriceText.text = "Price: " + terrainPrices[currentTerrainIndex];

        if (terrainImages != null && currentTerrainIndex < terrainImages.Count)
        {
            UIManager.Instance.terrainImage.sprite = terrainImages[currentTerrainIndex];
        }

        UIManager.Instance.buyTerrainButton.interactable = CoinCount >= terrainPrices[currentTerrainIndex];
        UIManager.Instance.selectTerrainButton.interactable = !terrainLockedStatus[currentTerrainIndex];
    }

    public void BuyTerrain()
    {
        if (currentTerrainIndex < terrainLockedStatus.Length && CoinCount >= terrainPrices[currentTerrainIndex])
        {
            terrainLockedStatus[currentTerrainIndex] = false;
            AddCoin(-terrainPrices[currentTerrainIndex]);
            UpdateTerrainUI();

            // Update current terrain level if needed
            PlayerSelectionManager.Instance.currentTerrainLevel = currentTerrainIndex + 1;
            PlayerSelectionManager.Instance.UpdateUI();
        }
        else
        {
            Debug.LogError("Not enough coins to buy this terrain or invalid index.");
        }
    }

    public void SelectTerrain()
    {
        if (selectedTerrainIndex != currentTerrainIndex)
        {
            selectedTerrainIndex = currentTerrainIndex;
            SpawnSelectedTerrain();
            UpdateTerrainUI();
        }
    }

    public void NextTerrain()
    {
        currentTerrainIndex = (currentTerrainIndex + 1) % terrainPrefabs.Count;
        UpdateTerrainUI();
    }

    public void PreviousTerrain()
    {
        currentTerrainIndex = (currentTerrainIndex - 1 + terrainPrefabs.Count) % terrainPrefabs.Count;
        UpdateTerrainUI();
    }

    public void SpawnSelectedTerrain()
    {
        if (terrainPrefabs.Count == 0 || terrainSpawnPoint == null)
        {
            Debug.LogError("Terrain prefabs array or spawn point not set!");
            return;
        }

        if (currentTerrainInstance != null)
        {
            Destroy(currentTerrainInstance);
        }

        currentTerrainInstance = Instantiate(terrainPrefabs[selectedTerrainIndex], terrainSpawnPoint.position, terrainSpawnPoint.rotation);
        currentTerrainIndex = selectedTerrainIndex;
    }

    private void SpawnPlayer()
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogError("No spawn points set in GameManager");
            return;
        }

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        playerInstance = Instantiate(playerPrefabs[selectedPlayerIndex], spawnPoint.position, spawnPoint.rotation);

        PlayerController playerController = playerInstance.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.onPlayerDied += GameOver;
        }

        if (cameraControl != null)
        {
            cameraControl.SetTarget(playerInstance.transform);
        }

        NotifyEnemiesPlayerSpawned();
    }

    private void NotifyEnemiesPlayerSpawned()
    {
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        foreach (EnemyController enemy in enemies)
        {
            enemy.Initialize(playerInstance.transform);
        }
    }

    private void SpawnPowerUpNearPlayer()
    {
        if (playerInstance != null && powerUpPrefabs.Count > 0)
        {
            Vector3 spawnPosition;
            bool validSpawnPosition = false;
            int attempts = 0;
            int maxAttempts = 100;

            do
            {
                spawnPosition = playerInstance.transform.position + Random.insideUnitSphere * powerUpSpawnRadius;
                spawnPosition.y = 0.2f;
                attempts++;

                if (Vector3.Distance(spawnPosition, playerInstance.transform.position) >= minSpawnDistanceFromPlayer &&
                    !Physics.CheckSphere(spawnPosition, 1f, LayerMask.GetMask("Obstacle")))
                {
                    validSpawnPosition = true;
                }

            } while (!validSpawnPosition && attempts < maxAttempts);

            if (validSpawnPosition)
            {
                int randomIndex = Random.Range(0, powerUpPrefabs.Count);
                GameObject newPowerUp = Instantiate(powerUpPrefabs[randomIndex], spawnPosition, Quaternion.identity);
                newPowerUp.SetActive(true);
                spawnedPowerUps.Add(newPowerUp);
                Debug.Log("Power-up spawned at position: " + spawnPosition);
            }
        }
    }

    private void SpawnEnemyNearPlayer()
    {
        if (playerInstance != null)
        {
            List<GameObject> enemyPrefabs = GetEnemyPrefabsForCurrentTerrain();

            if (enemyPrefabs != null && enemyPrefabs.Count > 0)
            {
                Vector3 spawnPosition;
                bool validSpawnPosition = false;
                int attempts = 0;
                int maxAttempts = 100;

                do
                {
                    spawnPosition = playerInstance.transform.position + Random.insideUnitSphere * enemySpawnRadius;
                    spawnPosition.y = 0.2f;
                    attempts++;

                    if (Vector3.Distance(spawnPosition, playerInstance.transform.position) >= minSpawnDistanceFromPlayer &&
                        !Physics.CheckSphere(spawnPosition, 1f, LayerMask.GetMask("Obstacle")))
                    {
                        validSpawnPosition = true;
                    }

                } while (!validSpawnPosition && attempts < maxAttempts);

                if (validSpawnPosition)
                {
                    int randomIndex = Random.Range(0, enemyPrefabs.Count);
                    GameObject newEnemy = Instantiate(enemyPrefabs[randomIndex], spawnPosition, Quaternion.identity);
                    newEnemy.SetActive(true);

                    EnemyController enemyController = newEnemy.GetComponent<EnemyController>();
                    if (enemyController != null)
                    {
                        enemyController.Initialize(playerInstance.transform);
                        enemyController.UpdateSpeed(currentEnemySpeed);
                    }

                    spawnedEnemies.Add(newEnemy);
                }
            }
        }
    }

    private List<GameObject> GetEnemyPrefabsForCurrentTerrain()
    {
        switch (currentTerrainIndex)
        {
            case 0:
                return enemyPrefabsForTerrain1;
            case 1:
                return enemyPrefabsForTerrain2;
            case 2:
                return enemyPrefabsForTerrain3;
            // Add more cases for additional terrains if needed
            default:
                return null;
        }
    }

    private void ClearAllSpawnedObjects()
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        spawnedEnemies.Clear();

        coinSpawner.ClearAllCoins();
        gemSpawner.ClearAllGems(); // Clear all gems

        foreach (GameObject powerUp in spawnedPowerUps)
        {
            if (powerUp != null)
            {
                Destroy(powerUp);
            }
        }
        spawnedPowerUps.Clear();
    }

    public void AddCoin(int amount)
    {
        CoinCount += amount;
    }

    public void ActivatePowerUp(PowerUp.PowerUpType powerUpType, float duration, float speedMultiplier)
    {
        if (isPowerUpActive && activePowerUpCoroutine != null)
        {
            StopCoroutine(activePowerUpCoroutine);
        }

        isPowerUpActive = true;
        UIManager.Instance.powerUpText.text = $"{powerUpType} Activated! Time remaining: {duration:F1}s";
        UIManager.Instance.powerUpText.gameObject.SetActive(true);

        switch (powerUpType)
        {
            case PowerUp.PowerUpType.SpeedBoost:
                activePowerUpCoroutine = StartCoroutine(ApplySpeedBoost(powerUpType, duration, speedMultiplier));
                break;
            case PowerUp.PowerUpType.DoubleCoins:
                activePowerUpCoroutine = StartCoroutine(ApplyDoubleCoins(powerUpType, duration));
                break;
            case PowerUp.PowerUpType.Shield:
                activePowerUpCoroutine = StartCoroutine(ApplyShield(powerUpType, duration));
                break;
            case PowerUp.PowerUpType.MagnetCoin:
                activePowerUpCoroutine = StartCoroutine(ApplyMagnet(powerUpType, duration));
                break;
            case PowerUp.PowerUpType.FreezeEnemy:
                activePowerUpCoroutine = StartCoroutine(ApplyFreezeEnemy(powerUpType, duration));
                break;
            case PowerUp.PowerUpType.HealthBoost:
                activePowerUpCoroutine = StartCoroutine(ApplyHealthBoost(powerUpType, duration));
                break;
            case PowerUp.PowerUpType.ScoreMultiplier: // New power-up effect
                activePowerUpCoroutine = StartCoroutine(ApplyScoreMultiplier(powerUpType, duration));
                break;
            default:
                break;
        }
    }

    private IEnumerator UpdatePowerUpUI(PowerUp.PowerUpType powerUpType, float duration)
    {
        float remainingTime = duration;
        while (remainingTime > 0)
        {
            UIManager.Instance.powerUpText.text = $"{powerUpType} Active! Time remaining: {remainingTime:F1}s";
            remainingTime -= Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator ApplyScoreMultiplier(PowerUp.PowerUpType powerUpType, float duration)
    {
        scoreMultiplier = 2; // Double the score
        StartCoroutine(UpdatePowerUpUI(powerUpType, duration));
        yield return new WaitForSeconds(duration);
        scoreMultiplier = 1; // Reset the score multiplier
        EndPowerUp();
    }

    private IEnumerator ApplySpeedBoost(PowerUp.PowerUpType powerUpType, float duration, float speedMultiplier = 1.5f)
    {
        PlayerController playerController = playerInstance.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.moveSpeed *= speedMultiplier;
            playerController.rotateSpeed *= speedMultiplier;
        }
        StartCoroutine(UpdatePowerUpUI(powerUpType, duration));
        yield return new WaitForSeconds(duration);
        if (playerController != null)
        {
            playerController.moveSpeed /= speedMultiplier;
            playerController.rotateSpeed /= speedMultiplier; // Removed misplaced parenthesis here
        }
        EndPowerUp();
    }

    private IEnumerator ApplyDoubleCoins(PowerUp.PowerUpType powerUpType, float duration)
    {
        doubleCoinsActive = true;
        StartCoroutine(UpdatePowerUpUI(powerUpType, duration));
        yield return new WaitForSeconds(duration);
        doubleCoinsActive = false;
        EndPowerUp();
    }

    private IEnumerator ApplyShield(PowerUp.PowerUpType powerUpType, float duration)
    {
        PlayerController playerController = playerInstance.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.isShielded = true;
            playerController.EnableShieldEffect(true);
        }
        StartCoroutine(UpdatePowerUpUI(powerUpType, duration));
        yield return new WaitForSeconds(duration);
        if (playerController != null)
        {
            playerController.isShielded = false;
            playerController.EnableShieldEffect(false);
        }
        EndPowerUp();
    }

    private IEnumerator ApplyMagnet(PowerUp.PowerUpType powerUpType, float duration)
    {
        PlayerController playerController = playerInstance.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.isMagnetActive = true;
        }
        StartCoroutine(UpdatePowerUpUI(powerUpType, duration));
        yield return new WaitForSeconds(duration);
        if (playerController != null)
        {
            playerController.isMagnetActive = false;
        }
        EndPowerUp();
    }

    private IEnumerator ApplyFreezeEnemy(PowerUp.PowerUpType powerUpType, float duration)
    {
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        foreach (EnemyController enemy in enemies)
        {
            enemy.Freeze(true);
        }
        StartCoroutine(UpdatePowerUpUI(powerUpType, duration));
        yield return new WaitForSeconds(duration);
        foreach (EnemyController enemy in enemies)
        {
            enemy.Freeze(false);
        }
        EndPowerUp();
    }

    private IEnumerator ApplyHealthBoost(PowerUp.PowerUpType powerUpType, float duration)
    {
        PlayerController playerController = playerInstance.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.IncreaseHealth();
        }
        yield return null;
        EndPowerUp();
    }

    private void EndPowerUp()
    {
        isPowerUpActive = false;
        UIManager.Instance.powerUpText.gameObject.SetActive(false);
    }

    public void SetCurrentTerrain(int terrainIndex)
    {
        if (terrainIndex >= 0 && terrainIndex < terrainPrefabs.Count)
        {
            currentTerrainIndex = terrainIndex;
        }
        else
        {
            Debug.LogError("Invalid terrain index: " + terrainIndex);
        }
    }

    public void AddEnemyPrefabForTerrain(int terrainIndex, GameObject enemyPrefab)
    {
        switch (terrainIndex)
        {
            case 0:
                if (enemyPrefabsForTerrain1 == null)
                    enemyPrefabsForTerrain1 = new List<GameObject>();
                enemyPrefabsForTerrain1.Add(enemyPrefab);
                break;
            case 1:
                if (enemyPrefabsForTerrain2 == null)
                    enemyPrefabsForTerrain2 = new List<GameObject>();
                enemyPrefabsForTerrain2.Add(enemyPrefab);
                break;
            case 2:
                if (enemyPrefabsForTerrain3 == null)
                    enemyPrefabsForTerrain3 = new List<GameObject>();
                enemyPrefabsForTerrain3.Add(enemyPrefab);
                break;
            // Add more cases for additional terrains if needed
            default:
                Debug.LogError("Invalid terrain index: " + terrainIndex);
                break;
        }
    }
}
