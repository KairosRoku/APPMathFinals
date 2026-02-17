using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

public class TowerBase : MonoBehaviour
{
    public float Range = 3f;
    public float Damage = 10f;
    public float FireRate = 1f;
    public ElementType Element;
    public float ExplosionRadius = 0f;

    public Transform FirePoint;
    public GameObject ProjectilePrefab; 
    public GameObject ImpactVFXPrefab; 
    public GameObject MuzzleFlashPrefab;
    public LineRenderer LaserLine; 
    public SpriteRenderer IcePulseSprite; 

    public AudioClip ShootSFX; 
    public AudioClip ImpactSFX; 
    public AudioClip SpecialSFX; 

    public float BurnDamage = 2f;
    public float SlowAmount = 0.3f; 
    public float SlowDuration = 2f;
    
    private float _fireCountdown = 0f;
    private EnemyBase _target;
    private List<LineRenderer> _chainLines = new List<LineRenderer>(); 
    private LightningBoltJitter _mainBoltJitter;

    private void Start()
    {
        if (LaserLine != null) _mainBoltJitter = LaserLine.GetComponent<LightningBoltJitter>();

        if (Element == ElementType.Ice)
        {
            FireRate = 1.0f;
        }
        else if (Element == ElementType.IceIce)
        {
            FireRate = 0.5f;
        }
    }

    private void Update()
    {
        if (_target == null || IsTargetOutOfRange() || _target.IsDead)
        {
            UpdateTarget();
        }

        if (_target != null)
        {
            if (_fireCountdown <= 0f)
            {
                Shoot();
                _fireCountdown = 1f / FireRate;
            }
        }

        _fireCountdown -= Time.deltaTime;
        
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
            if (enemy.IsDead) continue;
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
        if (MuzzleFlashPrefab != null && FirePoint != null)
        {
            GameObject flash = Instantiate(MuzzleFlashPrefab, FirePoint.position, FirePoint.rotation);
            Destroy(flash, 0.5f);
        }

        if (GameManager.Instance != null && GameManager.Instance.AudioManager != null)
        {
            if (ShootSFX != null) GameManager.Instance.AudioManager.PlaySFX(ShootSFX, 0.7f);
            else GameManager.Instance.AudioManager.PlaySFX("Fire", 0.7f);
        }

        if (Element == ElementType.Lightning || Element == ElementType.LightningLightning || 
            Element == ElementType.FireLightning || Element == ElementType.IceLightning)
        {
            ChainLightningAttack();
        }
        else if (Element == ElementType.Ice || Element == ElementType.IceIce)
        {
            IcePulseAttack();
        }
        else if (Element == ElementType.FireFire)
        {
            FireFireSpreadingAttack();
        }
        else if (ProjectilePrefab != null)
        {
            GameObject projectileGO = Instantiate(ProjectilePrefab, FirePoint.position, FirePoint.rotation);
            Projectile projectile = projectileGO.GetComponent<Projectile>();
            
            if (projectile != null)
            {
                projectile.Seek(_target, Damage, Element, BurnDamage, SlowAmount, SlowDuration, ExplosionRadius, 0f);
            }
        }
    }

    private void IcePulseAttack()
    {
         if (IcePulseSprite != null)
         {
             StartCoroutine(AnimateIcePulse());
         }
         
         if (GameManager.Instance != null && GameManager.Instance.AudioManager != null)
         {
             if (SpecialSFX != null) GameManager.Instance.AudioManager.PlaySFX(SpecialSFX, 0.8f);
             else GameManager.Instance.AudioManager.PlaySFX("Ice_01", 0.8f);
         }

         if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.1f, 0.03f);

