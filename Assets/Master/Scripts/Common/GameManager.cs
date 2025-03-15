using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("GamePlay Object")]
    [SerializeField] private SimplePlayerController player;
    [SerializeField] private BotController botPrefab;
    [SerializeField] private List<Transform> platforms;

    private int score = 0;
    private float gameDuration = 60f; // Game duration in seconds
    private float timeRemaining;
    private bool isGameActive = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }

    private void Update()
    {
        if (isGameActive)
        {
            timeRemaining -= Time.deltaTime;
            UIManager.Instance.UpdateTimer(timeRemaining);

            if (timeRemaining <= 0)
            {
                EndGame();
            }
        }
    }

    public void StartGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        score = 0;
        timeRemaining = gameDuration;
        isGameActive = true;

        UIManager.Instance.UpdateScore(score);
        UIManager.Instance.ToggleGameOverPanel(false);

        player.gameObject.SetActive(true);
        player.RespawnPlatforms = platforms;
        SpawnBotOnRandomPlatform();
    }

    public void BotCaught()
    {
        if (!isGameActive) return;

        score++;
        UIManager.Instance.UpdateScore(score);
        SpawnBotOnRandomPlatform();
    }

    void SpawnBotOnRandomPlatform()
    {
        if (platforms.Count == 0)
        {
            Debug.LogError("No platforms assigned in GameManager!");
            return;
        }

        Transform randomPlatform = platforms[UnityEngine.Random.Range(0, platforms.Count)];

        if (NavMesh.SamplePosition(randomPlatform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            Instantiate(botPrefab, hit.position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Couldn't place the bot on a valid NavMesh position.");
        }
    }

    void EndGame()
    {
        isGameActive = false;
        player.gameObject.SetActive(false);
        UIManager.Instance.ToggleGameOverPanel(true);
        Debug.Log("Game Over! Final Score: " + score);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
