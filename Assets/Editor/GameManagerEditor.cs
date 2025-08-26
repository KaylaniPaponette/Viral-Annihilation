// ===== GameManagerEditor.cs (Updated) =====
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement; // <-- ADD THIS LINE for the runtime SceneManager
using UnityEditor.SceneManagement;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    private int levelIndexToLoad = 0;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GameManager gameManager = (GameManager)target;

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Level Loader (Editor Tool)", EditorStyles.boldLabel);

        string[] levelNames = new string[gameManager.levelSequence.Count];
        for (int i = 0; i < gameManager.levelSequence.Count; i++)
        {
            levelNames[i] = gameManager.levelSequence[i].sceneName;
        }

        levelIndexToLoad = EditorGUILayout.Popup("Select Level", levelIndexToLoad, levelNames);

        // --- THIS IS THE UPDATED SECTION ---
        if (GUILayout.Button("Load Selected Level"))
        {
            string sceneToLoad = levelNames[levelIndexToLoad];

            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                // Check if Unity is currently in Play Mode.
                if (Application.isPlaying)
                {
                    // If we ARE in Play Mode, use the regular SceneManager.
                    Debug.Log($"PLAY MODE: Loading scene '{sceneToLoad}'.");
                    SceneManager.LoadScene(sceneToLoad);
                }
                else
                {
                    // If we are in Edit Mode, use the EditorSceneManager to open the scene for editing.
                    Debug.Log($"EDITOR MODE: Opening scene '{sceneToLoad}'.");
                    EditorSceneManager.OpenScene(sceneToLoad, OpenSceneMode.Single);
                }
            }
        }
    }
}