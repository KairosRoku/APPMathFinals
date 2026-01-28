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
    public Renderer EnemyRenderer; // Can be SpriteRenderer (2D) or MeshRenderer (3D)
    public Scrollbar HealthBar; // Using Scrollbar for easy setup
    public GameObject CoinPrefab;

    private float _currentHP;
    private Transform[] _waypoints;
    private int _targetWaypointIndex;
    private float _baseSpeed;
    
    [Header("Status Visuals")]
    public GameObject FreezeOverlay; // Show an ice block or frost sprite
    public GameObject ShockOverlay;  // Show sparks or electricity sprite
    public Color FreezeColor = new Color(0.5f, 0.8f, 1f, 1f);
    public Color BurnColor = new Color(1f, 0.5f, 0.3f, 1f);

    private float _slowFactor = 1f;
    private float _slowTimer = 0f;
    private float _burnDamage = 0f;
    private float _burnTimer = 0f;
    private float _burnTickTimer = 0f;
    private float _shockTimer = 0f;
    
    private Color _originalColor;
    private Color _targetStatusColor;

    private void Start()
    {
        _currentHP = MaxHP;
        _baseSpeed = Speed;
        if(EnemyRenderer != null) _originalColor = EnemyRenderer.material.color;

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
        _targetStatusColor = _originalColor;

        // Slow / Freeze Visuals
        if (_slowTimer > 0)
        {
            _slowTimer -= Time.deltaTime;
            _targetStatusColor = Color.Lerp(_targetStatusColor, FreezeColor, 0.6f);
            if (FreezeOverlay != null) FreezeOverlay.SetActive(true);
            if (_slowTimer <= 0) 
            {
                _slowFactor = 1f;
                if (FreezeOverlay != null) FreezeOverlay.SetActive(false);
            }
        }

        // Burn Visuals
        if (_burnTimer > 0)
        {
            _burnTimer -= Time.deltaTime;
            _burnTickTimer -= Time.deltaTime;
            _targetStatusColor = Color.Lerp(_targetStatusColor, BurnColor, 0.4f);
            if (_burnTickTimer <= 0)
            {
                TakeDamage(_burnDamage, ElementType.Fire);
                _burnTickTimer = 1f; 
            }
        }

        // Shock Visuals
        if (_shockTimer > 0)
        {
            _shockTimer -= Time.deltaTime;
            if (ShockOverlay != null) ShockOverlay.SetActive(true);
            if (_shockTimer <= 0)
            {
                if (ShockOverlay != null) ShockOverlay.SetActive(false);
            }
        }

        // Apply constant color tint based on status
        if (_currentHP > 0 && EnemyRenderer != null)
        {
            EnemyRenderer.material.color = Color.Lerp(EnemyRenderer.material.color, _targetStatusColor, Time.deltaTime * 5f);
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

    public void ApplyShock(float duration)
    {
        _shockTimer = duration;
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
            EnemyRenderer.material.color = Color.Lerp(_originalColor, Color.red, t);
            yield return null;
        }
        
        t = 0;
        // Back to Original
        while (t < 1f)
        {
            t += Time.deltaTime / (duration/2);
            EnemyRenderer.material.color = Color.Lerp(Color.red, _originalColor, t);
            yield return null;
        }
        EnemyRenderer.material.color = _originalColor;
    }

    // Lerp Health Bar
    private IEnumerator UpdateHealthUI()
    {
        if (HealthBar == null) yield break;

        float targetSize = _currentHP / MaxHP;
        float startSize = HealthBar.size;
        float t = 0;
        float duration = 0.2f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            HealthBar.size = Mathf.Lerp(startSize, targetSize, t);
            yield return null;
        }
        HealthBar.size = targetSize;
    }
}
