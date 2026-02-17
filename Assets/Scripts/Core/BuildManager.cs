using UnityEngine;
using System.Collections;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    private TowerBlueprint towerToBuild;
    private Node selectedNode;
    
    public bool CanBuild { get { return towerToBuild != null; } }
    public bool HasMoney { get { return GameManager.Instance.CurrentGold >= towerToBuild.cost; } }

    public GameObject SelectionIconPrefab;
    public GameObject RangeIndicatorPrefab;
    public GameObject SmokeVFXPrefab;
    
    private GameObject currentSelectionIcon;
    private GameObject currentRangeIndicator;

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

    public void ResetBuildManager()
    {
        towerToBuild = null;
        selectedNode = null;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Instance.SelectionIconPrefab = this.SelectionIconPrefab;
            Instance.RangeIndicatorPrefab = this.RangeIndicatorPrefab;
            Instance.SmokeVFXPrefab = this.SmokeVFXPrefab;

            Instance.ResetBuildManager();
            Destroy(this);
            return;
        }

        Instance = this;
        ResetBuildManager();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void EnsureVisualHelpers()
    {
        int ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");
        if (ignoreLayer == -1) ignoreLayer = 2;

        if (currentSelectionIcon == null && SelectionIconPrefab != null)
        {
            currentSelectionIcon = Instantiate(SelectionIconPrefab, new Vector3(0, 0.2f, 0), Quaternion.identity);
            currentSelectionIcon.layer = ignoreLayer;
            foreach (Transform child in currentSelectionIcon.GetComponentsInChildren<Transform>()) child.gameObject.layer = ignoreLayer;
            currentSelectionIcon.SetActive(false);
        }

        if (currentRangeIndicator == null && RangeIndicatorPrefab != null)
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
        EnsureVisualHelpers();
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
        currentSelectionIcon.transform.position = node.transform.position + Vector3.up * 0.2f;
        
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
            TowerBase existingTower = node.turret.GetComponent<TowerBase>();
            TowerBase draggedTowerComp = draggedBlueprint.prefab.GetComponent<TowerBase>();
            
            if (existingTower != null && draggedTowerComp != null)
            {
                GameObject fusedPrefab = FusionManager.Instance.GetFusionResult(existingTower.Element, draggedTowerComp.Element);
                if (fusedPrefab != null)
                {
                    if (GameManager.Instance.CurrentGold >= draggedBlueprint.cost)
                    {
                        GameManager.Instance.SpendGold(draggedBlueprint.cost);
                        FuseTowers(node, fusedPrefab);
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }
        
        BuildTurretOn(node);
    }

    private void FuseTowers(Node node, GameObject fusedPrefab)
    {
        if (SmokeVFXPrefab != null)
        {
            GameObject smoke = Instantiate(SmokeVFXPrefab, node.transform.position, Quaternion.identity);
            Destroy(smoke, 2f);
        }

        if (node.turret != null)
        {
            Destroy(node.turret);
        }

        GameObject newTower = Instantiate(fusedPrefab, node.transform.position, Quaternion.identity);
        node.turret = newTower;
        
        if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.25f, 0.1f);
    }

    public void BuildTurretOn(Node node)
    {
        if (node == null) return;
        if (node.turret != null) return;
        if (towerToBuild == null) return;
        if (towerToBuild.prefab == null) return;

        if (GameManager.Instance.SpendGold(towerToBuild.cost))
        {
            if (SmokeVFXPrefab != null)
            {
                GameObject smoke = Instantiate(SmokeVFXPrefab, node.transform.position, Quaternion.identity);
                Destroy(smoke, 2f);
            }

            GameObject turret = Instantiate(towerToBuild.prefab, node.transform.position, Quaternion.identity);
            node.turret = turret;
            
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.15f, 0.05f);
        }
    }
}
