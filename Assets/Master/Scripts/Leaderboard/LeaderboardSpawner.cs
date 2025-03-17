using System.Collections.Generic;
using UnityEngine;
using CarterGames.Assets.LeaderboardManager; // Ensure you're using CarterGames' leaderboard system

public class LeaderboardSpawner : MonoBehaviour
{
    [Header("Leaderboard Settings")]
    [SerializeField] private Transform leaderboardContainer;
    [SerializeField] private GameObject leaderboardEntryPrefab;

    private List<GameObject> spawnedEntries = new List<GameObject>();

    public void PopulateLeaderboard(List<LeaderboardEntry> leaderboardEntries)
    {
        ClearLeaderboard();

        for (int i = 0; i < leaderboardEntries.Count; i++)
        {
            GameObject entryGO = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
            LeaderboardEntryDisplayTMP entryDisplay = entryGO.GetComponent<LeaderboardEntryDisplayTMP>();

            if (entryDisplay != null)
            {
                entryDisplay.UpdateDisplay(leaderboardEntries[i], i, new LeaderboardDisplayCustomisations());
            }
            else
            {
                Debug.LogError("LeaderboardEntryDisplayTMP component missing!");
            }

            spawnedEntries.Add(entryGO);
        }

        Debug.Log($"Spawned {leaderboardEntries.Count} leaderboard entries.");
    }

    private void ClearLeaderboard()
    {
        foreach (GameObject entry in spawnedEntries)
        {
            Destroy(entry);
        }
        spawnedEntries.Clear();
    }
}
