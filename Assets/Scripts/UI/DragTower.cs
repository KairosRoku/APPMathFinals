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

        GameManager.Instance.OnDamageTaken -= null; // Dummy check

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
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_slideIcon != null) Destroy(_slideIcon);

        // Raycast to find Node
        Ray ray = _cam.ScreenPointToRay(eventData.position);
        RaycastHit hit;
        
        Debug.Log($"DragTower: Raycasting from {eventData.position}");
        
        if (Physics.Raycast(ray, out hit, 1000f))
        {
            Debug.Log($"DragTower: Hit {hit.transform.name}");
            Node node = hit.transform.GetComponent<Node>();
            if (node != null)
            {
                Debug.Log($"DragTower: Found Node, attempting to build");
                if (BuildManager.Instance != null)
                {
                    BuildManager.Instance.BuildTurretOn(node);
                }
                else
                {
                    Debug.LogError("BuildManager.Instance is null!");
                }
            }
            else
            {
                Debug.Log($"DragTower: Hit object has no Node component");
            }
        }
        else
        {
            Debug.Log("DragTower: Raycast hit nothing");
        }
        
        // Clear selection
        BuildManager.Instance.SelectTowerToBuild(null);
    }
}
