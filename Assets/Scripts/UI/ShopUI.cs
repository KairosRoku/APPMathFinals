using UnityEngine;

public class ShopUI : MonoBehaviour
{
    public TowerBlueprint FireTower;
    public TowerBlueprint IceTower;
    public TowerBlueprint LightningTower;
    
    public void SelectFire()
    {
        BuildManager.Instance.SelectTowerToBuild(FireTower);
    }
    
    public void SelectIce()
    {
        BuildManager.Instance.SelectTowerToBuild(IceTower);
    }
    
    public void SelectLightning()
    {
        BuildManager.Instance.SelectTowerToBuild(LightningTower);
    }
}
