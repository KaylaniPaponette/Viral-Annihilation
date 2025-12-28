using UnityEngine;
using LootLocker.Requests;
using System;

public class LootLockerManager : MonoBehaviour
{
    public static LootLockerManager Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("The ID found on your LootLocker Dashboard -> Game Systems -> Leaderboards")]
    [SerializeField] private int leaderboardID = 31702;

    public string CurrentPlayerID { get; private set; }
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
        InitializeSession();
    }

    private void InitializeSession()
    {
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (response.success)
            {
                Debug.Log("<color=green>LootLocker: Session started successfully.</color>");
                CurrentPlayerID = response.player_id.ToString();

                // Check if we have a saved name to restore
                if (PlayerPrefs.HasKey(PlayerNamePrefsKey))
                {
                    string savedName = PlayerPrefs.GetString(PlayerNamePrefsKey);
                    SetPlayerName(savedName, null);
                }
            }
            else
            {
                // DIAGNOSTIC: This helps you see if your API Key or Game Settings are wrong
                Debug.LogError($"<color=red>LootLocker Session Error:</color> {response.errorData.message}");
                Debug.LogError($"Error Code: {response.errorData.code}");
            }
        });
    }

    public void SetPlayerName(string name, Action<bool> onComplete)
    {
        LootLockerSDKManager.SetPlayerName(name, (response) =>
        {
            if (response.success)
            {
                Debug.Log($"LootLocker: Name set to {name}");
                PlayerPrefs.SetString(PlayerNamePrefsKey, name);
                PlayerPrefs.Save();
                onComplete?.Invoke(true);
            }
            else
            {
                Debug.LogWarning($"LootLocker: Failed to set name. {response.errorData.message}");
                onComplete?.Invoke(false);
            }
        });
    }

    public void SubmitScore(int scoreToSubmit)
    {
        // Use the instance ID to ensure we are submitting to the right board
        LootLockerSDKManager.SubmitScore(null, scoreToSubmit, leaderboardID.ToString(), (response) =>
        {
            if (response.success)
            {
                Debug.Log($"<color=cyan>LootLocker: Score {scoreToSubmit} submitted successfully!</color>");
            }
            else
            {
                // DIAGNOSTIC: Tells you if the specific Board ID is the problem
                Debug.LogError($"<color=red>LootLocker Submit Error:</color> {response.errorData.message}");
                if (response.errorData.message.Contains("not found"))
                {
                    Debug.LogError("HINT: Your Leaderboard ID might be incorrect or the board is not 'Active' in the dashboard.");
                }
            }
        });
    }

    public void FetchLeaderboard(Action<LootLockerLeaderboardMember[]> onComplete)
    {
        LootLockerSDKManager.GetScoreList(leaderboardID.ToString(), 100, 0, (response) =>
        {
            if (response.success)
            {
                Debug.Log($"LootLocker: Successfully fetched {response.items.Length} scores.");
                onComplete?.Invoke(response.items);
            }
            else
            {
                // DIAGNOSTIC: Most common place for "broken" boards to show errors
                Debug.LogError($"<color=red>LootLocker Fetch Error:</color> {response.errorData.message}");
                onComplete?.Invoke(null);
            }
        });
    }
}

//// ===== LootLockerManager.cs (Updated for Player Names) =====
//using UnityEngine;
//using LootLocker.Requests;
//using System;

//public class LootLockerManager : MonoBehaviour
//{
//    public static LootLockerManager Instance { get; private set; }

//    // IMPORTANT: Replace this with the ID from your LootLocker dashboard!
//    private int leaderboardID = 31702; // <--- This should be your ID

//    // --- NEW: Store the Player ID here ---
//    public string CurrentPlayerID { get; private set; }

//    // --- NEW ---
//    // Key to save the player's name locally
//    private const string PlayerNamePrefsKey = "PlayerName";

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
//        LootLockerSDKManager.StartGuestSession((response) =>
//        {
//            if (response.success)
//            {
//                Debug.Log("LootLocker session started successfully.");

//                // --- NEW: Save the ID from the response ---
//                CurrentPlayerID = response.player_id.ToString();

//                if (PlayerPrefs.HasKey(PlayerNamePrefsKey))
//                {
//                    string savedName = PlayerPrefs.GetString(PlayerNamePrefsKey);
//                    SetPlayerName(savedName, null);
//                }
//            }
//            else
//            {
//                Debug.LogError("Error starting LootLocker session: " + response.errorData.message);
//            }
//        });
//    }

//    // --- NEW METHOD ---
//    /// <summary>
//    /// Sets the player's name on LootLocker and saves it locally.
//    /// </summary>
//    /// <param name="name">The name to set for the player.</param>
//    /// <param name="onComplete">Callback that returns true on success, false on failure.</param>
//    public void SetPlayerName(string name, Action<bool> onComplete)
//    {
//        LootLockerSDKManager.SetPlayerName(name, (response) =>
//        {
//            if (response.success)
//            {
//                Debug.Log("Successfully set player name: " + name);
//                // Save the name locally upon successful update
//                PlayerPrefs.SetString(PlayerNamePrefsKey, name);
//                PlayerPrefs.Save();
//                onComplete?.Invoke(true);
//            }
//            else
//            {
//                Debug.LogError("Error setting player name: " + response.errorData.message);
//                onComplete?.Invoke(false);
//            }
//        });
//    }


//    public void SubmitScore(int scoreToSubmit)
//    {
//        // No changes needed here. LootLocker automatically uses the name of the currently logged-in player.
//        LootLockerSDKManager.SubmitScore(null, scoreToSubmit, leaderboardID.ToString(), (response) =>
//        {
//            if (response.success)
//            {
//                Debug.Log("Score submitted successfully to LootLocker.");
//            }
//            else
//            {
//                Debug.LogError("Error submitting score: " + response.errorData.message);
//            }
//        });
//    }

//    // ===== LootLockerManager.cs (Updated for 100 entries) =====
//    public void FetchLeaderboard(Action<LootLockerLeaderboardMember[]> onComplete)
//    {
//        // Increased the second parameter from 10 to 100
//        LootLockerSDKManager.GetScoreList(leaderboardID.ToString(), 100, 0, (response) =>
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