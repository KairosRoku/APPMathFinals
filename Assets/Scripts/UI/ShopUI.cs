using UnityEngine;

public class ShopUI : MonoBehaviour
{
    public TowerBlueprint FireTower;
    public TowerBlueprint IceTower;
    public TowerBlueprint LightningTower;
    
    // Call these from UI Buttons
    public void SelectFire()
    {
        Debug.Log("Selected Fire Tower");
        BuildManager.Instance.SelectTowerToBuild(FireTower);
    }
    
    public void SelectIce()
    {
        Debug.Log("Selected Ice Tower");
        BuildManager.Instance.SelectTowerToBuild(IceTower);
    }
    
    public void SelectLightning()
    {
        Debug.Log("Selected Lightning Tower");
        BuildManager.Instance.SelectTowerToBuild(LightningTower);
    }
}
