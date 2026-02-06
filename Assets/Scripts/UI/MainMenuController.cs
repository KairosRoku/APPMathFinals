using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject MainPanel;
    public GameObject SettingsPanel;

    [Header("Scene Names")]
    public string GameSceneName = "SampleScene";

    private void Start()
    {
        ShowMainPanel();
    }

    private void Update()
    {
        // Support Escape key to go back to main menu if in a sub-panel
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (SettingsPanel.activeSelf)
            {
                ShowMainPanel();
            }
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(GameSceneName);
    }

    public void ShowSettings()
    {
        MainPanel.SetActive(false);
        SettingsPanel.SetActive(true);
    }

    public void ShowMainPanel()
    {
        if (MainPanel != null) MainPanel.SetActive(true);
        if (SettingsPanel != null) SettingsPanel.SetActive(false);
    }

    // Alias for ShowMainPanel to use on "Back" buttons
    public void GoBack()
    {
        ShowMainPanel();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
