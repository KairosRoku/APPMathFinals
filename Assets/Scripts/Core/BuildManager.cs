using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    private TowerBlueprint towerToBuild;
    private Node selectedNode;
    
    // UI Reference
    // public NodeUI nodeUI; 

    public bool CanBuild { get { return towerToBuild != null; } }
    public bool HasMoney { get { return GameManager.Instance.CurrentGold >= towerToBuild.cost; } }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void SelectTowerToBuild(TowerBlueprint tower)
    {
        towerToBuild = tower;
        selectedNode = null;
        // nodeUI.Hide();
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

        // nodeUI.SetTarget(node);
    }

    public void DeselectNode()
    {
        selectedNode = null;
        // nodeUI.Hide();
    }

    public void BuildTurretOn(Node node)
    {
        if (GameManager.Instance.SpendGold(towerToBuild.cost))
        {
            GameObject turret = Instantiate(towerToBuild.prefab, node.transform.position, Quaternion.identity);
            node.turret = turret;
            
            // Visual feedback
            Debug.Log("Tower Built!");
        }
        else
        {
            Debug.Log("Not enough Money!");
        }
    }
}

[System.Serializable]
public class TowerBlueprint
{
    public GameObject prefab;
    public int cost;
}
