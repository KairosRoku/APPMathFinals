using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BuildButton : MonoBehaviour
{
    public TowerBlueprint Tower;

    private Button _button;

    private void Start()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(SelectThisTower);
    }

    public void SelectThisTower()
    {
        if (Tower == null || Tower.prefab == null) return;
        
        if (BuildManager.Instance != null)
        {
            BuildManager.Instance.SelectTowerToBuild(Tower);
        }
    }
}
