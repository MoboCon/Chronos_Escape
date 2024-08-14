using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Canvases")]
    public Canvas mainMenuCanvas;
    public Canvas gameCanvas;
    public Canvas pauseCanvas;
    public Canvas gameOverCanvas;
    public Canvas levelCanvas;
    public Canvas shopCanvas;

    [Header("Main Menu UI Elements")]
    public Button startButton;
    public Text coinTextMainMenu;
    public Text gemsTextMainMenu;
    public Button levelButton;
    public Button shopButton;

    [Header("Game UI Elements")]
    public Text scoreText;
    public Text coinTextGame;
    public Text gemsTextGame;
    public Button pauseButton;

    [Header("Pause Menu UI Elements")]
    public Button continueButton;
    public Button settingsButton;
    public Button giveUpButton;

    [Header("Game Over UI Elements")]
    public Text currentScoreText;
    public Text bestScoreText;
    public Button gameOverBackButton;

    [Header("Level UI Elements")]
    public Text terrainNameText;
    public Text terrainPriceText;
    public Image terrainImage;
    public Button buyTerrainButton;
    public Button selectTerrainButton;
    public Button nextTerrainButton;
    public Button prevTerrainButton;
    public Text coinTextLevelCanvas;
    public Button backToMainMenuButton;

    [Header("Shop UI Elements")]
    public Button shopBackButton;
    public Text coinTextShop;

    [Header("Power Up UI Elements")]
    public Text powerUpText;

    [Header("PlayScene Continue Panel")]
    public GameObject playSceneContinuePanel;
    public Button continueWithAdButton;
    public Button continueWithGemsButton;
    public Button exitContinuePanelButton;

    public Text errorMessageText; // Error message text
    public Text gemText; // Gem text

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
        startButton.onClick.AddListener(GameManager.Instance.StartGame);
        pauseButton.onClick.AddListener(GameManager.Instance.PauseGame);
        continueButton.onClick.AddListener(GameManager.Instance.ContinueGame);
        giveUpButton.onClick.AddListener(GameManager.Instance.GiveUpGame);
        gameOverBackButton.onClick.AddListener(GameManager.Instance.BackToMainMenu);
        buyTerrainButton.onClick.AddListener(GameManager.Instance.BuyTerrain);
        selectTerrainButton.onClick.AddListener(GameManager.Instance.SelectTerrain);
        nextTerrainButton.onClick.AddListener(GameManager.Instance.NextTerrain);
        prevTerrainButton.onClick.AddListener(GameManager.Instance.PreviousTerrain);
        levelButton.onClick.AddListener(GameManager.Instance.OpenLevelCanvas);
        shopBackButton.onClick.AddListener(CloseShop);
        exitContinuePanelButton.onClick.AddListener(HidePlaySceneContinuePanel);
        continueWithAdButton.onClick.AddListener(GameManager.Instance.ContinueAfterAd);
        continueWithGemsButton.onClick.AddListener(GameManager.Instance.ContinueAfterGems);
        shopButton.onClick.AddListener(GameManager.Instance.OpenShop);
        backToMainMenuButton.onClick.AddListener(GameManager.Instance.BackToMainMenu);

        UpdateCoinText(GameManager.Instance.CoinCount);
        UpdateGemsText(GemManager.Instance.GetGemsCount());
    }

    public void OpenShop()
    {
        mainMenuCanvas.gameObject.SetActive(false);
        shopCanvas.gameObject.SetActive(true);
        UpdateCoinText(GameManager.Instance.CoinCount);
    }

    public void CloseShop()
    {
        shopCanvas.gameObject.SetActive(false);
        mainMenuCanvas.gameObject.SetActive(true);
    }

    public void UpdateCoinText(int coinCount)
    {
        if (coinTextMainMenu != null)
        {
            coinTextMainMenu.text = coinCount.ToString();
        }

        if (coinTextGame != null)
        {
            coinTextGame.text = coinCount.ToString();
        }

        if (coinTextLevelCanvas != null)
        {
            coinTextLevelCanvas.text = coinCount.ToString();
        }

        if (coinTextShop != null)
        {
            coinTextShop.text = coinCount.ToString();
        }
    }

    public void UpdateGemsText(int gemsCount)
    {
        if (gemsTextMainMenu != null)
        {
            gemsTextMainMenu.text = gemsCount.ToString();
        }

        if (gemsTextGame != null)
        {
            gemsTextGame.text = gemsCount.ToString();
        }

        if (gemText != null)
        {
            gemText.text = gemsCount.ToString();
        }
    }

    public void ShowPlaySceneContinuePanel()
    {
        if (playSceneContinuePanel != null)
        {
            playSceneContinuePanel.SetActive(true);
        }
    }

    public void ShowErrorMessage(string message)
    {
        errorMessageText.text = message;
        errorMessageText.gameObject.SetActive(true);
    }

    public void HideErrorMessage()
    {
        errorMessageText.gameObject.SetActive(false);
    }

    public void HidePlaySceneContinuePanel()
    {
        if (playSceneContinuePanel != null)
        {
            playSceneContinuePanel.SetActive(false);
        }
    }
}
