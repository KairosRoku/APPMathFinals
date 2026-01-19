using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TowerBase : MonoBehaviour
{
    [Header("Stats")]
    public float Range = 3f;
    public float Damage = 10f;
    public float FireRate = 1f;
    public ElementType Element;

    [Header("Visuals")]
    public Transform FirePoint;
    public GameObject ProjectilePrefab; // Optional: If projectile based
    public LineRenderer LaserLine; // Optional: If laser based (Lightning)
    
    // Status Effect Stats
    public float BurnDamage = 2f;
    public float SlowAmount = 0.3f; // 30% slow
    public float SlowDuration = 2f;
    
    private float _fireCountdown = 0f;
    private EnemyBase _target;

    private void Update()
    {
        if (_target == null || IsTargetOutOfRange())
        {
            UpdateTarget();
        }

        if (_target != null)
        {
            // Rotate towards target (2D)
            Vector3 dir = _target.transform.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward); // Assumes sprite faces right

            if (_fireCountdown <= 0f)
            {
                Shoot();
                _fireCountdown = 1f / FireRate;
            }
        }

        _fireCountdown -= Time.deltaTime;
        
        // Disable Laser if no target or not shooting
        if(LaserLine != null && (Time.timeScale == 0 || _target == null)) 
        {
            LaserLine.enabled = false;
        }
    }

    private void UpdateTarget()
    {
        EnemyBase[] enemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        float shortestDistance = Mathf.Infinity;
        EnemyBase nearestEnemy = null;

        foreach (EnemyBase enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && shortestDistance <= Range)
        {
            _target = nearestEnemy;
        }
        else
        {
            _target = null;
        }
    }
    
    private bool IsTargetOutOfRange()
    {
         if(_target == null) return true;
         return Vector3.Distance(transform.position, _target.transform.position) > Range;
    }

    private void Shoot()
    {
        // Attack Logic based on Element
        if (Element == ElementType.Lightning)
        {
            // Instant Hit
            ChainLightningAttack();
        }
        else if (ProjectilePrefab != null)
        {
            // Spawn Projectile
            GameObject projectileGO = Instantiate(ProjectilePrefab, FirePoint.position, FirePoint.rotation);
            Projectile projectile = projectileGO.GetComponent<Projectile>();
            
            if (projectile != null)
            {
                projectile.Seek(_target, Damage, Element, BurnDamage, SlowAmount, SlowDuration);
            }
        }
    }
    
    private void ChainLightningAttack()
    {
        if(LaserLine != null)
        {
             LaserLine.enabled = true;
             LaserLine.SetPosition(0, FirePoint.position);
             LaserLine.SetPosition(1, _target.transform.position);
             StartCoroutine(DisableLaserAfter(0.1f));
        }

        DealDamage(_target);

        // Chain Logic
        // Simple implementation: Find 1 neighbor near target
        Collider2D[] colliders = Physics2D.OverlapCircleAll(_target.transform.position, 3f);
        int chainCount = 0;
        foreach (var col in colliders)
        {
            if (chainCount >= 2) break; // Max chain
            
            EnemyBase enemy = col.GetComponent<EnemyBase>();
            if (enemy != null && enemy != _target)
            {
                DealDamage(enemy);
                chainCount++;
                // Visuals for chain would go here (complex for this snippet)
            }
        }
    }

    private void DealDamage(EnemyBase enemy)
    {
          enemy.TakeDamage(Damage, Element);
          if (Element == ElementType.Ice) enemy.ApplySlow(SlowAmount, SlowDuration);
          if (Element == ElementType.Fire) enemy.ApplyBurn(BurnDamage, 3f);
    }
    
    private IEnumerator DisableLaserAfter(float time)
    {
        yield return new WaitForSeconds(time);
        if(LaserLine != null) LaserLine.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Range);
    }
}
