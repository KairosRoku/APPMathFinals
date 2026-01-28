using UnityEngine;

public class Projectile : MonoBehaviour
{
    private EnemyBase _target;
    private float _damage;
    private ElementType _element;
    private float _burnDmg;
    private float _slowAmt;
    private float _slowDur;
    
    public float Speed = 10f;
    public GameObject ImpactVFXPrefab;

    public void Seek(EnemyBase target, float damage, ElementType element, float burnDmg, float slowAmt, float slowDur)
    {
        _target = target;
        _damage = damage;
        _element = element;
        _burnDmg = burnDmg;
        _slowAmt = slowAmt;
        _slowDur = slowDur;
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
        _target.TakeDamage(_damage, _element);
        
        // Apply Slow if Ice component exists
        if (_element == ElementType.Ice || _element == ElementType.LightningIce || _element == ElementType.FireIce)
        {
            _target.ApplySlow(_slowAmt, _slowDur);
        }
        
        // Apply Burn if Fire component exists
        if (_element == ElementType.Fire || _element == ElementType.LightningFire || _element == ElementType.FireIce)
        {
            _target.ApplyBurn(_burnDmg, 3f);
        }

        Destroy(gameObject);
    }
}
