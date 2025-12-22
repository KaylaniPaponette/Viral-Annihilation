using UnityEngine;
using TMPro;
using LootLocker.Requests;
using UnityEngine.UI;
using System.Collections; 

public class LeaderboardDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform scoreContainer;
    [SerializeField] private GameObject scoreEntryPrefab;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Scroll Settings")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Color highlightColor = Color.yellow;

    void Start()
    {
        FetchAndDisplayScores();
    }

    public void FetchAndDisplayScores()
    {
        foreach (Transform child in scoreContainer)
        {
            Destroy(child.gameObject);
        }

        if (statusText != null) statusText.text = "Loading Scores...";

        if (LootLockerManager.Instance != null)
        {
            LootLockerManager.Instance.FetchLeaderboard(OnLeaderboardFetched);
        }
    }

    private void OnLeaderboardFetched(LootLockerLeaderboardMember[] scores)
    {
        if (scores == null)
        {
            if (statusText != null) statusText.text = "Could not fetch scores.";
            return;
        }

        if (statusText != null) statusText.text = "High Scores";

        // --- FIX: Get the ID from our own Manager Instance ---
        string currentPlayerId = "";
        if (LootLockerManager.Instance != null)
        {
            currentPlayerId = LootLockerManager.Instance.CurrentPlayerID;
        }

        GameObject playerEntry = null;

        for (int i = 0; i < scores.Length; i++)
        {
            GameObject entryObject = Instantiate(scoreEntryPrefab, scoreContainer);
            TextMeshProUGUI entryText = entryObject.GetComponent<TextMeshProUGUI>();

            string playerName = string.IsNullOrEmpty(scores[i].player.name)
                ? "Player " + scores[i].player.id
                : scores[i].player.name;

            if (entryText != null)
            {
                entryText.text = $"{scores[i].rank}. {playerName} - {scores[i].score:N0}";

                // Compare with the player's ID from the leaderboard entry
                if (scores[i].player.id.ToString() == currentPlayerId || scores[i].player.public_uid == currentPlayerId)
                {
                    entryText.color = highlightColor;
                    playerEntry = entryObject;
                }
            }
        }

        if (playerEntry != null && scrollRect != null)
        {
            StartCoroutine(FocusOnEntry(playerEntry.GetComponent<RectTransform>()));
        }
    }

    private IEnumerator FocusOnEntry(RectTransform target)
    {
        // Wait until the end of the frame so the Vertical Layout Group 
        // has finished positioning the new entries.
        yield return new WaitForEndOfFrame();

        // Force the UI to update its positions immediately
        Canvas.ForceUpdateCanvases();

        // Calculate the position to center the player's entry
        float viewportHeight = scrollRect.viewport.rect.height;
        float centerOffset = viewportHeight / 2f;

        // We move the content's Y position to bring the target into view
        Vector2 newPos = scrollRect.content.localPosition;
        newPos.y = -target.localPosition.y - centerOffset;

        scrollRect.content.localPosition = newPos;
    }
}



//// ===== LeaderboardDisplay.cs =====
//using UnityEngine;
//using TMPro;
//using LootLocker.Requests;

//public class LeaderboardDisplay : MonoBehaviour
//{
//    [Header("UI References")]
//    [Tooltip("The parent object where score entries will be created.")]
//    [SerializeField] private Transform scoreContainer;

//    [Tooltip("The prefab for a single score entry (must have a TextMeshProUGUI component).")]
//    [SerializeField] private GameObject scoreEntryPrefab;

//    [Tooltip("A text object to show a 'Loading...' or error message.")]
//    [SerializeField] private TextMeshProUGUI statusText;

//    void Start()
//    {
//        // Automatically fetch scores when this object is enabled.
//        FetchAndDisplayScores();
//    }

//    public void FetchAndDisplayScores()
//    {
//        // Clear any old scores
//        foreach (Transform child in scoreContainer)
//        {
//            Destroy(child.gameObject);
//        }

//        if (statusText != null) statusText.text = "Loading Scores...";

//        if (LootLockerManager.Instance != null)
//        {
//            LootLockerManager.Instance.FetchLeaderboard(OnLeaderboardFetched);
//        }
//        else
//        {
//            if (statusText != null) statusText.text = "Error: LootLocker not found.";
//            Debug.LogError("LootLockerManager instance not found!");
//        }
//    }

//    private void OnLeaderboardFetched(LootLockerLeaderboardMember[] scores)
//    {
//        if (scores != null)
//        {
//            if (statusText != null) statusText.text = "High Scores";

//            // Create a new UI element for each score
//            for (int i = 0; i < scores.Length; i++)
//            {
//                GameObject entryObject = Instantiate(scoreEntryPrefab, scoreContainer);
//                TextMeshProUGUI entryText = entryObject.GetComponent<TextMeshProUGUI>();

//                // Use the player's name if they set one, otherwise their ID
//                string playerName = string.IsNullOrEmpty(scores[i].player.name) ? "Player " + scores[i].player.id : scores[i].player.name;

//                if (entryText != null)
//                {
//                    entryText.text = $"{scores[i].rank}. {playerName} - {scores[i].score:N0}";
//                }
//            }
//        }
//        else
//        {
//            if (statusText != null) statusText.text = "Could not fetch scores.";
//        }
//    }
//}