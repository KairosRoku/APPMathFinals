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
        _target.TakeDamage(_damage, _element);
        
        if (_element == ElementType.Fire)
        {
            _target.ApplyBurn(_burnDmg, 3f);
        }
        else if (_element == ElementType.Ice)
        {
            _target.ApplySlow(_slowAmt, _slowDur);
        }

        Destroy(gameObject);
    }
}
