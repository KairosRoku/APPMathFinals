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
    public GameObject RangeIndicatorPrefab; // New field for range circle
    public GameObject SmokeVFXPrefab;
    
    private GameObject currentSelectionIcon;
    private GameObject currentRangeIndicator;

    public void SelectTowerToBuild(TowerBlueprint tower)
    {
        towerToBuild = tower;
        selectedNode = null;
        
        if (tower != null && tower.prefab != null)
            Debug.Log($"[BuildManager] Selected tower: {tower.prefab.name}");
        else if (tower == null)
            Debug.Log("[BuildManager] Tower selection cleared.");
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
        if (Instance == null) 
        {
            Instance = this;
            Debug.Log("[BuildManager] Instance initialized.");
        }
        
        int ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");
        if (ignoreLayer == -1) ignoreLayer = 2; // Fallback to Layer 2 which is usually Ignore Raycast

        if (SelectionIconPrefab != null)
        {
            currentSelectionIcon = Instantiate(SelectionIconPrefab);
            currentSelectionIcon.layer = ignoreLayer;
            // Also set children
            foreach (Transform child in currentSelectionIcon.GetComponentsInChildren<Transform>()) child.gameObject.layer = ignoreLayer;
            currentSelectionIcon.SetActive(false);
        }

        if (RangeIndicatorPrefab != null)
        {
            currentRangeIndicator = Instantiate(RangeIndicatorPrefab);
            currentRangeIndicator.layer = ignoreLayer;
            foreach (Transform child in currentRangeIndicator.GetComponentsInChildren<Transform>()) child.gameObject.layer = ignoreLayer;
            currentRangeIndicator.SetActive(false);
        }
    }

    private bool _isPulsing = false;
    private Coroutine _pulseCoroutine;

    public void UpdateSelectionIcon(Node node, bool isFusionPossible = false)
    {
        if (currentSelectionIcon == null) return;

        if (node == null)
        {
            if (currentSelectionIcon.activeSelf)
            {
                currentSelectionIcon.SetActive(false);
                if (currentRangeIndicator != null) currentRangeIndicator.SetActive(false);
                
                if (_isPulsing)
                {
                    if (_pulseCoroutine != null) StopCoroutine(_pulseCoroutine);
                    _isPulsing = false;
                    currentSelectionIcon.transform.localScale = Vector3.one;
                }
            }
            return;
        }

        if (!currentSelectionIcon.activeSelf) currentSelectionIcon.SetActive(true);
        currentSelectionIcon.transform.position = node.transform.position + Vector3.up * 0.1f;
        
        // Update Range Indicator
        if (currentRangeIndicator != null && towerToBuild != null)
        {
            if (!currentRangeIndicator.activeSelf) currentRangeIndicator.SetActive(true);
            currentRangeIndicator.transform.position = node.transform.position + Vector3.up * 0.05f;
            
            TowerBase towerBase = towerToBuild.prefab.GetComponent<TowerBase>();
            if (towerBase != null)
            {
                float diameter = towerBase.Range * 2f;
                currentRangeIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);
            }
        }

        // Handle Pulse Logic stable
        if (isFusionPossible)
        {
            if (!_isPulsing)
            {
                _isPulsing = true;
                if (_pulseCoroutine != null) StopCoroutine(_pulseCoroutine);
                _pulseCoroutine = StartCoroutine(PulseIcon(currentSelectionIcon.transform));
            }
        }
        else
        {
            if (_isPulsing)
            {
                _isPulsing = false;
                if (_pulseCoroutine != null) StopCoroutine(_pulseCoroutine);
                currentSelectionIcon.transform.localScale = Vector3.one;
            }
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
                        GameManager.Instance.SpendGold(draggedBlueprint.cost); // Corrected line
                        FuseTowers(node, fusedPrefab);
                        return; // Successfully fused, don't build!
                    }
                    else
                    {
                        Debug.Log("[BuildManager] Not enough gold for fusion!");
                        return; // Don't build if we attempted fusion but failed due to gold
                    }
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
            Debug.LogError("[BuildManager] Build failed: Node is null!");
            return;
        }

        if (node.turret != null)
        {
            Debug.LogWarning("[BuildManager] Cannot build: There is already a turret on this node!");
            return;
        }

        if (towerToBuild == null)
        {
            Debug.LogError("[BuildManager] Build failed: No tower selected! Make sure to select a tower from the UI first.");
            return;
        }

        if (towerToBuild.prefab == null)
        {
            Debug.LogError($"[BuildManager] Build failed: Prefab for {towerToBuild.cost} tower is null! Check your BuildButton assignment.");
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

