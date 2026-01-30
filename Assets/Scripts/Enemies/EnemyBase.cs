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
        if (_isDead) return;

        float finalDamage = amount;

        // 50% Damage Reduction if enemy element matches tower element
        if (IsResistantTo(element))
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
        if (Type == EnemyType.Fire)
        {
            return element == ElementType.Fire || element == ElementType.FireFire || 
                   element == ElementType.FireIce || element == ElementType.FireLightning;
        }
        if (Type == EnemyType.Ice)
        {
            return element == ElementType.Ice || element == ElementType.IceIce || 
                   element == ElementType.FireIce || element == ElementType.IceLightning;
        }
        if (Type == EnemyType.Electric)
        {
            return element == ElementType.Lightning || element == ElementType.LightningLightning || 
                   element == ElementType.FireLightning || element == ElementType.IceLightning;
        }
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
        Type = type;
        ApplyTypeVisuals();
    }

    private void ApplyTypeVisuals()
    {
        if (EnemyRenderer == null) return;
        
        switch (Type)
        {
            case EnemyType.Fire:
                _originalColor = FireTypeColor;
                break;
            case EnemyType.Ice:
                _originalColor = IceTypeColor;
                break;
            case EnemyType.Electric:
                _originalColor = ElectricTypeColor;
                break;
            default:
                _originalColor = Color.white;
                break;
        }
        
        // Use property block if possible for better performance, but material.color is fine for now
        EnemyRenderer.material.color = _originalColor;
    }

    private void ReachEnd()
    {
        GameManager.Instance.ReduceHealth(1);
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
