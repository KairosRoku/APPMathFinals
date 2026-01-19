using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("Text Elements")]
    [Header("Text Elements")]
    public TextMeshProUGUI GoldText;
    public TextMeshProUGUI HealthText;
    public TextMeshProUGUI WaveText;

    [Header("Panels")]
    public GameObject PauseMenuConfig;
    public GameObject GameOverPanel;
    
    [Header("Effects")]
    public Image DamageVignette;

    private void Start()
    {
        GameManager.Instance.OnGoldChanged += UpdateGold;
        GameManager.Instance.OnHealthChanged += UpdateHealth;
        GameManager.Instance.OnDamageTaken += PlayDamageEffect;
        GameManager.Instance.OnGameLost += ShowGameOver;
        
        // Init
        UpdateGold(GameManager.Instance.CurrentGold);
        UpdateHealth(GameManager.Instance.CurrentHealth);
        
        if (PauseMenuConfig != null) PauseMenuConfig.SetActive(false);
         if (GameOverPanel != null) GameOverPanel.SetActive(false);
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
    private void UpdateGold(int newAmount) 
    {
        if (GoldText == null) return;
        
        if (goldCoroutine != null) StopCoroutine(goldCoroutine);
        goldCoroutine = StartCoroutine(AnimateGold(int.Parse(GoldText.text), newAmount));
    }

    IEnumerator AnimateGold(int start, int end)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            int current = (int)Mathf.Lerp(start, end, elapsed / duration);
            GoldText.text = current.ToString();
            yield return null;
        }
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

    private void PlayDamageEffect()
    {
        if (DamageVignette != null)
        {
            StartCoroutine(FlashVignette());
        }
        // Camera Shake
        CameraShake.Instance.Shake(0.2f, 0.3f);
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
    }

    public void TogglePause()
    {
        bool isPaused = Time.timeScale == 0;
        Time.timeScale = isPaused ? 1 : 0;
        if (PauseMenuConfig != null) 
        {
            PauseMenuConfig.SetActive(!isPaused);
            // Simple scale animation
            if(!isPaused) StartCoroutine(AnimateScaleOpen(PauseMenuConfig.transform));
        }
    }
    
    IEnumerator AnimateScaleOpen(Transform target)
    {
        target.localScale = Vector3.zero;
        float t = 0;
        while (t < 0.2f)
        {
             t += Time.unscaledDeltaTime; // Unscaled because time is 0
             target.localScale = Vector3.one * Mathf.Lerp(0, 1, t/0.2f);
             yield return null;
        }
        target.localScale = Vector3.one;
    }
}
