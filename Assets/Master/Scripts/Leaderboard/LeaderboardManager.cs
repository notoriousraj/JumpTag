using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using TMPro;
using CarterGames.Assets.LeaderboardManager; // Use CarterGames' LeaderboardEntry

[System.Serializable]
public class LeaderboardData
{
    public List<LeaderboardEntry> leaderboard;
}

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance;
    [SerializeField] private LeaderboardSpawner leaderboardSpawner;
    [SerializeField] private CatoffChallengeAPI catoffChallengeAPI;

    [Header("Leaderboard UI")]
    public TextMeshProUGUI leaderboardText;

    private string apiUrl = "https://your-api-url.com/leaderboard"; // Replace with your actual API URL

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void FetchLeaderboard()
    {
        Debug.Log("Fetching leaderboard data...");

        if (catoffChallengeAPI != null)
        {
            StartCoroutine(catoffChallengeAPI.GetLeaderboard(GameManager.Instance.challengeResponse.data.ChallengeID, (jsonResponse) =>
            {
                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    Debug.Log("Leaderboard JSON Response: " + jsonResponse);

                    // Deserialize JSON into LeaderboardData
                    LeaderboardData leaderboardData = JsonUtility.FromJson<LeaderboardData>(jsonResponse);

                    if (leaderboardData != null && leaderboardData.leaderboard != null)
                    {
                        Debug.Log($"Received {leaderboardData.leaderboard.Count} leaderboard entries.");
                        DisplayLeaderboard(leaderboardData);
                    }
                    else
                    {
                        Debug.LogError("Leaderboard data is null or empty.");
                    }
                }
                else
                {
                    Debug.LogError("Failed to retrieve leaderboard data.");
                }
            }));
        }
        else
        {
            Debug.LogError("CatoffChallengeAPI reference is missing.");
        }
    }

    private void DisplayLeaderboard(LeaderboardData leaderboardData)
    {
        leaderboardSpawner.PopulateLeaderboard(leaderboardData.leaderboard);
    }
}
