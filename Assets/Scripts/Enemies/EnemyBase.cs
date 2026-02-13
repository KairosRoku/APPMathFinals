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
    public Slider HealthBar; // Changed from Scrollbar to Slider
    public GameObject CoinPrefab;
    public GameObject DamagePopupPrefab;

    private float _currentHP;
    private Transform[] _waypoints;
    private int _targetWaypointIndex;
    private float _baseSpeed;
    
    [Header("Status Visuals")]
    public GameObject FreezeOverlay; // Show an ice block or frost sprite
    public GameObject ShockOverlay;  // Show sparks or electricity sprite
    public Color FreezeColor = new Color(0.5f, 0.8f, 1f, 1f);
    public Color BurnColor = new Color(1f, 0.5f, 0.3f, 1f);
    public Color FireTypeColor = new Color(1f, 0.4f, 0.4f);
    public Color IceTypeColor = new Color(0.4f, 0.7f, 1f);
    public Color ElectricTypeColor = new Color(1f, 1f, 0.3f);

    private float _slowFactor = 1f;
    private float _slowTimer = 0f;
    private float _burnDamage = 0f;
    private float _burnTimer = 0f;
    private float _burnTickTimer = 0f;
    private float _shockTimer = 0f;
    
    private Color _originalColor;
    private Color _targetStatusColor;
    private bool _isDead = false;
    public bool ResistanceEnabled = true; // Added this to handle wave-specific resistance rules

    private void Start()
    {
        _currentHP = MaxHP;
        _baseSpeed = Speed;
        if(EnemyRenderer != null) 
        {
            _originalColor = EnemyRenderer.material.color;
            ApplyTypeVisuals();
        }

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

    private float _segmentT = 0f;
    private float _segmentLength = 1f;
    private int _segmentStartIndex = 0;

    private void HandleMovement()
    {
        if (_waypoints == null || _waypoints.Length == 0) return;
        if (PathController.Instance == null) return;

        PathType pathType = PathController.Instance.type;

        switch (pathType)
        {
            case PathType.Linear:
                HandleLinearMovement();
                break;
            case PathType.QuadraticBezier:
                HandleQuadraticMovement();
                break;
            case PathType.CubicBezier:
                HandleCubicMovement();
                break;
        }
    }

    private void HandleLinearMovement()
    {
        if (_targetWaypointIndex >= _waypoints.Length) return;

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

    private void HandleQuadraticMovement()
    {
        // Segments are 0-1-2, 2-3-4...
        if (_segmentStartIndex + 2 >= _waypoints.Length)
        {
            // If we can't form a quadratic segment, move linearly to the end if any points left
            if (_segmentStartIndex + 1 < _waypoints.Length)
            {
                _targetWaypointIndex = _segmentStartIndex + 1;
                HandleLinearMovement();
            }
            else
            {
                ReachEnd();
            }
            return;
        }

        Vector3 p0 = _waypoints[_segmentStartIndex].position;
        Vector3 p1 = _waypoints[_segmentStartIndex + 1].position;
        Vector3 p2 = _waypoints[_segmentStartIndex + 2].position;

        // Approximate length of the segment if not already set
        if (_segmentT == 0)
        {
            _segmentLength = BezierUtils.GetQuadraticLength(p0, p1, p2);
        }

        float step = Speed * _slowFactor * Time.deltaTime;
        _segmentT += step / _segmentLength;

        transform.position = BezierUtils.GetQuadraticBezierPoint(p0, p1, p2, _segmentT);

        if (_segmentT >= 1f)
        {
            _segmentT = 0;
            _segmentStartIndex += 2;
            if (_segmentStartIndex >= _waypoints.Length - 1)
            {
                ReachEnd();
            }
        }
    }

    private void HandleCubicMovement()
    {
        // Segments are 0-1-2-3, 3-4-5-6...
        if (_segmentStartIndex + 3 >= _waypoints.Length)
        {
            // Fallback to linear for remaining points
            if (_segmentStartIndex + 1 < _waypoints.Length)
            {
                _targetWaypointIndex = _segmentStartIndex + 1;
                HandleLinearMovement();
            }
            else
            {
                ReachEnd();
            }
            return;
        }

        Vector3 p0 = _waypoints[_segmentStartIndex].position;
        Vector3 p1 = _waypoints[_segmentStartIndex + 1].position;
        Vector3 p2 = _waypoints[_segmentStartIndex + 2].position;
        Vector3 p3 = _waypoints[_segmentStartIndex + 3].position;

        if (_segmentT == 0)
        {
            _segmentLength = BezierUtils.GetCubicLength(p0, p1, p2, p3);
        }

        float step = Speed * _slowFactor * Time.deltaTime;
        _segmentT += step / _segmentLength;

        transform.position = BezierUtils.GetCubicBezierPoint(p0, p1, p2, p3, _segmentT);

        if (_segmentT >= 1f)
        {
            _segmentT = 0;
            _segmentStartIndex += 3;
            if (_segmentStartIndex >= _waypoints.Length - 1)
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
        if (_isDead) return;

        float finalDamage = amount;

        // Resistance handling
        if (ResistanceEnabled && IsResistantTo(element))
        {
            finalDamage *= 0.5f;
        }

        _currentHP -= finalDamage;
        
        // Spawn Damage Popup
        SpawnDamagePopup(finalDamage, element);

        // Visual Feedback
        StartCoroutine(FlashColor());
        StartCoroutine(UpdateHealthUI());

        if (_currentHP <= 0)
        {
            Die();
        }
    }

    private bool IsResistantTo(ElementType element)
    {
        if (Type == EnemyType.Grunt)
        {
            return element == ElementType.Fire || element == ElementType.FireFire || 
                   element == ElementType.FireIce || element == ElementType.FireLightning;
        }
        if (Type == EnemyType.Runner)
        {
            return element == ElementType.Lightning || element == ElementType.LightningLightning || 
                   element == ElementType.FireLightning || element == ElementType.IceLightning;
        }
        if (Type == EnemyType.Tank)
        {
            return element == ElementType.Ice || element == ElementType.IceIce || 
                   element == ElementType.FireIce || element == ElementType.IceLightning;
        }
        // Boss has no resistances
        return false;
    }

    private void SpawnDamagePopup(float damage, ElementType element)
    {
        if (DamagePopupPrefab == null || HealthBar == null) return;

        // Get the World Canvas (it's the parent of the HealthBar slider)
        Canvas worldCanvas = HealthBar.GetComponentInParent<Canvas>();
        if (worldCanvas == null) return;

        GameObject popupGO = Instantiate(DamagePopupPrefab, worldCanvas.transform);
        
        // Since it's in a World Canvas, we might want it slightly offset from the bar
        // RectTransform of the popup should be reset/configured
        RectTransform rt = popupGO.GetComponent<RectTransform>();
        if(rt != null)
        {
            // Randomize spawn position to spread them out
            Vector2 randomOffset = new Vector2(Random.Range(-40f, 40f), Random.Range(-20f, 20f));
            rt.anchoredPosition = (Vector2.up * 50f) + randomOffset; 
            rt.localScale = Vector3.one;
        }

        DamagePopup popup = popupGO.GetComponent<DamagePopup>();
        
        if (popup != null)
        {
            Color dColor = Color.white;
            switch(element)
            {
                case ElementType.Fire:
                case ElementType.FireFire:
                    dColor = new Color(1f, 0.3f, 0.1f); // Orange/Red
                    break;
                case ElementType.Ice:
                case ElementType.IceIce:
                    dColor = new Color(0.3f, 0.6f, 1f); // Blue
                    break;
                case ElementType.Lightning:
                case ElementType.LightningLightning:
                    dColor = new Color(1f, 1f, 0.1f); // Yellow
                    break;
                case ElementType.FireIce:
                    dColor = new Color(0.7f, 0.5f, 1f); // Purple-ish
                    break;
                case ElementType.FireLightning:
                    dColor = new Color(1f, 0.6f, 0f); // Bright Orange
                    break;
                case ElementType.IceLightning:
                    dColor = new Color(0.2f, 1f, 0.8f); // Cyan
                    break;
            }
            popup.Setup(damage, dColor);
        }
    }

    public void SetType(EnemyType type)
    {
        ConfigureStats(type);
    }

    private void ApplyTypeVisuals()
    {
        if (EnemyRenderer == null) return;
        
        switch (Type)
        {
            case EnemyType.Grunt:
                _originalColor = Color.white;
                break;
            case EnemyType.Runner:
                _originalColor = new Color(0.4f, 1f, 0.4f, 1f); // Green for speed
                break;
            case EnemyType.Tank:
                _originalColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Grey for tank
                break;
            case EnemyType.Boss:
                _originalColor = new Color(1f, 0.2f, 0.8f, 1f); // Pink/Purple for boss
                break;
            default:
                _originalColor = Color.white;
                break;
        }
        
        EnemyRenderer.material.color = _originalColor;
    }

    public void ConfigureStats(EnemyType type)
    {
        Type = type;
        switch (type)
        {
            case EnemyType.Grunt:
                MaxHP = 50f;
                Speed = 4f;
                CoinValue = 5;
                break;
            case EnemyType.Runner:
                MaxHP = 30f;
                Speed = 7f;
                CoinValue = 8;
                break;
            case EnemyType.Tank:
                MaxHP = 200f;
                Speed = 2.5f;
                CoinValue = 15;
                break;
            case EnemyType.Boss:
                MaxHP = 1500f;
                Speed = 2f;
                CoinValue = 100;
                break;
        }
        _currentHP = MaxHP;
        _baseSpeed = Speed;
        ApplyTypeVisuals();
    }

    private void ReachEnd()
    {
        GameManager.Instance.ReduceHealth(1);
        if (WaveManager.Instance != null) WaveManager.Instance.NotifyEnemyDestroyed();
        Destroy(gameObject);
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

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

        if (WaveManager.Instance != null) WaveManager.Instance.NotifyEnemyDestroyed();
        
        Destroy(gameObject);
    }

    // Shader-based Flash Feedback
    private static readonly int FlashAmountID = Shader.PropertyToID("_FlashAmount");
    private static readonly int FlashColorID = Shader.PropertyToID("_FlashColor");

    private IEnumerator FlashColor()
    {
        if(EnemyRenderer == null) yield break;

        Material mat = EnemyRenderer.material;
        float t = 0;
        float duration = 0.2f;

        // Set flash color to red
        mat.SetColor(FlashColorID, Color.red);

        // Flash On
        while (t < 1f)
        {
            t += Time.deltaTime / (duration / 2);
            mat.SetFloat(FlashAmountID, Mathf.Lerp(0, 1, t));
            yield return null;
        }
        
        t = 0;
        // Flash Off
        while (t < 1f)
        {
            t += Time.deltaTime / (duration / 2);
            mat.SetFloat(FlashAmountID, Mathf.Lerp(1, 0, t));
            yield return null;
        }
        mat.SetFloat(FlashAmountID, 0);
    }

    // Lerp Health Bar
    private IEnumerator UpdateHealthUI()
    {
        if (HealthBar == null) yield break;

        float targetVal = _currentHP / MaxHP;
        float startVal = HealthBar.value;
        float t = 0;
        float duration = 0.2f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            HealthBar.value = Mathf.Lerp(startVal, targetVal, t);
            yield return null;
        }
        HealthBar.value = targetVal;
    }
}
