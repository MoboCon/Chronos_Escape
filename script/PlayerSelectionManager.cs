using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSelectionManager : MonoBehaviour
{
    public static PlayerSelectionManager Instance;

    public Camera shopCamera;
    public Transform playerPreviewSpawnPoint;

    public GameObject[] playerPrefabs;
    public PlayerPrice[] playerPrices; // Reference to PlayerPrice class
    public bool[] playerLockedStatus;
    public int currentTerrainLevel; // Track the current terrain level

    public Text playerPriceText;
    public Text playerStatusText;
    public Text coinText;
    public Text gemText; // Added gem text for UI update
    public Button buyButton;
    public Button selectButton;
    public Button nextButton;
    public Button prevButton;
    public Button backButton;

    public Slider speedSlider;
    public Slider rotationSlider;
    public Slider healthSlider;
    public Button upgradeSpeedButton;
    public Button upgradeRotationButton;
    public Button upgradeHealthButton;

    public Text upgradeSpeedPriceText;
    public Text upgradeRotationPriceText;
    public Text upgradeHealthPriceText;

    public Text errorMessageText; // Error message text
    public Material blackMaterial;
    private Material originalMaterial;

    public float rotationSpeed = 20f;

    private GameObject currentPreview;
    private int selectedPlayerIndex = -1;
    private int currentIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        if (playerPreviewSpawnPoint == null)
        {
            Debug.LogError("Player preview spawn point not assigned in inspector!");
        }
    }

    private void Start()
    {
        UpdateUI();

        buyButton.onClick.AddListener(BuyPlayer);
        selectButton.onClick.AddListener(SelectPlayer);
        nextButton.onClick.AddListener(NextPlayer);
        prevButton.onClick.AddListener(PreviousPlayer);
        backButton.onClick.AddListener(BackToMenu);

        upgradeSpeedButton.onClick.AddListener(UpgradeSpeed);
        upgradeRotationButton.onClick.AddListener(UpgradeRotation);
        upgradeHealthButton.onClick.AddListener(UpgradeHealth);

        UpdateCoinText();
        UpdateGemText(); // Update gems text
    }

    private void Update()
    {
        RotatePlayerPreview();
    }

    public void UpdateUI()
    {
        PlayerPrice currentPlayerPrice = playerPrices[currentIndex];
        bool isTerrainUnlocked = currentPlayerPrice.requiredLevel <= currentTerrainLevel;

        if (playerLockedStatus[currentIndex])
        {
            if (isTerrainUnlocked)
            {
                playerPriceText.text = currentPlayerPrice.amount.ToString() + (currentPlayerPrice.isGems ? " Gems" : " Coins");
                playerStatusText.text = "Locked";
                buyButton.gameObject.SetActive(true);
                selectButton.gameObject.SetActive(false);
                upgradeSpeedButton.gameObject.SetActive(false);
                upgradeRotationButton.gameObject.SetActive(false);
                upgradeHealthButton.gameObject.SetActive(false);
            }
            else
            {
                playerPriceText.text = "Unlock terrain to buy";
                playerStatusText.text = "Locked";
                buyButton.gameObject.SetActive(false);
                selectButton.gameObject.SetActive(false);
                upgradeSpeedButton.gameObject.SetActive(false);
                upgradeRotationButton.gameObject.SetActive(false);
                upgradeHealthButton.gameObject.SetActive(false);
            }
        }
        else
        {
            playerPriceText.text = "";
            playerStatusText.text = "Unlocked";
            buyButton.gameObject.SetActive(false);
            selectButton.gameObject.SetActive(true);
            selectButton.GetComponentInChildren<Text>().text = (selectedPlayerIndex == currentIndex) ? "Selected" : "Select";
            upgradeSpeedButton.gameObject.SetActive(true);
            upgradeRotationButton.gameObject.SetActive(true);
            upgradeHealthButton.gameObject.SetActive(true);
        }

        bool canAfford = currentPlayerPrice.isGems
            ? GameManager.Instance.GemCount >= currentPlayerPrice.amount
            : GameManager.Instance.CoinCount >= currentPlayerPrice.amount;

        buyButton.interactable = canAfford && isTerrainUnlocked;
        selectButton.interactable = !playerLockedStatus[currentIndex] && isTerrainUnlocked;

        UpdateCoinText();
        UpdateGemText();
        SpawnPlayerPreview();
        UpdateStatsUI();
    }

    public void ActivateShopCamera(bool activate)
    {
        if (shopCamera != null)
        {
            shopCamera.gameObject.SetActive(activate);
        }
    }

    private void BuyPlayer()
    {
        PlayerPrice price = playerPrices[currentIndex];
        bool canAfford = price.isGems
            ? GameManager.Instance.GemCount >= price.amount
            : GameManager.Instance.CoinCount >= price.amount;

        if (price.requiredLevel <= currentTerrainLevel && canAfford)
        {
            playerLockedStatus[currentIndex] = false;
            if (price.isGems)
            {
                GameManager.Instance.SpendGems(price.amount);
            }
            else
            {
                GameManager.Instance.AddCoin(-price.amount);
            }
            UpdateUI();
        }
        else if (price.requiredLevel > currentTerrainLevel)
        {
            StartCoroutine(ShowErrorMessage("Unlock the terrain to buy this player."));
        }
        else
        {
            StartCoroutine(ShowErrorMessage("Not enough currency to buy this player."));
        }
    }

    private void SelectPlayer()
    {
        if (!playerLockedStatus[currentIndex])
        {
            selectedPlayerIndex = currentIndex;
            GameManager.Instance.SetSelectedPlayer(currentIndex);
            UpdateUI();
        }
    }

    private void NextPlayer()
    {
        currentIndex = (currentIndex + 1) % playerPrefabs.Length;
        UpdateUI();
    }

    private void PreviousPlayer()
    {
        currentIndex = (currentIndex - 1 + playerPrefabs.Length) % playerPrefabs.Length;
        UpdateUI();
    }

    private void BackToMenu()
    {
        UIManager.Instance.CloseShop();
    }

    private void UpdateCoinText()
    {
        coinText.text = "Coins: " + GameManager.Instance.CoinCount;
    }

    private void UpdateGemText()
    {
        gemText.text = "Gems: " + GameManager.Instance.GemCount;
    }

    public void SpawnPlayerPreview()
    {
        if (playerPrefabs.Length == 0 || playerPreviewSpawnPoint == null)
        {
            Debug.LogError("Player prefabs array or spawn point not set!");
            return;
        }

        if (currentPreview != null)
        {
            Destroy(currentPreview);
        }

        currentPreview = Instantiate(playerPrefabs[currentIndex], playerPreviewSpawnPoint.position, playerPreviewSpawnPoint.rotation, playerPreviewSpawnPoint);

        Rigidbody rb = currentPreview.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        PlayerController playerController = currentPreview.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        TrailRenderer trailRenderer = currentPreview.GetComponent<TrailRenderer>();
        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
        }

        // Apply black material if player is locked
        if (playerLockedStatus[currentIndex])
        {
            Renderer[] renderers = currentPreview.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                originalMaterial = renderer.material;  // Save the original material
                renderer.material = blackMaterial;  // Apply the black material
            }
        }
    }

    private void RotatePlayerPreview()
    {
        if (currentPreview != null)
        {
            currentPreview.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }

    public void RemovePlayerPreview()
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;
        }
    }

    private void UpdateStatsUI()
    {
        if (playerPrefabs.Length == 0) return;

        PlayerController playerController = playerPrefabs[currentIndex].GetComponent<PlayerController>();
        if (playerController != null)
        {
            speedSlider.maxValue = playerController.maxSpeed;
            speedSlider.value = playerController.moveSpeed;
            rotationSlider.maxValue = 360f; // Assuming 360 degrees as the max rotation value
            rotationSlider.value = playerController.rotateSpeed;
            healthSlider.maxValue = playerController.maxHealth;
            healthSlider.value = playerController.maxHealth; // This can also be set to playerController.currentHealth if available

            upgradeSpeedPriceText.text = playerController.GetSpeedUpgradePrice().ToString();
            upgradeRotationPriceText.text = playerController.GetRotationUpgradePrice().ToString();
            upgradeHealthPriceText.text = playerController.GetHealthUpgradePrice().ToString();
        }
    }

    private void UpgradeSpeed()
    {
        PlayerController playerController = playerPrefabs[currentIndex].GetComponent<PlayerController>();
        if (playerController != null)
        {
            int price = playerController.GetSpeedUpgradePrice();
            if (GameManager.Instance.CoinCount >= price)
            {
                GameManager.Instance.AddCoin(-price);
                playerController.UpgradeSpeed();
                UpdateUI();
            }
            else
            {
                StartCoroutine(ShowErrorMessage("Not enough coins to upgrade speed."));
            }
        }
    }

    private void UpgradeRotation()
    {
        PlayerController playerController = playerPrefabs[currentIndex].GetComponent<PlayerController>();
        if (playerController != null)
        {
            int price = playerController.GetRotationUpgradePrice();
            if (GameManager.Instance.CoinCount >= price)
            {
                GameManager.Instance.AddCoin(-price);
                playerController.UpgradeRotation();
                UpdateUI();
            }
            else
            {
                StartCoroutine(ShowErrorMessage("Not enough coins to upgrade rotation."));
            }
        }
    }

    private void UpgradeHealth()
    {
        PlayerController playerController = playerPrefabs[currentIndex].GetComponent<PlayerController>();
        if (playerController != null)
        {
            int price = playerController.GetHealthUpgradePrice();
            if (GameManager.Instance.CoinCount >= price)
            {
                GameManager.Instance.AddCoin(-price);
                playerController.UpgradeHealth();
                UpdateUI();
            }
            else
            {
                StartCoroutine(ShowErrorMessage("Not enough coins to upgrade health."));
            }
        }
    }

    private IEnumerator ShowErrorMessage(string message)
    {
        errorMessageText.text = message;
        errorMessageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        errorMessageText.gameObject.SetActive(false);
    }
}
