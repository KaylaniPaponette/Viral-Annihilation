// ===== LootLockerManager.cs (Updated for Player Names) =====
using UnityEngine;
using LootLocker.Requests;
using System;

public class LootLockerManager : MonoBehaviour
{
    public static LootLockerManager Instance { get; private set; }

    // IMPORTANT: Replace this with the ID from your LootLocker dashboard!
    private int leaderboardID = 31702; // <--- This should be your ID

    // --- NEW ---
    // Key to save the player's name locally
    private const string PlayerNamePrefsKey = "PlayerName";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (response.success)
            {
                Debug.Log("LootLocker session started successfully.");
                // --- NEW ---
                // When the session starts, check if we have a saved name and set it
                if (PlayerPrefs.HasKey(PlayerNamePrefsKey))
                {
                    string savedName = PlayerPrefs.GetString(PlayerNamePrefsKey);
                    SetPlayerName(savedName, null); // Set the name without needing a callback here
                }
            }
            else
            {
                Debug.LogError("Error starting LootLocker session: " + response.errorData.message);
            }
        });
    }

    // --- NEW METHOD ---
    /// <summary>
    /// Sets the player's name on LootLocker and saves it locally.
    /// </summary>
    /// <param name="name">The name to set for the player.</param>
    /// <param name="onComplete">Callback that returns true on success, false on failure.</param>
    public void SetPlayerName(string name, Action<bool> onComplete)
    {
        LootLockerSDKManager.SetPlayerName(name, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully set player name: " + name);
                // Save the name locally upon successful update
                PlayerPrefs.SetString(PlayerNamePrefsKey, name);
                PlayerPrefs.Save();
                onComplete?.Invoke(true);
            }
            else
            {
                Debug.LogError("Error setting player name: " + response.errorData.message);
                onComplete?.Invoke(false);
            }
        });
    }


    public void SubmitScore(int scoreToSubmit)
    {
        // No changes needed here. LootLocker automatically uses the name of the currently logged-in player.
        LootLockerSDKManager.SubmitScore(null, scoreToSubmit, leaderboardID.ToString(), (response) =>
        {
            if (response.success)
            {
                Debug.Log("Score submitted successfully to LootLocker.");
            }
            else
            {
                Debug.LogError("Error submitting score: " + response.errorData.message);
            }
        });
    }

    public void FetchLeaderboard(Action<LootLockerLeaderboardMember[]> onComplete)
    {
        LootLockerSDKManager.GetScoreList(leaderboardID.ToString(), 10, 0, (response) =>
        {
            if (response.success)
            {
                onComplete?.Invoke(response.items);
            }
            else
            {
                Debug.LogError("Failed to fetch leaderboard: " + response.errorData.message);
                onComplete?.Invoke(null);
            }
        });
    }
}















//// ===== LootLockerManager.cs (Corrected) =====
//using UnityEngine;
//using LootLocker.Requests;
//using System;

//public class LootLockerManager : MonoBehaviour
//{
//    public static LootLockerManager Instance { get; private set; }

//    // IMPORTANT: Replace this with the ID from your LootLocker dashboard!
//    private int leaderboardID = 31702; // <--- CHANGE THIS

//    private void Awake()
//    {
//        if (Instance == null)
//        {
//            Instance = this;
//            DontDestroyOnLoad(gameObject);
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//    }

//    void Start()
//    {
//        // Start a guest session for the player.
//        // This is a simple way to authenticate without requiring a login.
//        LootLockerSDKManager.StartGuestSession((response) =>
//        {
//            if (response.success)
//            {
//                Debug.Log("LootLocker session started successfully.");
//            }
//            else
//            {
//                // FIXED: Changed response.Error to response.errorData.message
//                Debug.LogError("Error starting LootLocker session: " + response.errorData.message);
//            }
//        });
//    }

//    public void SubmitScore(int scoreToSubmit)
//    {
//        LootLockerSDKManager.SubmitScore(null, scoreToSubmit, leaderboardID.ToString(), (response) =>
//        {
//            if (response.success)
//            {
//                Debug.Log("Score submitted successfully to LootLocker.");
//            }
//            else
//            {
//                // FIXED: Changed response.Error to response.errorData.message
//                Debug.LogError("Error submitting score: " + response.errorData.message);
//            }
//        });
//    }

//    public void FetchLeaderboard(Action<LootLockerLeaderboardMember[]> onComplete)
//    {
//        // Fetch the top 10 scores
//        // FIXED: Changed leaderboardID to leaderboardID.ToString()
//        LootLockerSDKManager.GetScoreList(leaderboardID.ToString(), 10, 0, (response) =>
//        {
//            if (response.success)
//            {
//                onComplete?.Invoke(response.items);
//            }
//            else
//            {
//                Debug.LogError("Failed to fetch leaderboard: " + response.errorData.message);
//                onComplete?.Invoke(null);
//            }
//        });
//    }
//}