//using UnityEngine;
//public class LevelManager : MonoBehaviour
//{
//    private GameObject[] enemies;  // Array to store all enemies in the level
//    public string enemyTag = "Enemy"; // Tag used for enemies

//    void Start()
//    {
//        // Find all enemies in the scene with the specified tag
//        enemies = GameObject.FindGameObjectsWithTag(enemyTag);
//    }

//    void Update()
//    {
//        // Check if all enemies are destroyed
//        bool allEnemiesDestroyed = true;
//        foreach (GameObject enemy in enemies)
//        {
//            if (enemy != null)
//            {
//                allEnemiesDestroyed = false;
//                break;
//            }
//        }

//        // If all enemies are destroyed, let the GameManager handle level completion
//        if (allEnemiesDestroyed && GameManager.Instance != null)
//        {
//            // This will call the CompleteLevel method in the GameManager
//            GameManager.Instance.SendMessage("CompleteLevel", SendMessageOptions.DontRequireReceiver);

//            // Disable this script to prevent multiple calls
//            this.enabled = false;
//        }
//    }
//}

/*
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private GameObject[] enemies;  // Array to store all enemies in the level
    public string enemyTag = "Enemy"; // Tag used for enemies
    private bool levelCompleted = false;

    void Start()
    {
        // Find all enemies in the scene with the specified tag
        enemies = GameObject.FindGameObjectsWithTag(enemyTag);
    }

    void Update()
    {
        // Avoid multiple triggers
        if (levelCompleted)
            return;

        // Check if all enemies are destroyed
        bool allEnemiesDestroyed = true;
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                allEnemiesDestroyed = false;
                break;
            }
        }

        // If all enemies are destroyed, let the GameManager handle level completion
        if (allEnemiesDestroyed && GameManager.Instance != null)
        {
            levelCompleted = true;
            GameManager.Instance.CompleteLevel();

            // Disable this script to prevent multiple calls
            this.enabled = false;
        }
    }
}
*/