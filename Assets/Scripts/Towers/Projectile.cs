using UnityEngine;

public class Projectile : MonoBehaviour
{
    private EnemyBase _target;
    private float _damage;
    private ElementType _element;
    private float _burnDmg;
    private float _slowAmt;
    private float _slowDur;
    private float _explosionRadius;
    
    public float Speed = 10f;
    public GameObject ImpactVFXPrefab;

    [Header("Trail Settings")]
    public GameObject TrailPrefab;
    public float TrailSpawnRate = 0.05f;
    private float _trailTimer = 0f;

    public void Seek(EnemyBase target, float damage, ElementType element, float burnDmg, float slowAmt, float slowDur, float explosionRadius = 0)
    {
        _target = target;
        _damage = damage;
        _element = element;
        _burnDmg = burnDmg;
        _slowAmt = slowAmt;
        _slowDur = slowDur;
        _explosionRadius = explosionRadius;
    }

    private void Update()
    {
        if (_target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = _target.transform.position - transform.position;
        float distanceThisFrame = Speed * Time.deltaTime;

        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
        
        // Handle Trail
        if (TrailPrefab != null)
        {
            _trailTimer += Time.deltaTime;
            if (_trailTimer >= TrailSpawnRate)
            {
                Instantiate(TrailPrefab, transform.position, transform.rotation);
                _trailTimer = 0f;
            }
        }

        // Rotate (2D)
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void HitTarget()
    {
        if (ImpactVFXPrefab != null)
        {
            Instantiate(ImpactVFXPrefab, transform.position, Quaternion.identity);
        }

        if (_explosionRadius > 0)
        {
            Explode();
        }
        else
        {
            ApplyEffects(_target);
        }

        Destroy(gameObject);
    }

    private void Explode()
    {
        EnemyBase[] enemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        foreach (EnemyBase enemy in enemies)
        {
            if (Vector3.Distance(transform.position, enemy.transform.position) <= _explosionRadius)
            {
                ApplyEffects(enemy);
            }
        }
    }

    private void ApplyEffects(EnemyBase enemy)
    {
        enemy.TakeDamage(_damage, _element);

        // Fire status (DoT)
        if (_element == ElementType.Fire || _element == ElementType.FireIce || _element == ElementType.FireLightning || _element == ElementType.FireFire)
        {
            enemy.ApplyBurn(_burnDmg, 3f);
        }

        // Ice status (Slow)
        if (_element == ElementType.Ice || _element == ElementType.FireIce || _element == ElementType.IceLightning)
        {
            enemy.ApplySlow(_slowAmt, _slowDur);
        }
        
        // IceIce (Freeze)
        if (_element == ElementType.IceIce)
        {
            enemy.ApplySlow(1f, _slowDur); // 100% slow = Freeze
        }
    }
}
