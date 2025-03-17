using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;
using Solana.Unity.Wallet;

public class CatoffChallengeAPI : MonoBehaviour
{
    private const string BASE_URL = "https://sonicmainnet-apiv2.catoff.xyz/";
    private const string API_KEY = "6e9b6b872c8505336f3f82b8b7937656acf76e6676a7560141d2de0b69476d0c";

    private PublicKey usdcMintAddress = new PublicKey("EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v");
    private PublicKey bonkMintAddress = new PublicKey("DezXAZ8z7PnrnRJjz3wXBoRgixCa6xjnB7YaB1pPB263");
    private PublicKey sendMintAddress = new PublicKey("SENDdRQtYMWaQrBroBrJ2Q53fgVuq95CV9UPGEvpCxa");
    private PublicKey wifMintAddress = new PublicKey("EKpQGSJtjMFqKZ9KQanSqYXRcF8fBopzLHYxdM65zcjm");
    private PublicKey popcatMintAddress = new PublicKey("7GCihgDB8fe6KNjn2MYtkzZcRjQy3t9GHdC8uHYmW2hr");
    private PublicKey pnutMintAddress = new PublicKey("2qEHjDLDLbuBgRYvsxhc5D6uDWAivNFZGan56P1tpump");
    private PublicKey gigaMintAddress = new PublicKey("63LfDmNb3MQ8mw9MtZ2To9bEA2M71kZUUGq5tiJxcqj9");
    private PublicKey trumpMintAddress = new PublicKey("6p6xgHyF7AeE6TZkSmFsko444wqoP15icUSqi2jfGiPN");
    private PublicKey melaniaMintAddress = new PublicKey("FUAfBo2jgks6gB4Z4LfZkqSZgzNucisEHqnNebaRxM1P");
    private PublicKey aicatMintAddress = new PublicKey("FmMzuNwEiK1Gua2mMoTpQDF9k3mHW3QVVfq3RjxV4QQY");

    public IEnumerator CreateChallenge(string challengeName, string challengeDescription, long startDate, long endDate, float wager, Action<string> callback, int target, bool allowSideBets = false, float sideBetsWager = 0, bool isPrivate = false, string userAddress = "BrzYoy2wF5ZWb2BQtCcBcFMGj7AdfvrbWA11A4bz99om", string currency = "SOL")
    {
        string url = BASE_URL + "challenge";

        ChallengeData challenge = new ChallengeData
        {
            ChallengeName = challengeName,
            ChallengeDescription = challengeDescription,
            StartDate = startDate,
            EndDate = endDate,
            GameID = 10,
            Wager = wager,
            Target = target,
            AllowSideBets = allowSideBets,
            SideBetsWager = sideBetsWager,
            IsPrivate = isPrivate,
            Currency = currency,
            ChallengeCategory = "Gaming",
            NFTMedia = "",
            Media = "",
            ActualStartDate = startDate,
            UserAddress = userAddress
        };

        string jsonData = JsonUtility.ToJson(challenge);
        yield return SendPostRequest(url, jsonData, callback);
    }

    public IEnumerator CreatePlayer(int challengeID, string userAddress, Action<string> callback)
    {
        string url = BASE_URL + "player";

        PlayerData player = new PlayerData
        {
            ChallengeID = challengeID,
            UserAddress = userAddress
        };

        string jsonData = JsonUtility.ToJson(player);
        yield return SendPostRequest(url, jsonData, callback);
    }

    public IEnumerator UpdatePlayerScore(int challengeID, int updatedScore, string userAddress, Action<string> callback)
    {
        Debug.Log("Update Player Score Called");
        string url = BASE_URL + "player/updatePlayerScore";

        ScoreUpdateData scoreUpdate = new ScoreUpdateData
        {
            ChallengeID = challengeID,
            UpdatedScore = updatedScore,
            UserAddress = userAddress
        };

        string jsonData = JsonUtility.ToJson(scoreUpdate);
        yield return SendPostRequest(url, jsonData, callback);
    }

    public IEnumerator GetLeaderboard(int challengeID, Action<string> callback)
    {
        Debug.Log("GetLeaderBoard");
        string url = BASE_URL + "challenge/leaderboard/" + challengeID;
        yield return SendGetRequest(url, callback);
    }

    private IEnumerator SendPostRequest(string url, string jsonData, Action<string> callback)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("x-api-key", API_KEY);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                callback?.Invoke(request.downloadHandler.text);
            else
                Debug.LogError("Error: " + request.error);
        }
    }

    private IEnumerator SendGetRequest(string url, Action<string> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("x-api-key", API_KEY);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                callback?.Invoke(request.downloadHandler.text);
            else
                Debug.LogError("Error: " + request.error);
        }
    }
}

[Serializable]
public class ChallengeData
{
    public string ChallengeName;
    public string ChallengeDescription;
    public long StartDate;
    public long EndDate;
    public int GameID;
    public float Wager;
    public int Target;
    public bool AllowSideBets;
    public float SideBetsWager;
    public bool IsPrivate;
    public string Currency;
    public string ChallengeCategory;
    public string NFTMedia;
    public string Media;
    public long ActualStartDate;
    public string UserAddress;
}

[Serializable]
public class PlayerData
{
    public int ChallengeID;
    public string UserAddress;
}

[Serializable]
public class ScoreUpdateData
{
    public int ChallengeID;
    public int UpdatedScore;
    public string UserAddress;
}
