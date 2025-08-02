using UnityEngine;
using TMPro; // Required for TextMeshPro
using UnityEngine.SceneManagement; // Required for loading scenes

public class TextButtonHandler : MonoBehaviour
{
    private TextMeshProUGUI m_TextMesh;

    [Header("Button Colors")]
    public Color normalColor = Color.white;
    public Color highlightedColor = new Color(0.9f, 0.9f, 0.9f);
    public Color pressedColor = Color.grey;

    void Awake()
    {
        m_TextMesh = GetComponent<TextMeshProUGUI>();
        if (m_TextMesh != null)
        {
            m_TextMesh.color = normalColor;
        }
    }

    // ---- VISUAL FEEDBACK FUNCTIONS ----

    public void OnPointerEnter()
    {
        if (m_TextMesh != null) m_TextMesh.color = highlightedColor;
    }

    public void OnPointerExit()
    {
        if (m_TextMesh != null) m_TextMesh.color = normalColor;
    }

    public void OnPointerDown()
    {
        if (m_TextMesh != null) m_TextMesh.color = pressedColor;
    }

    public void OnPointerUp()
    {
        if (m_TextMesh != null) m_TextMesh.color = highlightedColor;
    }

    // ---- MENU ACTION FUNCTIONS ----

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
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

    // I renamed these for clarity to be more general-purpose
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("_Scenes/MainMenu");
    }

    public void GoToAbout()
    {
        SceneManager.LoadScene("_Scenes/About");
    }
}