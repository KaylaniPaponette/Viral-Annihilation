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

    // 1. Create a dedicated Load method
    public void LoadAd()
    {
        Debug.Log($"Loading Ad: {_rewardedVideoPlacementId}");
        Advertisement.Load(_rewardedVideoPlacementId, this);
    }

    // --- Interface Implementation ---

    // 2. Update OnInitializationComplete to use the new method
    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
        LoadAd(); // Use the helper
    }
    //public void OnInitializationComplete()
    //{
    //    Debug.Log("Unity Ads initialization complete.");
    //    // Pre-load the rewarded ad
    //    Advertisement.Load(_rewardedVideoPlacementId, this);
    //}



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

    // 4. Update OnUnityAdsShowFailure to prevent the softlock
    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"Ad Show Failed: {error} - {message}");

        // Resume the game so it doesn't softlock!
        Time.timeScale = 1f;

        // If we can't show an ad, we should probably just go to Game Over
        // or give them the shot anyway if you're feeling generous.
        GameManager.Instance.GoToGameOver();

        // Try to load again for next time
        LoadAd();
    }
    //public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    //{
    //    Debug.Log($"Error showing Ad Unit {placementId}: {error.ToString()} - {message}");
    //}

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

    // 3. Update OnUnityAdsShowComplete to reload for the NEXT time
    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log($"Ad completed: {placementId} state: {showCompletionState}");
        Time.timeScale = 1f;

        if (placementId.Equals(_rewardedVideoPlacementId))
        {
            if (showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
            {
                GameManager.Instance.GrantExtraShot();
            }
            else
            {
                GameManager.Instance.GoToGameOver();
            }

            // --- THE FIX ---
            // Load the NEXT ad immediately so it's ready for the next time they lose
            LoadAd();
        }
    }
    //public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    //{
    //    Debug.Log($"Ad completed for placement: {placementId} with state: {showCompletionState}");
    //    Time.timeScale = 1f; // Resume game
    //    if (placementId.Equals(_rewardedVideoPlacementId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
    //    {
    //        Debug.Log("Rewarded ad completed! Granting extra shot.");
    //        // Grant the reward
    //        GameManager.Instance.GrantExtraShot();
    //    }
    //    else if (placementId.Equals(_rewardedVideoPlacementId) && showCompletionState.Equals(UnityAdsShowCompletionState.SKIPPED))
    //    {
    //        Debug.Log("Ad was skipped. No reward.");
    //        // Decide if you want to go to game over immediately
    //        GameManager.Instance.GoToGameOver();
    //    }
    //}
}