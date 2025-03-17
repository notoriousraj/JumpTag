using System;
using Solana.Unity.SDK;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Start Panel")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private Button walletButton;
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;

    [Header("Confirmation Panel")]
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    [Header("Retry Panel")]
    [SerializeField] private GameObject retryPanel;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Play Mode Panel")]
    [SerializeField] private GameObject playModePanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenu2Button;

    [Header("Game Stats UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject gameOverPanel;
    [Header("Crypto")]
    [SerializeField] private SolanaTransactionExample solanaTransactionExample;
    private bool isPaused = false;
    public bool IsPaused { get { return isPaused; } }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }

    private void OnEnable()
    {
        ChangeGameState(GameState.Menu);

        startButton.onClick.AddListener(OnStartGame);
        retryButton.onClick.AddListener(OnRetryGame);
        exitButton.onClick.AddListener(OnExitGame);
        confirmButton.onClick.AddListener(OnConfirmGame);
        cancelButton.onClick.AddListener(OnCancelGame);
        mainMenuButton.onClick.AddListener(OnMainMenu);
        pauseButton.onClick.AddListener(OnPauseGame);
        resumeButton.onClick.AddListener(OnResumeGame);
        mainMenu2Button.onClick.AddListener(OnMainMenu);
    }

    private void Start()
    {
        // Check initial wallet connection state
        UpdateWalletButtonText();

        // Subscribe to wallet connection event
        Web3.OnLogin += (account) =>
        {
            Debug.Log($"Wallet connected: {account.PublicKey}");
            UpdateWalletButtonText();
        };
    }

    // Method to update wallet button text
    private void UpdateWalletButtonText()
    {
        if (Web3.Account != null && !string.IsNullOrEmpty(Web3.Account.PublicKey))
        {
            walletButton.GetComponentInChildren<TextMeshProUGUI>().text = "Connected";
        }
        else
        {
            walletButton.GetComponentInChildren<TextMeshProUGUI>().text = "Connect \nWallet";
        }
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && playModePanel.activeInHierarchy == true)
            OnPauseGame();
    }

    private void OnStartGame()
    {
        // Check if wallet is connected before allowing game start
        if (Web3.Account == null || string.IsNullOrEmpty(Web3.Account.PublicKey))
        {
            Debug.LogError("Wallet not connected! Please connect your wallet before starting the game.");
            return;
        }

        confirmationPanel.SetActive(true);
    }

    public async void OnConfirmGame()
    {
        bool transactionSuccess = await solanaTransactionExample.TransferEntryFeeToEscrow();
        if (transactionSuccess)
        {
            OnConfirm();
        }
        else
        {
            Debug.LogError("❌ Transaction failed. Cannot proceed.");
            confirmationPanel.SetActive(false);
            OnConfirm();
        }
    }

    private void OnConfirm()
    {
        GameManager.Instance.StartGame();
        confirmationPanel.SetActive(false);
        ChangeGameState(GameState.Game);
    }

    void OnCancelGame()
    {
        confirmationPanel.SetActive(false);
    }

    private void OnRetryGame()
    {
        confirmationPanel.SetActive(true);
    }

    private void OnExitGame()
    {
        Application.Quit();
    }

    private void OnMainMenu()
    {
        ChangeGameState(GameState.Menu);
        isPaused = false;
        pausePanel.SetActive(false);
    }

    private void OnPauseGame()
    {
        isPaused = true;  // Stop the timer updates
        pausePanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnResumeGame()
    {
        isPaused = false;  // Resume the timer updates
        pausePanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ChangeGameState(GameState gameState)
    {
        if (gameState == GameState.Menu)
            LeaderboardManager.Instance.FetchLeaderboard();
            
        startPanel.SetActive(gameState == GameState.Menu);
        playModePanel.SetActive(gameState == GameState.Game);
        retryPanel.SetActive(gameState == GameState.GameOver);
    }

    public void UpdateTimer(float timeRemaining)
    {
        if (isPaused) return; // Prevent updating the timer when paused
        timerText.text = "Time Remaining \n" + Mathf.CeilToInt(timeRemaining) + "s";
    }

    public void UpdateScore(int score)
    {
        scoreText.text = "Score \n" + score;
    }

    public void ToggleGameOverPanel(bool state)
    {
        gameOverPanel.SetActive(state);
    }

    private void OnDisable()
    {
        startButton.onClick.RemoveListener(OnStartGame);
        retryButton.onClick.RemoveListener(OnRetryGame);
        exitButton.onClick.RemoveListener(OnExitGame);
        confirmButton.onClick.RemoveListener(OnConfirmGame);
        cancelButton.onClick.RemoveListener(OnCancelGame);
        mainMenuButton.onClick.RemoveListener(OnMainMenu);
        pauseButton.onClick.RemoveListener(OnPauseGame);
        resumeButton.onClick.RemoveListener(OnResumeGame);
        mainMenu2Button.onClick.RemoveListener(OnMainMenu);
    }
}
