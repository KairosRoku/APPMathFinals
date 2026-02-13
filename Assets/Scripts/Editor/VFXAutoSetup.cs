using UnityEngine;
using UnityEditor;
using System.IO;

public class VFXAutoSetup : EditorWindow
{
    [MenuItem("Tools/Auto Setup Tower VFX")]
    public static void Setup()
    {
        // 1. Setup BuildManager
        BuildManager buildManager = GameObject.FindAnyObjectByType<BuildManager>();
        if (buildManager != null)
        {
            Undo.RecordObject(buildManager, "Setup BuildManager VFX");
            buildManager.SmokeVFXPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Smoke.prefab");
            buildManager.SelectionIconPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Range.prefab"); // Assuming Range is selection icon
            EditorUtility.SetDirty(buildManager);
            Debug.Log("[VFX Setup] BuildManager references updated.");
        }

        // 2. Setup Tower Prefabs
        string[] towerGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Towers" });
        foreach (string guid in towerGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            TowerBase tower = prefab.GetComponent<TowerBase>();
            
            if (tower != null)
            {
                Undo.RecordObject(tower, "Setup Tower VFX");
                
                // Assign VFX based on type/needs
                if (tower.Element == ElementType.Lightning || tower.Element == ElementType.LightningLightning)
                {
                    // Lightning towers usually use LineRenderer with LightningBolt shader
                    LineRenderer lr = tower.GetComponent<LineRenderer>();
                    if (lr != null)
                    {
                        lr.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/LightningBolt.mat");
                    }
                }
                else if (tower.Element == ElementType.Ice || tower.Element == ElementType.IceIce)
                {
                    tower.IcePulseSprite = tower.transform.Find("IcePulse")?.GetComponent<SpriteRenderer>();
                    if (tower.IcePulseSprite != null)
                    {
                        tower.IcePulseSprite.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/IcePulse.mat");
                    }
                }
                else if (tower.Element == ElementType.Fire || tower.Element == ElementType.FireFire)
                {
                    tower.ProjectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/FireProjectileVFX.prefab");
                }

                // Global Smoke/Muzzle/Impact slots if you want to auto-fill them
                if (tower.MuzzleFlashPrefab == null)
                    tower.MuzzleFlashPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Smoke.prefab"); // Using smoke as default muzzle for now
                
                if (tower.ImpactVFXPrefab == null)
                    tower.ImpactVFXPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Smoke.prefab");

                EditorUtility.SetDirty(tower);
            }
        }

        // 3. Setup Enemy Prefabs
        string[] enemyGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Enemies", "Assets" });
        Material flashMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/EnemyDamageFlash.mat");
        foreach (string guid in enemyGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            EnemyBase enemy = prefab.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                Undo.RecordObject(enemy, "Setup Enemy VFX");
                if (enemy.EnemyRenderer != null)
                {
                    // Assign the flash material
                    enemy.EnemyRenderer.sharedMaterial = flashMat;
                }
                EditorUtility.SetDirty(enemy);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[VFX Setup] All tower and enemy VFX references automatically implemented!");
    }
}
