using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragTower : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public TowerBlueprint Tower; // Assign in Inspector for each button
    private GameObject _slideIcon;
    private Canvas _canvas;
    private Camera _cam;

    private void Start()
    {
        _canvas = GetComponentInParent<Canvas>();
        _cam = Camera.main;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Tower == null) return;
        
        // Check funds
        if (GameManager.Instance.CurrentGold < Tower.cost) 
        {
             Debug.Log("Not enough gold to drag!");
             // Optional: visual feedback
             return;
        }


        BuildManager.Instance.SelectTowerToBuild(Tower);

        // Create Drag Icon
        _slideIcon = new GameObject("DragIcon");
        _slideIcon.transform.SetParent(_canvas.transform, false);
        _slideIcon.AddComponent<Image>().sprite = GetComponent<Image>().sprite;
        _slideIcon.GetComponent<Image>().raycastTarget = false; // Important
        
        // make it slightly transparent
        Color c = _slideIcon.GetComponent<Image>().color;
        c.a = 0.6f;
        _slideIcon.GetComponent<Image>().color = c;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_slideIcon != null)
        {
            _slideIcon.transform.position = eventData.position;
            
            // Raycast for hover icon
            Ray ray = _cam.ScreenPointToRay(eventData.position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000f))
            {
                Node node = hit.transform.GetComponent<Node>();
                if (node != null)
                {
                    bool fusionPossible = false;
                    if (node.turret != null)
                    {
                        TowerBase t1 = node.turret.GetComponent<TowerBase>();
                        TowerBase t2 = Tower.prefab.GetComponent<TowerBase>();
                        if (t1 != null && t2 != null)
                        {
                            fusionPossible = FusionManager.Instance.GetFusionResult(t1.Element, t2.Element) != null;
                        }
                    }
                    BuildManager.Instance.UpdateSelectionIcon(node, fusionPossible);
                }
                else
                {
                    BuildManager.Instance.UpdateSelectionIcon(null);
                }
            }
            else
            {
                BuildManager.Instance.UpdateSelectionIcon(null);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_slideIcon != null) Destroy(_slideIcon);
        BuildManager.Instance.UpdateSelectionIcon(null);

        // Raycast to find Node
        Ray ray = _cam.ScreenPointToRay(eventData.position);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 1000f))
        {
            Node node = hit.transform.GetComponent<Node>();
            if (node != null)
            {
                BuildManager.Instance.TryFuseOrBuild(node, Tower);
            }
        }
        
        // Clear selection
        BuildManager.Instance.SelectTowerToBuild(null);
    }
}
