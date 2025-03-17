using TMPro;
using UnityEngine;

public class LeaderboardEntryDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text positionLabel;
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private TMP_Text scoreLabel;

    /// <summary>
    /// Updates the leaderboard entry display.
    /// </summary>
    public void UpdateDisplay(string playerName, int playerScore, int rank)
    {
        if (positionLabel) positionLabel.text = (rank + 1).ToString();
        if (nameLabel) nameLabel.text = playerName;
        if (scoreLabel) scoreLabel.text = playerScore.ToString();
        
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Resets the leaderboard entry display.
    /// </summary>
    public void ResetDisplay()
    {
        if (positionLabel) positionLabel.text = "";
        if (nameLabel) nameLabel.text = "";
        if (scoreLabel) scoreLabel.text = "";

        gameObject.SetActive(false);
    }
}
