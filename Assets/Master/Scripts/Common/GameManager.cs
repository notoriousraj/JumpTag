using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using Solana.Unity.SDK;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("GamePlay Object")]
    [SerializeField] private SimplePlayerController player;
    [SerializeField] private BotController botPrefab;
    [SerializeField] private List<Transform> platforms;
    [SerializeField] private CatoffChallengeAPI catoffChallengeAPI;

    private int score = 0;
    private float gameDuration = 15f; // Game duration in seconds
    private float timeRemaining;
    private bool isGameActive = false;
    public ResponseData challengeResponse;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }

    private void Update()
    {
        if (isGameActive && !UIManager.Instance.IsPaused)
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
        Destroy(GameObject.FindGameObjectWithTag("It"));

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        score = 0;
        timeRemaining = gameDuration;
        isGameActive = false; // Set to false initially until API calls succeed

        UIManager.Instance.UpdateScore(score);
        UIManager.Instance.ToggleGameOverPanel(false);

        player.gameObject.SetActive(false); // Keep player inactive until API response

        StartCoroutine(CreateChallengeAndStartGame());
        // player.gameObject.SetActive(true);
        // player.RespawnPlatforms = platforms;
        // SpawnBotOnRandomPlatform();
        // isGameActive = true;
    }

    private IEnumerator CreateChallengeAndStartGame()
    {
        string challengeName = "Jump Tag Challenge";
        string challengeDescription = "Tag the bots before time runs out!";
        long startDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long endDate = startDate + (long)gameDuration;
        float wager = 0.01f;
        int target = 100;

        yield return catoffChallengeAPI.CreateChallenge(
            challengeName,
            challengeDescription,
            startDate,
            endDate,
            wager,
            (response) =>
            {
                Debug.Log("Challenge Created: " + response);
                challengeResponse = JsonUtility.FromJson<ResponseData>(response);
                Debug.Log("Parsed Challenge ID: " + challengeResponse.data.ChallengeID);

            },
            target
        );

        if (challengeResponse.data.ChallengeID == -1)
        {
            Debug.LogError("Failed to create challenge, cannot start game.");
            yield break;
        }

        // Step 2: Create Player
        bool playerCreated = false;
        yield return catoffChallengeAPI.CreatePlayer(
            challengeResponse.data.ChallengeID,
            Web3.Account.PublicKey, // Player's wallet address
            (response) =>
            {
                Debug.Log("Player Registered: " + response);
                playerCreated = true;
            }
        );

        if (!playerCreated)
        {
            Debug.LogError("Failed to create player, cannot start game.");
            yield break;
        }

        // Step 3: Start the game after successful API calls
        player.gameObject.SetActive(true);
        player.RespawnPlatforms = platforms;
        SpawnBotOnRandomPlatform();
        isGameActive = true;
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

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        int challengeID = challengeResponse.data.ChallengeID;
        int updatedScore = score;
        string userAddress = Web3.Account.PublicKey;

        Debug.Log("Game Over! Final Score: " + updatedScore);

        // Start the coroutine properly
        StartCoroutine(catoffChallengeAPI.UpdatePlayerScore(challengeID, updatedScore, userAddress, (response) =>
        {
            Debug.Log("Score update response: " + response);
        }));
    }
}

[Serializable]
public class ResponseData
{
    public bool success;
    public string message;
    public ChallengeCreateData data;
}

[Serializable]
public class ChallengeCreateData
{
    public int ChallengeID;
}

