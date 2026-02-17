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
        if (Instance == null) 
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public TextMeshProUGUI GoldText;
    public TextMeshProUGUI CountdownText;
    
    public Image HealthBarFillImage;
    public Image GoldIconImage;

    public Button StartWaveButton;
    public GameObject PauseButton;
    public GameObject ResumeButton;
    public Button[] RestartButtons; 
    public Button[] HomeButtons;

    public Image WaveIndicatorImage;
    public Sprite[] WaveSprites;

    public GameObject PauseMenuPanel;
    public GameObject SettingsPanel;
    public GameObject GameOverPanel;
    public GameObject VictoryPanel;
    
    public Image DamageVignette;

    private System.Collections.Generic.Stack<GameObject> _panelHistory = new System.Collections.Generic.Stack<GameObject>();
    private System.Collections.Generic.Dictionary<GameObject, Vector3> _initialScales = new System.Collections.Generic.Dictionary<GameObject, Vector3>();
    private Coroutine _healthCoroutine;
    private Color _originalCountdownColor;
    private Vector3 _originalCountdownScale;

    private void Start()
    {
        GameManager.Instance.OnGoldChanged += UpdateGold;
        GameManager.Instance.OnHealthChanged += UpdateHealth;
        GameManager.Instance.OnDamageTaken += PlayDamageEffect;
        GameManager.Instance.OnGameLost += ShowGameOver;
        GameManager.Instance.OnGameWon += ShowVictory;
        
        UpdateGold(GameManager.Instance.CurrentGold);
        UpdateHealth(GameManager.Instance.CurrentHealth);
        
        if (PauseMenuPanel != null) _initialScales[PauseMenuPanel] = PauseMenuPanel.transform.localScale;
        if (SettingsPanel != null) _initialScales[SettingsPanel] = SettingsPanel.transform.localScale;
        if (GameOverPanel != null) _initialScales[GameOverPanel] = GameOverPanel.transform.localScale;
        if (VictoryPanel != null) _initialScales[VictoryPanel] = VictoryPanel.transform.localScale;

        if (PauseMenuPanel != null) PauseMenuPanel.SetActive(false);
        if (SettingsPanel != null) SettingsPanel.SetActive(false);
        if (GameOverPanel != null) GameOverPanel.SetActive(false);
        if (VictoryPanel != null) VictoryPanel.SetActive(false);
        if (ResumeButton != null) ResumeButton.SetActive(false);
        if (PauseButton != null) PauseButton.SetActive(true);
        
        UpdateWave(1);
        
        if (CountdownText != null)
        {
            _originalCountdownColor = CountdownText.color;
            _originalCountdownScale = CountdownText.transform.localScale;
            CountdownText.text = "";
            
            CountdownText.alignment = TextAlignmentOptions.Center;
            RectTransform rect = CountdownText.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
            }
        }

        if (StartWaveButton != null)
        {
            StartWaveButton.onClick.AddListener(() => {
                if (WaveManager.Instance != null) WaveManager.Instance.StartNextWave();
            });
        }

        if (RestartButtons != null)
        {
            foreach (Button btn in RestartButtons)
            {
                if (btn != null) btn.onClick.AddListener(RestartLevel);
            }
        }

        if (HomeButtons != null)
        {
            foreach (Button btn in HomeButtons)
            {
                if (btn != null) btn.onClick.AddListener(BackToMainMenu);
            }
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
            GameManager.Instance.OnGameWon -= ShowVictory;
        }
        if (Instance == this) Instance = null;
    }

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
        if (HealthBarFillImage != null && GameManager.Instance != null)
        {
            float targetFill = (float)health / GameManager.Instance.StartingHealth;
            if (_healthCoroutine != null) StopCoroutine(_healthCoroutine);
            _healthCoroutine = StartCoroutine(AnimateHealthBar(targetFill));
        }
    }

    IEnumerator AnimateHealthBar(float targetFill)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        float startFill = HealthBarFillImage.fillAmount;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            HealthBarFillImage.fillAmount = Mathf.Lerp(startFill, targetFill, elapsed / duration);
            yield return null;
        }
        HealthBarFillImage.fillAmount = targetFill;
    }

    public void UpdateWave(int wave)
    {
        if (WaveIndicatorImage != null && WaveSprites != null && WaveSprites.Length > 0)
        {
            int index = wave - 1;
            if (index >= 0 && index < WaveSprites.Length)
            {
                WaveIndicatorImage.sprite = WaveSprites[index];
            }
        }
    }

    private int _lastCountdownValue = -1;

    public void UpdateCountdown(float timeLeft)
    {
        if (CountdownText == null) return;

        if (timeLeft > 0.1f && timeLeft <= 3.5f)
        {
            int currentVal = Mathf.CeilToInt(timeLeft);
            if (currentVal != _lastCountdownValue)
            {
                _lastCountdownValue = currentVal;
                CountdownText.text = currentVal.ToString();
                CountdownText.color = _originalCountdownColor;
                StartCoroutine(PopText(CountdownText.transform, 1.2f));
            }
        }
        else if (timeLeft > 3.5f || timeLeft <= 0)
        {
            if (timeLeft > 3.5f) CountdownText.text = "";
            _lastCountdownValue = -1;
        }
    }

    public void ShowGo()
    {
        if (CountdownText == null) return;
        CountdownText.text = "GO!";
        CountdownText.color = new Color(0.1f, 1f, 0.1f);
        StartCoroutine(PopText(CountdownText.transform, 1.5f));
        StartCoroutine(FadeOutText(CountdownText, 1f));
    }

    IEnumerator PopText(Transform target, float scaleMult = 1.2f)
    {
        Vector3 baseScale = _originalCountdownScale;
        target.localScale = baseScale * scaleMult;
        float elapsed = 0;
        float duration = 0.2f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            target.localScale = Vector3.Lerp(baseScale * scaleMult, baseScale, elapsed / duration);
            yield return null;
        }
        target.localScale = baseScale;
    }

    IEnumerator FadeOutText(TextMeshProUGUI text, float delay)
    {
        yield return new WaitForSeconds(delay);
        float duration = 0.5f;
        float elapsed = 0;
        Color baseColor = _originalCountdownColor;
        Color c = text.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1, 0, elapsed / duration);
            text.color = c;
            yield return null;
        }
        text.text = "";
        text.color = baseColor;
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
        if (GameOverPanel != null) 
        {
            GameOverPanel.SetActive(true);
            StartCoroutine(AnimateScaleOpen(GameOverPanel.transform));
        }
        Time.timeScale = 0;
    }

    private void ShowVictory()
    {
        if (VictoryPanel != null) 
        {
            VictoryPanel.SetActive(true);
            StartCoroutine(AnimateScaleOpen(VictoryPanel.transform));
        }
        Time.timeScale = 0;
    }

    public void TogglePause()
    {
        bool isPaused = Time.timeScale == 0;
        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        Time.timeScale = 0;
        _panelHistory.Clear();
        if (PauseMenuPanel != null) 
        {
            PauseMenuPanel.SetActive(true);
            StartCoroutine(AnimateScaleOpen(PauseMenuPanel.transform));
        }
        if (SettingsPanel != null) SettingsPanel.SetActive(false);

        if (PauseButton != null) PauseButton.SetActive(false);
        if (ResumeButton != null) 
        {
            ResumeButton.SetActive(true);
            ResumeButton.transform.SetAsLastSibling();
        }
    }

    public void Resume()
    {
        Time.timeScale = 1;
        _panelHistory.Clear();
        if (PauseMenuPanel != null) PauseMenuPanel.SetActive(false);
        if (SettingsPanel != null) SettingsPanel.SetActive(false);

        if (PauseButton != null) 
        {
            PauseButton.SetActive(true);
            PauseButton.transform.SetAsLastSibling();
        }
        if (ResumeButton != null) ResumeButton.SetActive(false);
    }

    public void OpenSettings()
    {
        if (PauseMenuPanel != null && PauseMenuPanel.activeSelf)
        {
            _panelHistory.Push(PauseMenuPanel);
            PauseMenuPanel.SetActive(false);
        }

        if (SettingsPanel != null) 
        {
            SettingsPanel.SetActive(true);
            StartCoroutine(AnimateScaleOpen(SettingsPanel.transform));
        }
    }

    public void GoBack()
    {
        if (_panelHistory.Count > 0)
        {
            if (SettingsPanel != null) SettingsPanel.SetActive(false);
            
            GameObject previousPanel = _panelHistory.Pop();
            previousPanel.SetActive(true);
            StartCoroutine(AnimateScaleOpen(previousPanel.transform));
        }
        else
        {
            Resume();
        }
    }

    public void CloseSettings()
    {
        GoBack();
    }

    public void BackToMainMenu()
    {
        if (TowerDefense.UI.UIManager.Instance != null) 
            TowerDefense.UI.UIManager.Instance.ReturnToMainMenu();
        else 
        {
            Time.timeScale = 1;
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    IEnumerator AnimateScaleOpen(Transform target)
    {
        Vector3 targetScale = Vector3.one;
        if (_initialScales.TryGetValue(target.gameObject, out Vector3 storedScale))
        {
            targetScale = storedScale;
        }

        target.localScale = Vector3.zero;
        float t = 0;
        while (t < 0.2f)
        {
             t += Time.unscaledDeltaTime;
             target.localScale = targetScale * Mathf.Lerp(0, 1, t/0.2f);
             yield return null;
        }
        target.localScale = targetScale;
    }
}
