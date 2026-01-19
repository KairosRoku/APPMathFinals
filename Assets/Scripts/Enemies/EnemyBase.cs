using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    public float MaxHP = 100f;
    public float Speed = 5f;
    public int CoinValue = 10;
    public EnemyType Type;

    [Header("References")]
    public SpriteRenderer EnemyRenderer;
    public Image HealthBarFill; // Must be assigned
    public GameObject CoinPrefab;

    private float _currentHP;
    private Transform[] _waypoints;
    private int _targetWaypointIndex;
    private float _baseSpeed;
    
    // Status Effects
    private float _slowFactor = 1f;
    private float _slowTimer = 0f;
    private float _burnDamage = 0f;
    private float _burnTimer = 0f;
    private float _burnTickTimer = 0f;
    
    private Color _originalColor;

    private void Start()
    {
        _currentHP = MaxHP;
        _baseSpeed = Speed;
        if(EnemyRenderer != null) _originalColor = EnemyRenderer.color;

        if (PathController.Instance != null)
        {
            _waypoints = PathController.Instance.Waypoints;
            if (_waypoints.Length > 0)
            {
                transform.position = _waypoints[0].position;
                _targetWaypointIndex = 1;
            }
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleStatusEffects();
    }

    private void HandleMovement()
    {
        if (_waypoints == null || _targetWaypointIndex >= _waypoints.Length) return;

        Transform target = _waypoints[_targetWaypointIndex];
        float step = Speed * _slowFactor * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            _targetWaypointIndex++;
            if (_targetWaypointIndex >= _waypoints.Length)
            {
                ReachEnd();
            }
        }
    }

    private void HandleStatusEffects()
    {
        // Slow Logic
        if (_slowTimer > 0)
        {
            _slowTimer -= Time.deltaTime;
            if (_slowTimer <= 0) _slowFactor = 1f;
        }

        // Burn Logic
        if (_burnTimer > 0)
        {
            _burnTimer -= Time.deltaTime;
            _burnTickTimer -= Time.deltaTime;
            if (_burnTickTimer <= 0)
            {
                TakeDamage(_burnDamage, ElementType.Fire);
                _burnTickTimer = 1f; // Tick every second
            }
        }
    }

    public void ApplySlow(float pct, float duration)
    {
        _slowFactor = 1f - pct;
        _slowTimer = duration;
    }

    public void ApplyBurn(float dps, float duration)
    {
        _burnDamage = dps;
        _burnTimer = duration;
    }

    public void TakeDamage(float amount, ElementType element)
    {
        _currentHP -= amount;
        
        // Visual Feedback
        StartCoroutine(FlashColor());
        StartCoroutine(UpdateHealthUI());

        if (_currentHP <= 0)
        {
            Die();
        }
    }

    private void ReachEnd()
    {
        GameManager.Instance.ReduceHealth(1);
        Destroy(gameObject);
    }

    private void Die()
    {
        if (CoinPrefab != null)
        {
            GameObject coin = Instantiate(CoinPrefab, transform.position, Quaternion.identity);
            Coin coinScript = coin.GetComponent<Coin>();
            if(coinScript != null) coinScript.SetValue(CoinValue);
        }
        else
        {
            // Fallback if no prefab, add gold immediately
            GameManager.Instance.AddGold(CoinValue);
        }
        Destroy(gameObject);
    }

    // Lerp Color Feedback
    private IEnumerator FlashColor()
    {
        if(EnemyRenderer == null) yield break;

        float t = 0;
        float duration = 0.2f;

        // To Red
        while (t < 1f)
        {
            t += Time.deltaTime / (duration/2);
            EnemyRenderer.color = Color.Lerp(_originalColor, Color.red, t);
            yield return null;
        }
        
        t = 0;
        // Back to Original
        while (t < 1f)
        {
            t += Time.deltaTime / (duration/2);
            EnemyRenderer.color = Color.Lerp(Color.red, _originalColor, t);
            yield return null;
        }
        EnemyRenderer.color = _originalColor;
    }

    // Lerp Health Bar
    private IEnumerator UpdateHealthUI()
    {
        if (HealthBarFill == null) yield break;

        float targetFill = _currentHP / MaxHP;
        float startFill = HealthBarFill.fillAmount;
        float t = 0;
        float duration = 0.2f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            HealthBarFill.fillAmount = Mathf.Lerp(startFill, targetFill, t);
            yield return null;
        }
        HealthBarFill.fillAmount = targetFill;
    }
}
