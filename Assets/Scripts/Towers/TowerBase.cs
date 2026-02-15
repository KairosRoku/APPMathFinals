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
    public float ExplosionRadius = 0f; // For Fire AOE

    [Header("Visuals")]
    public Transform FirePoint;
    public GameObject ProjectilePrefab; // Optional: If projectile based
    public GameObject ImpactVFXPrefab; // For instant hit/AOE effects
    public GameObject MuzzleFlashPrefab; // NEW: Muzzle flash at fire point
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

        // Override stats based on element
        if (Element == ElementType.Ice)
        {
            FireRate = 1.0f; // Attack every 1 second
        }
        else if (Element == ElementType.IceIce)
        {
            FireRate = 0.5f; // Attack every 2 seconds
        }
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
        // Muzzle Flash
        if (MuzzleFlashPrefab != null && FirePoint != null)
        {
            GameObject flash = Instantiate(MuzzleFlashPrefab, FirePoint.position, FirePoint.rotation);
            Destroy(flash, 0.5f);
        }

        // Attack Logic based on Element
        if (Element == ElementType.Lightning || Element == ElementType.LightningLightning || 
            Element == ElementType.FireLightning || Element == ElementType.IceLightning)
        {
            // Instant Hit / Chain
            ChainLightningAttack();
        }
        else if (Element == ElementType.Ice || Element == ElementType.IceIce)
        {
            // Ice Pulse AOE
            IcePulseAttack();
        }
        else if (Element == ElementType.FireFire)
        {
            // Spreading Fire
            FireFireSpreadingAttack();
        }
        else if (ProjectilePrefab != null)
        {
            // Spawn Projectile (Default for Fire, FireIce, etc.)
            GameObject projectileGO = Instantiate(ProjectilePrefab, FirePoint.position, FirePoint.rotation);
            Projectile projectile = projectileGO.GetComponent<Projectile>();
            
            if (projectile != null)
            {
                // Slow/Burn amounts can be passed or handled in DealDamage upon impact
                projectile.Seek(_target, Damage, Element, BurnDamage, SlowAmount, SlowDuration, ExplosionRadius);
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
         
         if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.1f, 0.03f);

         // Slow/Freeze enemies in radius around tower
         EnemyBase[] allEnemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
         foreach(var enemy in allEnemies)
         {
             if(Vector3.Distance(transform.position, enemy.transform.position) <= Range)
             {
                 if (Element == ElementType.IceIce)
                 {
                     // Stun: 100% slow for 1.5 seconds (shorter than the 2s cooldown)
                     enemy.ApplySlow(1f, 1.5f); 
                     Debug.Log($"[Tower] {gameObject.name} (IceIce) applied STUN to {enemy.name}");
                 }
                 else
                 {
                     // Regular Slow: Apply configured slow for 1.2 seconds (longer than the 1s cooldown)
                     enemy.ApplySlow(SlowAmount, 1.2f);
                 }
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

        if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.05f, 0.02f);

        DealDamage(_target);
        SpawnImpactVFX(_target.transform.position);

        // Chain Logic
        List<EnemyBase> hitEnemies = new List<EnemyBase> { _target };
        int maxChains = 2; // Default
        if (Element == ElementType.LightningLightning) maxChains = 99; // "Entire wave"

        Vector3 lastHitPosition = _target.transform.position;
        EnemyBase currentSource = _target;

        for (int i = 0; i < maxChains; i++)
        {
            EnemyBase nextTarget = FindNextChainTarget(currentSource, hitEnemies);
            if (nextTarget == null)
            {
                // Last enemy in chain effects
                ApplyLastChainEffects(currentSource);
                break;
            }

            CreateChainLine(lastHitPosition, nextTarget.transform.position);
            DealDamage(nextTarget);
            SpawnImpactVFX(nextTarget.transform.position);

            hitEnemies.Add(nextTarget);
            lastHitPosition = nextTarget.transform.position;
            currentSource = nextTarget;

            // If it's the last possible chain in the loop, apply effects
            if (i == maxChains - 1)
            {
                ApplyLastChainEffects(nextTarget);
            }
        }
    }

    private EnemyBase FindNextChainTarget(EnemyBase source, List<EnemyBase> alreadyHit)
    {
        EnemyBase[] allEnemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        EnemyBase nearest = null;
        float shortestDist = Mathf.Infinity;

        foreach (EnemyBase enemy in allEnemies)
        {
            if (alreadyHit.Contains(enemy)) continue;

            float dist = Vector3.Distance(source.transform.position, enemy.transform.position);
            if (dist <= 4f && dist < shortestDist) // 4f bounce range
            {
                shortestDist = dist;
                nearest = enemy;
            }
        }
        return nearest;
    }

    private void ApplyLastChainEffects(EnemyBase lastEnemy)
    {
        if (Element == ElementType.FireLightning)
        {
            // AOE on the last enemy
            SpawnImpactVFX(lastEnemy.transform.position);
            EnemyBase[] allEnemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
            foreach (EnemyBase enemy in allEnemies)
            {
                if (Vector3.Distance(lastEnemy.transform.position, enemy.transform.position) <= 2f) // Arbitrary AOE radius
                {
                    enemy.TakeDamage(Damage * 0.5f, Element);
                    enemy.ApplyBurn(BurnDamage, 2f);
                }
            }
        }
        else if (Element == ElementType.IceLightning)
        {
            // Slow + Freeze on the last enemy
            lastEnemy.ApplySlow(1f, SlowDuration); // Freeze
        }
    }

    private void FireFireSpreadingAttack()
    {
        // Fire projectile to first target
        GameObject projectileGO = Instantiate(ProjectilePrefab, FirePoint.position, FirePoint.rotation);
        Projectile projectile = projectileGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Seek(_target, Damage, Element, BurnDamage, SlowAmount, SlowDuration);
        }

        // AOE Spreading Logic: Hit all "adjacent" enemies in a radius
        EnemyBase[] allEnemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        foreach (EnemyBase enemy in allEnemies)
        {
            if (enemy == _target) continue;

            float dist = Vector3.Distance(_target.transform.position, enemy.transform.position);
            if (dist <= 2.5f) // "Adjacent" radius
            {
                // Apply immediate damage (50% effectiveness)
                enemy.TakeDamage(Damage * 0.5f, Element);
                
                // Apply spreading fire (50% effectiveness)
                enemy.ApplyBurn(BurnDamage * 0.5f, 3f);
                
                // Visual for spread
                CreateChainLine(_target.transform.position, enemy.transform.position);
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
        float actualDamage = Damage;
        if (Element == ElementType.LightningLightning) actualDamage *= 2.5f; // "High damage"

        enemy.TakeDamage(actualDamage, Element);

        // Apply Slow (Ice component)
        if (Element == ElementType.Ice || Element == ElementType.FireIce || Element == ElementType.IceLightning)
        {
            enemy.ApplySlow(SlowAmount, SlowDuration);
        }

        // Apply Freeze (IceIce)
        if (Element == ElementType.IceIce)
        {
            enemy.ApplySlow(1f, SlowDuration);
        }

        // Apply Burn (Fire component)
        if (Element == ElementType.Fire || Element == ElementType.FireIce || Element == ElementType.FireLightning || Element == ElementType.FireFire)
        {
            enemy.ApplyBurn(BurnDamage, 3f);
        }

        // Apply Shock (Lightning component)
        if (Element == ElementType.Lightning || Element == ElementType.LightningLightning || 
            Element == ElementType.FireLightning || Element == ElementType.IceLightning)
        {
            enemy.ApplyShock(0.5f);
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
        chainLine.startWidth = LaserLine.startWidth * 0.7f;
        chainLine.endWidth = LaserLine.endWidth * 0.7f;
        
        // Use the full gradient from the source
        chainLine.colorGradient = LaserLine.colorGradient;
        
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
