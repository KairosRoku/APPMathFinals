using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BuildButton : MonoBehaviour
{
    [Header("Tower Settings")]
    public TowerBlueprint Tower; // Assign the prefab and cost here

    private Button _button;

    private void Awake()
    {
        Debug.Log($"[BuildButton] Awake on {gameObject.name}");
    }

    private void Start()
    {
        _button = GetComponent<Button>();
        
        if (Tower == null) Debug.LogError($"[BuildButton] {gameObject.name} has NO Tower Blueprint assigned!");
        else if (Tower.prefab == null) Debug.LogError($"[BuildButton] {gameObject.name} has a Blueprint but NO Prefab assigned!");
        else Debug.Log($"[BuildButton] {gameObject.name} initialized with {Tower.prefab.name} (Cost: {Tower.cost})");

        _button.onClick.AddListener(SelectThisTower);
    }

    private void OnValidate()
    {
        if (Tower == null) return;
        if (Tower.prefab == null) Debug.LogWarning($"[BuildButton] {gameObject.name}: Tower Blueprint is missing a PREFAB!");
    }

    [ContextMenu("TEST: Select This Tower")]

    public void SelectThisTower()
    {
        if (Tower == null)
        {
            Debug.LogError($"[BuildButton] No Tower Blueprint assigned to {gameObject.name}! Drag a blueprint or fill the fields in the inspector.");
            return;
        }

        if (Tower.prefab == null)
        {
            Debug.LogError($"[BuildButton] The tower blueprint on {gameObject.name} has no Prefab assigned!");
            return;
        }

        Debug.Log($"[BuildButton] Selected {Tower.prefab.name} for {Tower.cost} gold.");
        
        if (BuildManager.Instance != null)
        {
            BuildManager.Instance.SelectTowerToBuild(Tower);
        }
        else
        {
            Debug.LogError("[BuildButton] BuildManager Instance not found!");
        }
    }
}