         EnemyBase[] allEnemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
         foreach(var enemy in allEnemies)
         {
             if(enemy.IsDead) continue;
             if(Vector3.Distance(transform.position, enemy.transform.position) <= Range)
             {
                 if (Element == ElementType.IceIce)
                 {
                     enemy.ApplySlow(1f, 1.5f); 
                 }
                 else
                 {
                     enemy.ApplySlow(SlowAmount, 1.2f);
                 }
                 enemy.TakeDamage(Damage, Element); 
             }
         }
    }
    
    private void ChainLightningAttack()
    {
        if(LaserLine != null)
        {
             ApplyLightningVisuals(LaserLine);
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

        if (GameManager.Instance != null && GameManager.Instance.AudioManager != null)
        {
            if (ImpactSFX != null) GameManager.Instance.AudioManager.PlaySFX(ImpactSFX, 0.6f);
            else GameManager.Instance.AudioManager.PlaySFX("Lightning", 0.6f);
        }

        DealDamage(_target);
        SpawnImpactVFX(_target.transform.position);

        List<EnemyBase> hitEnemies = new List<EnemyBase> { _target };
        int maxChains = 2; 
        if (Element == ElementType.LightningLightning) maxChains = 99; 

        Vector3 lastHitPosition = _target.transform.position;
        EnemyBase currentSource = _target;

        for (int i = 0; i < maxChains; i++)
        {
            EnemyBase nextTarget = FindNextChainTarget(currentSource, hitEnemies);
            if (nextTarget == null)
            {
                ApplyLastChainEffects(currentSource);
                break;
            }

            CreateChainLine(lastHitPosition, nextTarget.transform.position);
            DealDamage(nextTarget);
            SpawnImpactVFX(nextTarget.transform.position);

            hitEnemies.Add(nextTarget);
            lastHitPosition = nextTarget.transform.position;
            currentSource = nextTarget;

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
            if (enemy.IsDead || alreadyHit.Contains(enemy)) continue;

            float dist = Vector3.Distance(source.transform.position, enemy.transform.position);
            if (dist <= 4f && dist < shortestDist) 
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
            SpawnImpactVFX(lastEnemy.transform.position);
            EnemyBase[] allEnemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
            foreach (EnemyBase enemy in allEnemies)
            {
                if (enemy.IsDead) continue;
                if (Vector3.Distance(lastEnemy.transform.position, enemy.transform.position) <= 2f) 
                {
                    enemy.TakeDamage(Damage * 0.5f, Element);
                    enemy.ApplyBurn(BurnDamage, 2f);
                }
            }
        }
        else if (Element == ElementType.IceLightning)
        {
            lastEnemy.ApplySlow(1f, SlowDuration);
        }
    }

    private void FireFireSpreadingAttack()
    {
        GameObject projectileGO = Instantiate(ProjectilePrefab, FirePoint.position, FirePoint.rotation);
        Projectile projectile = projectileGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Seek(_target, Damage, Element, BurnDamage, SlowAmount, SlowDuration, 0f, 2.5f);
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
        if (Element == ElementType.LightningLightning) actualDamage *= 2.5f;

        enemy.TakeDamage(actualDamage, Element);

        if (Element == ElementType.Ice || Element == ElementType.FireIce || Element == ElementType.IceLightning)
        {
            enemy.ApplySlow(SlowAmount, SlowDuration);
        }

        if (Element == ElementType.IceIce)
        {
            enemy.ApplySlow(1f, SlowDuration);
        }

        if (Element == ElementType.Fire || Element == ElementType.FireIce || Element == ElementType.FireLightning || Element == ElementType.FireFire)
        {
            enemy.ApplyBurn(BurnDamage, 3f);
        }

        if (Element == ElementType.Lightning || Element == ElementType.LightningLightning || 
            Element == ElementType.FireLightning || Element == ElementType.IceLightning)
        {
            enemy.ApplyShock(0.5f);
        }
    }
    
    private void CreateChainLine(Vector3 from, Vector3 to)
    {
        if (LaserLine == null) return;
        
        GameObject chainObj = new GameObject("ChainLine");
        LineRenderer chainLine = chainObj.AddComponent<LineRenderer>();
        
        chainLine.material = LaserLine.material;
        chainLine.startWidth = LaserLine.startWidth * 0.7f;
        chainLine.endWidth = LaserLine.endWidth * 0.7f;
        
        ApplyLightningVisuals(chainLine);
        
        chainLine.positionCount = 2;
        chainLine.useWorldSpace = true;
        
        chainLine.SetPosition(0, from);
        chainLine.SetPosition(1, to);
        
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
    
    private void ApplyLightningVisuals(LineRenderer line)
    {
        if (line == null) return;

        Color c1 = Color.white;
        Color c2 = Color.white;

        switch (Element)
        {
            case ElementType.Lightning:
            case ElementType.LightningLightning:
                c1 = new Color(0.4f, 0.7f, 1f);
                c2 = new Color(0.1f, 0.3f, 1f);
                break;
            case ElementType.FireLightning:
                c1 = new Color(1f, 0.8f, 0.2f);
                c2 = new Color(1f, 0.2f, 0f);
                break;
            case ElementType.IceLightning:
                c1 = new Color(0.8f, 1f, 1f);
                c2 = new Color(0.2f, 0.6f, 1f);
                break;
            default:
                c1 = Color.white;
                c2 = Color.cyan;
                break;
        }

        line.startColor = c1;
        line.endColor = c2;
        
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] { new GradientColorKey(c1, 0.0f), new GradientColorKey(c2, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 0.8f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        line.colorGradient = g;
    }

    private IEnumerator AnimateIcePulse()
    {
        if (IcePulseSprite == null) yield break;
        
        Color startColor = IcePulseSprite.color;
        startColor.a = 0.6f;
        IcePulseSprite.color = startColor;
        
        Vector3 startScale = Vector3.one * 0.5f;
        Vector3 endScale = Vector3.one * (Range * 2);
        
        IcePulseSprite.transform.localScale = startScale;
        IcePulseSprite.enabled = true;
        
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            IcePulseSprite.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            
            Color c = IcePulseSprite.color;
            c.a = Mathf.Lerp(0.6f, 0f, t);
            IcePulseSprite.color = c;
            
            yield return null;
        }
        
        IcePulseSprite.enabled = false;
        IcePulseSprite.transform.localScale = startScale;
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
