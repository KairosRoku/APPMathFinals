using UnityEngine;
using System.Collections;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    private TowerBlueprint towerToBuild;
    private Node selectedNode;
    
    // UI Reference
    // public NodeUI nodeUI; 

    public bool CanBuild { get { return towerToBuild != null; } }
    public bool HasMoney { get { return GameManager.Instance.CurrentGold >= towerToBuild.cost; } }

    public GameObject SelectionIconPrefab;
    public GameObject SmokeVFXPrefab;
    
    private GameObject currentSelectionIcon;

    public void SelectTowerToBuild(TowerBlueprint tower)
    {
        towerToBuild = tower;
        selectedNode = null;
    }

    public void SelectNode(Node node)
    {
        if (selectedNode == node)
        {
            DeselectNode();
            return;
        }

        selectedNode = node;
        towerToBuild = null;
    }

    public void DeselectNode()
    {
        selectedNode = null;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        
        if (SelectionIconPrefab != null)
        {
            currentSelectionIcon = Instantiate(SelectionIconPrefab);
            currentSelectionIcon.SetActive(false);
        }
    }

    public void UpdateSelectionIcon(Node node, bool isFusionPossible = false)
    {
        if (currentSelectionIcon == null) return;

        if (node == null)
        {
            currentSelectionIcon.SetActive(false);
            return;
        }

        currentSelectionIcon.SetActive(true);
        currentSelectionIcon.transform.position = node.transform.position + Vector3.up * 0.1f;
        
        if (isFusionPossible)
        {
            // Simple pulse animation logic could be here, but let's just use scale
            // To make it feel like an "animation" as requested:
            StopAllCoroutines(); 
            StartCoroutine(PulseIcon(currentSelectionIcon.transform));
        }
        else
        {
            StopAllCoroutines();
            currentSelectionIcon.transform.localScale = Vector3.one;
        }
    }

    private IEnumerator PulseIcon(Transform icon)
    {
        float t = 0;
        while (true)
        {
            t += Time.deltaTime * 5f;
            float s = 1.3f + Mathf.Sin(t) * 0.2f;
            icon.localScale = Vector3.one * s;
            yield return null;
        }
    }

    public void TryFuseOrBuild(Node node, TowerBlueprint draggedBlueprint)
    {
        if (node == null) return;

        if (node.turret != null)
        {
            // Try Fusion
            TowerBase existingTower = node.turret.GetComponent<TowerBase>();
            TowerBase draggedTowerComp = draggedBlueprint.prefab.GetComponent<TowerBase>();
            
            if (existingTower != null && draggedTowerComp != null)
            {
                GameObject fusedPrefab = FusionManager.Instance.GetFusionResult(existingTower.Element, draggedTowerComp.Element);
                if (fusedPrefab != null)
                {
                    if (GameManager.Instance.CurrentGold >= draggedBlueprint.cost) // Fusion cost same as tower placement for now or specific fusion cost
                    {
                        GameManager.Instance.SpendGold(draggedBlueprint.cost);
                        FuseTowers(node, fusedPrefab);
                    }
                    else
                    {
                        Debug.Log("Not enough money for fusion!");
                    }
                    return;
                }
            }
        }
        
        // If no fusion was done, try building
        BuildTurretOn(node);
    }

    private void FuseTowers(Node node, GameObject fusedPrefab)
    {
        // 1. Spawn Smoke VFX
        if (SmokeVFXPrefab != null)
        {
            GameObject smoke = Instantiate(SmokeVFXPrefab, node.transform.position, Quaternion.identity);
            Destroy(smoke, 2f); // Auto destroy
        }

        // 2. Clear existing turret
        if (node.turret != null)
        {
            Destroy(node.turret);
        }

        // 3. Spawn new fused turret
        GameObject newTower = Instantiate(fusedPrefab, node.transform.position, Quaternion.identity);
        node.turret = newTower;
        
        Debug.Log("Fusion Successful!");
    }

    public void BuildTurretOn(Node node)
    {
        if (node == null)
        {
            Debug.LogError("BuildManager: Node is null!");
            return;
        }

        if (node.turret != null)
        {
            Debug.Log("BuildManager: Node already occupied!");
            return;
        }

        if (towerToBuild == null)
        {
            Debug.LogError("BuildManager: No tower selected to build!");
            return;
        }

        Debug.Log($"BuildManager: Attempting to build {towerToBuild.prefab.name} for {towerToBuild.cost} gold");

        if (GameManager.Instance.SpendGold(towerToBuild.cost))
        {
            GameObject turret = Instantiate(towerToBuild.prefab, node.transform.position, Quaternion.identity);
            node.turret = turret;
            
            Debug.Log($"Tower Built at {node.transform.position}!");
        }
        else
        {
            Debug.Log($"Not enough Money! Need {towerToBuild.cost}, have {GameManager.Instance.CurrentGold}");
        }
    }
}

[System.Serializable]
public class TowerBlueprint
{
    public GameObject prefab;
    public int cost;
}
