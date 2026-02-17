using UnityEngine;
using TMPro;
using System.Collections;

public class DamagePopup : MonoBehaviour
{
    public TextMeshProUGUI Text; 
    public float Duration = 0.8f;
    
    [Header("Burst Settings")]
    public float InitialBurstForce = 150f;
    public float Gravity = 400f;
    public Vector2 HorizontalSpread = new Vector2(-100f, 100f);

    private RectTransform _rectTransform;
    private Vector2 _velocity;

    public void Setup(float damage, Color color)
    {
        if (Text == null) Text = GetComponent<TextMeshProUGUI>();
        _rectTransform = GetComponent<RectTransform>();
        
        Text.text = Mathf.RoundToInt(damage).ToString();
        Text.color = color;

        // Initial Velocity: Burst up and randomly left/right
        // Randomizing both horizontal and vertical force for more variety
        _velocity = new Vector2(
            Random.Range(HorizontalSpread.x, HorizontalSpread.y),
            InitialBurstForce * Random.Range(0.8f, 1.2f)
        );

        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        float elapsed = 0;
        Color startColor = Text.color;

        // "Juice": Scale pop at start
        Vector3 initialScale = Vector3.one * 0.5f;
        Vector3 peakScale = Vector3.one * 1.5f;
        Vector3 settleScale = Vector3.one;

        while (elapsed < Duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / Duration;

            // Manual Physics Arc
            if (_rectTransform != null)
            {
                _velocity.y -= Gravity * Time.deltaTime;
                _rectTransform.anchoredPosition += _velocity * Time.deltaTime;
            }

            // Enhanced Scaling logic
            if (t < 0.2f) // Pop phase
            {
                transform.localScale = Vector3.Lerp(initialScale, peakScale, t / 0.2f);
            }
            else if (t < 0.4f) // Settle phase
            {
                transform.localScale = Vector3.Lerp(peakScale, settleScale, (t - 0.2f) / 0.2f);
            }
            else // Drift/Fade phase
            {
                transform.localScale = settleScale;
            }

            // Alpha Fade (Standard Professional Curve)
            if (t > 0.5f)
            {
                float alphaT = (t - 0.5f) / 0.5f;
                Color c = startColor;
                c.a = Mathf.SmoothStep(1f, 0f, alphaT);
                Text.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
