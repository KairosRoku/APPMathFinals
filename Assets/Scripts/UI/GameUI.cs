using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    [Header("Text Elements")]
    [Header("Text Elements")]
    public TextMeshProUGUI GoldText;
    public TextMeshProUGUI HealthText;
    public TextMeshProUGUI WaveText;
    public TextMeshProUGUI TimerText;
    
    [Header("Buttons")]
    public Button StartWaveButton;

    [Header("Panels")]
    public GameObject PauseMenuPanel;
    public GameObject SettingsPanel; // Sub-panel of Pause Menu
    public GameObject GameOverPanel;
    public GameObject VictoryPanel;
    
    [Header("Effects")]
    public Image DamageVignette;

    private void Start()
    {
        GameManager.Instance.OnGoldChanged += UpdateGold;
        GameManager.Instance.OnHealthChanged += UpdateHealth;
        GameManager.Instance.OnDamageTaken += PlayDamageEffect;
        GameManager.Instance.OnGameLost += ShowGameOver;
        GameManager.Instance.OnGameWon += ShowVictory;
        
        // Init
        UpdateGold(GameManager.Instance.CurrentGold);
        UpdateHealth(GameManager.Instance.CurrentHealth);
        
        if (PauseMenuPanel != null) PauseMenuPanel.SetActive(false);
        if (SettingsPanel != null) SettingsPanel.SetActive(false);
        if (GameOverPanel != null) GameOverPanel.SetActive(false);
        if (VictoryPanel != null) VictoryPanel.SetActive(false);
        
        if (StartWaveButton != null)
        {
            StartWaveButton.onClick.AddListener(() => {
                FindAnyObjectByType<WaveManager>().StartNextWave();
            });
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoldChanged -= UpdateGold;
            GameManager.Instance.OnHealthChanged -= UpdateHealth;
            GameManager.Instance.OnDamageTaken -= PlayDamageEffect;
            GameManager.Instance.OnGameLost -= ShowGameOver;
        }
    }

    // Lerp Gold for smooth feedback
    private Coroutine goldCoroutine;
    private int _currentVisualGold;

    private void UpdateGold(int newAmount) 
    {
        if (GoldText == null) return;
        if (goldCoroutine != null) StopCoroutine(goldCoroutine);
        goldCoroutine = StartCoroutine(AnimateGold(_currentVisualGold, newAmount));
    }

    IEnumerator AnimateGold(int start, int end)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _currentVisualGold = (int)Mathf.Lerp(start, end, elapsed / duration);
            GoldText.text = _currentVisualGold.ToString();
            yield return null;
        }
        _currentVisualGold = end;
        GoldText.text = end.ToString();
    }

    private void UpdateHealth(int health)
    {
        if (HealthText != null) HealthText.text = "HP: " + health;
    }

    public void UpdateWave(int wave)
    {
        if (WaveText != null) WaveText.text = "Wave: " + wave;
    }

    public void UpdateTimer(float time)
    {
        if (TimerText == null) return;
        if (time <= 0) { TimerText.text = ""; return; }
        TimerText.text = "Next Wave in: " + Mathf.CeilToInt(time) + "s";
    }

    private void PlayDamageEffect()
    {
        if (DamageVignette != null) StartCoroutine(FlashVignette());
        if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.2f, 0.3f);
    }

    IEnumerator FlashVignette()
    {
        Color c = DamageVignette.color;
        c.a = 0.5f;
        DamageVignette.color = c;
        float t = 0;
        while(t < 0.5f)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0.5f, 0f, t/0.5f);
            DamageVignette.color = c;
            yield return null;
        }
    }
    
    private void ShowGameOver()
    {
        if (GameOverPanel != null) GameOverPanel.SetActive(true);
        Time.timeScale = 0; // Pause game on game over
    }

    private void ShowVictory() // New method to show victory panel
    {
        if (VictoryPanel != null) VictoryPanel.SetActive(true);
        Time.timeScale = 0; // Pause game on victory
    }

    // --- Pause Menu Logic ---
    
    public void TogglePause()
    {
        bool isPaused = Time.timeScale == 0;
        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        Time.timeScale = 0;
        if (PauseMenuPanel != null) 
        {
            PauseMenuPanel.SetActive(true);
            StartCoroutine(AnimateScaleOpen(PauseMenuPanel.transform));
        }
        if (SettingsPanel != null) SettingsPanel.SetActive(false);
    }

    public void Resume()
    {
        Time.timeScale = 1;
        if (PauseMenuPanel != null) PauseMenuPanel.SetActive(false);
        if (SettingsPanel != null) SettingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        if (SettingsPanel != null) SettingsPanel.SetActive(true);
        if (PauseMenuPanel != null) PauseMenuPanel.SetActive(false);
    }

    public void CloseSettings()
    {
        if (SettingsPanel != null) SettingsPanel.SetActive(false);
        if (PauseMenuPanel != null) 
        {
            PauseMenuPanel.SetActive(true);
            // Optionally re-animate or just show
            PauseMenuPanel.transform.localScale = Vector3.one;
        }
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu"); // Transition to MainMenu scene
    }

    public void RestartLevel()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    IEnumerator AnimateScaleOpen(Transform target)
    {
        target.localScale = Vector3.zero;
        float t = 0;
        while (t < 0.2f)
        {
             t += Time.unscaledDeltaTime;
             target.localScale = Vector3.one * Mathf.Lerp(0, 1, t/0.2f);
             yield return null;
        }
        target.localScale = Vector3.one;
    }
}

