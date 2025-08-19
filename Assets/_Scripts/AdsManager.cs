using UnityEngine;
using UnityEngine.Advertisements;

public class AdsManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public static AdsManager Instance { get; private set; }

    [SerializeField] string _androidGameId = "YOUR_ANDROID_GAME_ID";
    [SerializeField] string _iOSGameId = "YOUR_IOS_GAME_ID";
    [SerializeField] string _rewardedVideoPlacementId = "Rewarded_Android"; // Or "Rewarded_iOS"
    [SerializeField] bool _testMode = true;
    private string _gameId;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAds();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializeAds()
    {
#if UNITY_IOS
            _gameId = _iOSGameId;
#elif UNITY_ANDROID
        _gameId = _androidGameId;
#elif UNITY_EDITOR
            _gameId = _androidGameId; // Use Android for testing in editor
#endif

        if (!Advertisement.isInitialized && Advertisement.isSupported)
        {
            Advertisement.Initialize(_gameId, _testMode, this);
        }
    }

    public void ShowRewardedAd()
    {
        Debug.Log("Attempting to show rewarded ad...");
        Advertisement.Show(_rewardedVideoPlacementId, this);
    }

    // --- Interface Implementation ---

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
        // Pre-load the rewarded ad
        Advertisement.Load(_rewardedVideoPlacementId, this);
    }



    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log($"Ad loaded for placement: {placementId}");
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Error loading Ad Unit {placementId}: {error.ToString()} - {message}");
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing Ad Unit {placementId}: {error.ToString()} - {message}");
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        Debug.Log($"Ad started for placement: {placementId}");
        // You might want to pause your game here
        Time.timeScale = 0f;
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log($"Ad clicked for placement: {placementId}");
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log($"Ad completed for placement: {placementId} with state: {showCompletionState}");
        Time.timeScale = 1f; // Resume game
        if (placementId.Equals(_rewardedVideoPlacementId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            Debug.Log("Rewarded ad completed! Granting extra shot.");
            // Grant the reward
            GameManager.Instance.GrantExtraShot();
        }
        else if (placementId.Equals(_rewardedVideoPlacementId) && showCompletionState.Equals(UnityAdsShowCompletionState.SKIPPED))
        {
            Debug.Log("Ad was skipped. No reward.");
            // Decide if you want to go to game over immediately
            GameManager.Instance.GoToGameOver();
        }
    }
}