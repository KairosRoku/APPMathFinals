using UnityEngine;
using System.Collections;

public class SimpleVFX : MonoBehaviour
{
    public float Duration = 0.5f;
    public Vector3 StartScale = Vector3.one;
    public Vector3 EndScale = Vector3.one * 1.5f;
    public Color StartColor = Color.white;
    public Color EndColor = new Color(1, 1, 1, 0);

    private SpriteRenderer _sprite;

    private void Start()
    {
        _sprite = GetComponent<SpriteRenderer>();
        if (_sprite == null) _sprite = GetComponentInChildren<SpriteRenderer>();
        
        transform.localScale = StartScale;
        if (_sprite != null) _sprite.color = StartColor;

        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        float elapsed = 0f;
        while (elapsed < Duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / Duration;

            transform.localScale = Vector3.Lerp(StartScale, EndScale, t);

            if (_sprite != null)
            {
                _sprite.color = Color.Lerp(StartColor, EndColor, t);
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
