using UnityEngine;
using System.Collections;

public class SmokeEffect : MonoBehaviour
{
    public float Duration = 1.0f;
    public float MaxScale = 3f;
    public Color TargetColor = new Color(0.5f, 0.5f, 0.5f, 0f);

    private SpriteRenderer _renderer;
    private Vector3 _startScale;
    private Color _startColor;

    private void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        if (_renderer == null) _renderer = GetComponentInChildren<SpriteRenderer>();
        
        _startScale = transform.localScale;
        if (_renderer != null) _startColor = _renderer.color;

        StartCoroutine(AnimateSmoke());
    }

    private IEnumerator AnimateSmoke()
    {
        float elapsed = 0f;
        while (elapsed < Duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / Duration;

            // Scale up
            transform.localScale = Vector3.Lerp(_startScale, _startScale * MaxScale, t);

            // Fade out and turn gray
            if (_renderer != null)
            {
                _renderer.color = Color.Lerp(_startColor, TargetColor, t);
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
