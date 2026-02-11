using UnityEngine;
using UnityEngine.EventSystems;

public class Node : MonoBehaviour
{
    public Color hoverColor;
    public Color occupiedColor; // Optional: used if we want to show it's occupied
    
    [Header("References")]
    public Renderer rend; // Supports MeshRenderer for 3D Tiles

    [HideInInspector]
    public GameObject turret;

    private Color startColor;
    private BuildManager buildManager;

    private void Start()
    {
        if(rend == null) rend = GetComponent<Renderer>();
        startColor = rend.material.color;
        buildManager = BuildManager.Instance;
    }

    public void OnHoverEnter()
    {
        if (!buildManager.CanBuild) return;
        rend.material.color = hoverColor;
    }

    public void OnHoverExit()
    {
        rend.material.color = startColor;
    }

    public void OnClick()
    {
        if (turret != null)
        {
            Debug.Log($"[Node] Node {gameObject.name} is occupied by {turret.name}");
            buildManager.SelectNode(this);
            if(FusionManager.Instance != null) FusionManager.Instance.SelectForFusion(this);
            return;
        }

        if (!buildManager.CanBuild)
        {
            Debug.Log("[Node] Cannot build: No tower selected in BuildManager.");
            return;
        }

        Debug.Log($"[Node] Attempting to build on {gameObject.name}");
        buildManager.BuildTurretOn(this);
    }
}
