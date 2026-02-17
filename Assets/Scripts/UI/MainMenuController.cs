using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public static MainMenuController Instance;

    public GameObject MainPanel;
    public GameObject SettingsPanel;

    public string GameSceneName = "SampleScene";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (Instance.gameObject != gameObject)
            {
                Destroy(Instance.gameObject);
            }
            else
            {
                Destroy(Instance);
            }
        }
        
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        Time.timeScale = 1;
        
        if (MainPanel == null) MainPanel = GameObject.Find("MainPanel");
        if (SettingsPanel == null) SettingsPanel = GameObject.Find("SettingsPanel");

        ShowMainPanel();
    }

    private void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (SettingsPanel != null && SettingsPanel.activeSelf)
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
        if (MainPanel != null) MainPanel.SetActive(false);
        if (SettingsPanel != null) SettingsPanel.SetActive(true);
    }

    public void ShowMainPanel()
    {
        if (MainPanel != null) MainPanel.SetActive(true);
        if (SettingsPanel != null) SettingsPanel.SetActive(false);
    }

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
