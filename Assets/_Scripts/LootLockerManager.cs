// ===== LootLockerManager.cs (Corrected) =====
using UnityEngine;
using LootLocker.Requests;
using System;

public class LootLockerManager : MonoBehaviour
{
    public static LootLockerManager Instance { get; private set; }

    // IMPORTANT: Replace this with the ID from your LootLocker dashboard!
    private int leaderboardID = 31702; // <--- CHANGE THIS

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
        // Start a guest session for the player.
        // This is a simple way to authenticate without requiring a login.
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (response.success)
            {
                Debug.Log("LootLocker session started successfully.");
            }
            else
            {
                // FIXED: Changed response.Error to response.errorData.message
                Debug.LogError("Error starting LootLocker session: " + response.errorData.message);
            }
        });
    }

    public void SubmitScore(int scoreToSubmit)
    {
        LootLockerSDKManager.SubmitScore(null, scoreToSubmit, leaderboardID.ToString(), (response) =>
        {
            if (response.success)
            {
                Debug.Log("Score submitted successfully to LootLocker.");
            }
            else
            {
                // FIXED: Changed response.Error to response.errorData.message
                Debug.LogError("Error submitting score: " + response.errorData.message);
            }
        });
    }

    public void FetchLeaderboard(Action<LootLockerLeaderboardMember[]> onComplete)
    {
        // Fetch the top 10 scores
        // FIXED: Changed leaderboardID to leaderboardID.ToString()
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