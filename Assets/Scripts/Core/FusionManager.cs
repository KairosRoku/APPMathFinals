using UnityEngine;
using UnityEngine.UI;

public class FusionManager : MonoBehaviour
{
    public static FusionManager Instance;

    public Button FusionButton;
    private Node _nodeA;
    private Node _nodeB;

    [System.Serializable]
    public struct FusionRecipe
    {
        public ElementType ElementA;
        public ElementType ElementB;
        public GameObject ResultPrefab;
    }

    public FusionRecipe[] Recipes;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        if (FusionButton != null)
        {
            FusionButton.gameObject.SetActive(false);
            FusionButton.onClick.AddListener(DoFusion);
        }
    }

    public void SelectForFusion(Node node)
    {
        // Toggle selection logic
        if (_nodeA == node) 
        {
            _nodeA = null;
        }
        else if (_nodeB == node) 
        {
            _nodeB = null;
        }
        else if (_nodeA == null) 
        {
            _nodeA = node;
        }
        else if (_nodeB == null) 
        {
            _nodeB = node;
        }
        else 
        {
            // Both selected, maybe replace one?
            _nodeA = node;
            _nodeB = null;
        }

        CheckFusionPossible();
    }

    private void CheckFusionPossible()
    {
        bool possible = false;
        
        if (_nodeA != null && _nodeB != null && _nodeA.turret != null && _nodeB.turret != null)
        {
            TowerBase t1 = _nodeA.turret.GetComponent<TowerBase>();
            TowerBase t2 = _nodeB.turret.GetComponent<TowerBase>();

            // Check adjacency (Grid distance) OR just arbitrary selection as per GDD "Select together"
            // Assuming "Selected together" means sequential clicks.
            
            if (t1 != null && t2 != null)
            {
               GameObject result = GetFusionResult(t1.Element, t2.Element);
               if (result != null) possible = true;
            }
        }

        if (FusionButton != null) FusionButton.gameObject.SetActive(possible);
    }


    
    // Correction: Redo method with proper types
    public GameObject GetFusionResult(ElementType e1, ElementType e2)
    {
        foreach (var recipe in Recipes)
        {
            if ((recipe.ElementA == e1 && recipe.ElementB == e2) || (recipe.ElementA == e2 && recipe.ElementB == e1))
            {
                return recipe.ResultPrefab;
            }
        }
        return null;
    }

    public void DoFusion()
    {
         if (_nodeA == null || _nodeB == null) return;
         
         TowerBase t1 = _nodeA.turret.GetComponent<TowerBase>();
         TowerBase t2 = _nodeB.turret.GetComponent<TowerBase>();
         
         GameObject prefab = GetFusionResult(t1.Element, t2.Element);
         
         if (prefab != null)
         {
             // Cost? GDD says "Fusion requires coins". Let's assume arbitrary cost or sum
             int cost = 50; 
             if (!GameManager.Instance.SpendGold(cost)) return;

             // Destroy old
             Destroy(_nodeA.turret);
             Destroy(_nodeB.turret);
             
             _nodeA.turret = null; // Free up Node A
             
             // Build New on Node B (Arbitrary choice, or user choice)
             GameObject newTower = Instantiate(prefab, _nodeB.transform.position, Quaternion.identity);
             _nodeB.turret = newTower;
             
            // Reset Selection
            _nodeA = null;
            _nodeB = null;
            FusionButton.gameObject.SetActive(false);
         }
    }
}
