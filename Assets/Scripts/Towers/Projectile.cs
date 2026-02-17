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
    private float _spreadingRadius;
    
    public float Speed = 10f;
    public float TargetOffset = 0.5f;
    public GameObject ImpactVFXPrefab;

    public GameObject TrailPrefab;
    public float TrailSpawnRate = 0.05f;
    private float _trailTimer = 0f;

    public void Seek(EnemyBase target, float damage, ElementType element, float burnDmg, float slowAmt, float slowDur, float explosionRadius = 0, float spreadingRadius = 0)
    {
        _target = target;
        _damage = damage;
        _element = element;
        _burnDmg = burnDmg;
        _slowAmt = slowAmt;
        _slowDur = slowDur;
        _explosionRadius = explosionRadius;
        _spreadingRadius = spreadingRadius;
    }

    private void Update()
    {
        if (_target == null || _target.IsDead)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPos = _target.transform.position + Vector3.up * TargetOffset;
        Vector3 dir = targetPos - transform.position;
        float distanceThisFrame = Speed * Time.deltaTime;

        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
        
        if (TrailPrefab != null)
        {
            _trailTimer += Time.deltaTime;
            if (_trailTimer >= TrailSpawnRate)
            {
                Instantiate(TrailPrefab, transform.position, transform.rotation);
                _trailTimer = 0f;
            }
        }

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
        else if (_spreadingRadius > 0)
        {
            Spread();
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
            if (enemy.IsDead) continue;
            if (Vector3.Distance(transform.position, enemy.transform.position) <= _explosionRadius)
            {
                ApplyEffects(enemy);
            }
        }
    }

    private void Spread()
    {
        ApplyEffects(_target);

        EnemyBase[] enemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        foreach (EnemyBase enemy in enemies)
        {
            if (enemy == _target || enemy.IsDead) continue;

            float dist = Vector3.Distance(_target.transform.position, enemy.transform.position);
            if (dist <= _spreadingRadius)
            {
                enemy.TakeDamage(_damage * 0.5f, _element);
                enemy.ApplyBurn(_burnDmg * 0.5f, 3f);
            }
        }
    }

    private void ApplyEffects(EnemyBase enemy)
    {
        enemy.TakeDamage(_damage, _element);

        if (_element == ElementType.Fire || _element == ElementType.FireIce || _element == ElementType.FireLightning || _element == ElementType.FireFire)
        {
            enemy.ApplyBurn(_burnDmg, 3f);
        }

        if (_element == ElementType.Ice || _element == ElementType.FireIce || _element == ElementType.IceLightning)
        {
            enemy.ApplySlow(_slowAmt, _slowDur);
        }
        
        if (_element == ElementType.IceIce)
        {
            enemy.ApplySlow(1f, _slowDur);
        }
    }
}
