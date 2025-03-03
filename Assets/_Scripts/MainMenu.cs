using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{



    public void QuitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif

    }

    public void StartGame()
    {
        SceneManager.LoadScene("_Scenes/Level1");
    }

    public void FromAboutToMain()
    {
        SceneManager.LoadScene("_Scenes/MainMenu");
    }

    public void FromMainToAbout()
    {
        SceneManager.LoadScene("_Scenes/About");
    }
}
