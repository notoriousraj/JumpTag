using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Start Panel")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;

    [Header("Retry Panel")]
    [SerializeField] private GameObject retryPanel;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Play Mode Panel")]
    [SerializeField] private GameObject playModePanel;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenu2Button;

    [Header("Game Stats UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject gameOverPanel;

    private string playerName;

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
        mainMenuButton.onClick.AddListener(OnMainMenu);
        mainMenu2Button.onClick.AddListener(OnMainMenu);
    }

    private void Start()
    {
        playerName = PlayerPrefs.GetString("PlayerName", ""); // Default to empty string
        if (!string.IsNullOrEmpty(playerName))
            nameInput.text = playerName;
    }

    private void OnStartGame()
    {
        if (string.IsNullOrEmpty(nameInput.text))
            return;

        playerName = nameInput.text;
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();

        GameManager.Instance.StartGame();
        ChangeGameState(GameState.Game);
    }

    private void OnRetryGame()
    {
        GameManager.Instance.StartGame();
        ChangeGameState(GameState.Game);
    }

    private void OnExitGame()
    {
        Application.Quit();
    }

    private void OnMainMenu()
    {
        ChangeGameState(GameState.Menu);
    }

    public void ChangeGameState(GameState gameState)
    {
        startPanel.SetActive(gameState == GameState.Menu);
        playModePanel.SetActive(gameState == GameState.Game);
        retryPanel.SetActive(gameState == GameState.GameOver);
    }

    public void UpdateTimer(float timeRemaining)
    {
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
        mainMenuButton.onClick.RemoveListener(OnMainMenu);
        mainMenu2Button.onClick.RemoveListener(OnMainMenu);
    }
}
