using UnityEngine;
using UnityEngine.UI;

public class AdOfferController : MonoBehaviour
{
    public GameObject adOfferPanel;
    public Button watchAdButton;
    public Button declineButton;

    // MODIFIED: Changed Start to Awake
    void Awake()
    {
        // Find the GameManager and tell it that this is the AdOfferController it should use
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterAdOfferController(this);
        }
        else
        {
            Debug.LogError("AdOfferController could not find GameManager instance!");
        }


        // Add listeners to the buttons
        watchAdButton.onClick.AddListener(OnWatchAdClicked);
        declineButton.onClick.AddListener(OnDeclineClicked);

        // Start with the panel hidden
        if (adOfferPanel != null)
        {
            adOfferPanel.SetActive(false);
        }
    }

    public void ShowOffer()
    {
        if (adOfferPanel != null)
        {
            adOfferPanel.SetActive(true);
        }
    }

    public void HideOffer()
    {
        if (adOfferPanel != null)
        {
            adOfferPanel.SetActive(false);
        }
    }

    void OnWatchAdClicked()
    {
        Debug.Log("Player chose to watch the ad.");
        // Call the AdsManager to show the ad
        AdsManager.Instance.ShowRewardedAd();
        // Hide the panel so they don't click it again
        HideOffer();
    }

    void OnDeclineClicked()
    {
        Debug.Log("Player declined the ad offer.");
        // The player said no, so it's game over
        HideOffer();
        GameManager.Instance.GoToGameOver();
    }

    void OnDestroy()
    {
        // Clean up listeners
        watchAdButton.onClick.RemoveListener(OnWatchAdClicked);
        declineButton.onClick.RemoveListener(OnDeclineClicked);

        // --- NEW ---
        // Unregister itself from the GameManager when the scene is unloaded
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterAdOfferController();
        }
    }
}