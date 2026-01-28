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
    public GameObject ImpactVFXPrefab; // For instant hit/AOE effects
    public LineRenderer LaserLine; // For main Lightning line
    public SpriteRenderer IcePulseSprite; // For Ice AOE visual (optional)
    
    // Status Effect Stats
    public float BurnDamage = 2f;
    public float SlowAmount = 0.3f; // 30% slow
    public float SlowDuration = 2f;
    
    private float _fireCountdown = 0f;
    private EnemyBase _target;
    private List<LineRenderer> _chainLines = new List<LineRenderer>(); // For chain lightning visuals
    private LightningBoltJitter _mainBoltJitter;

    private void Start()
    {
        if (LaserLine != null) _mainBoltJitter = LaserLine.GetComponent<LightningBoltJitter>();
    }

    private void Update()
    {
        if (_target == null || IsTargetOutOfRange())
        {
            UpdateTarget();
        }

        if (_target != null)
        {
            // Rotation disabled for 2.5D - towers stay facing forward
            // Vector3 dir = _target.transform.position - transform.position;
            // float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            // transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

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
        if (Element == ElementType.Lightning || Element == ElementType.LightningIce || Element == ElementType.LightningFire)
        {
            // Instant Hit / Chain
            ChainLightningAttack();
        }
        else if (Element == ElementType.Ice)
        {
            // Ice Pulse AOE
            IcePulseAttack();
        }
        else if (ProjectilePrefab != null)
        {
            // Spawn Projectile (Default for Fire, FireIce, etc.)
            GameObject projectileGO = Instantiate(ProjectilePrefab, FirePoint.position, FirePoint.rotation);
            Projectile projectile = projectileGO.GetComponent<Projectile>();
            
            if (projectile != null)
            {
                // Slow/Burn amounts can be passed or handled in DealDamage upon impact
                projectile.Seek(_target, Damage, Element, BurnDamage, SlowAmount, SlowDuration);
            }
        }
    }

    private void IcePulseAttack()
    {
         // Visual Pulse Effect
         if (IcePulseSprite != null)
         {
             StartCoroutine(AnimateIcePulse());
         }
         
         // Freeze enemies in small radius around tower
         EnemyBase[] allEnemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
         foreach(var enemy in allEnemies)
         {
             if(Vector3.Distance(transform.position, enemy.transform.position) <= Range)
             {
                 // Stun effect -> High Slow or Stop
                 // bloon style freeze usually stops them completely for a moment
                 enemy.ApplySlow(1f, 1f); // 100% slow for 1 sec
                 enemy.TakeDamage(Damage, Element); 
             }
         }
    }
    
    private void ChainLightningAttack()
    {
        // Main lightning line to primary target
        if(LaserLine != null)
        {
             LaserLine.enabled = true;
             if (_mainBoltJitter != null)
                _mainBoltJitter.DrawBolt(FirePoint.position, _target.transform.position);
             else
             {
                LaserLine.SetPosition(0, FirePoint.position);
                LaserLine.SetPosition(1, _target.transform.position);
             }
             StartCoroutine(DisableLaserAfter(0.15f));
        }

        DealDamage(_target);
        SpawnImpactVFX(_target.transform.position);

        // Chain Logic - Find nearby enemies and show chain lines
        EnemyBase[] allEnemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        int chainCount = 0;
        Vector3 lastHitPosition = _target.transform.position; // Chain from last hit enemy
        
        foreach (EnemyBase enemy in allEnemies)
        {
            if (chainCount >= 2) break; // Max 2 chains
            if (enemy == _target) continue;

            float dist = Vector3.Distance(lastHitPosition, enemy.transform.position);
            if (dist <= 3f) // 3f is the bounce range
            {
                // Create visual chain line
                CreateChainLine(lastHitPosition, enemy.transform.position);
                
                DealDamage(enemy);
                SpawnImpactVFX(enemy.transform.position);
                lastHitPosition = enemy.transform.position; // Next chain starts from this enemy
                chainCount++;
            }
        }
    }

    private void SpawnImpactVFX(Vector3 position)
    {
        if (ImpactVFXPrefab != null)
        {
            Instantiate(ImpactVFXPrefab, position, Quaternion.identity);
        }
    }

    private void DealDamage(EnemyBase enemy)
    {
          enemy.TakeDamage(Damage, Element);
          
          // Apply Slow if Ice component exists
          if (Element == ElementType.Ice || Element == ElementType.LightningIce || Element == ElementType.FireIce)
          {
              enemy.ApplySlow(SlowAmount, SlowDuration);
          }
          
          // Apply Burn if Fire component exists
          if (Element == ElementType.Fire || Element == ElementType.LightningFire || Element == ElementType.FireIce)
          {
              enemy.ApplyBurn(BurnDamage, 3f);
          }
    }
    
    private void CreateChainLine(Vector3 from, Vector3 to)
    {
        if (LaserLine == null) return;
        
        // Create a temporary line renderer for the chain
        GameObject chainObj = new GameObject("ChainLine");
        LineRenderer chainLine = chainObj.AddComponent<LineRenderer>();
        
        // Copy settings from main laser line
        chainLine.material = LaserLine.material;
        chainLine.startWidth = LaserLine.startWidth * 0.7f; // Slightly thinner for chains
        chainLine.endWidth = LaserLine.endWidth * 0.7f;
        chainLine.startColor = LaserLine.startColor;
        chainLine.endColor = LaserLine.endColor;
        chainLine.positionCount = 2;
        chainLine.useWorldSpace = true;
        
        chainLine.SetPosition(0, from);
        chainLine.SetPosition(1, to);
        
        // Store and destroy after brief display
        _chainLines.Add(chainLine);
        StartCoroutine(DestroyChainLineAfter(chainLine, 0.15f));
    }
    
    private IEnumerator DestroyChainLineAfter(LineRenderer line, float time)
    {
        yield return new WaitForSeconds(time);
        if (line != null)
        {
            _chainLines.Remove(line);
            Destroy(line.gameObject);
        }
    }
    
    private IEnumerator AnimateIcePulse()
    {
        if (IcePulseSprite == null) yield break;
        
        // Start invisible and small
        Color startColor = IcePulseSprite.color;
        startColor.a = 0.6f;
        IcePulseSprite.color = startColor;
        
        Vector3 startScale = Vector3.one * 0.5f;
        Vector3 endScale = Vector3.one * (Range * 2); // Scale to match range
        
        IcePulseSprite.transform.localScale = startScale;
        IcePulseSprite.enabled = true;
        
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Scale up
            IcePulseSprite.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            
            // Fade out
            Color c = IcePulseSprite.color;
            c.a = Mathf.Lerp(0.6f, 0f, t);
            IcePulseSprite.color = c;
            
            yield return null;
        }
        
        IcePulseSprite.enabled = false;
        IcePulseSprite.transform.localScale = startScale; // Reset
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
