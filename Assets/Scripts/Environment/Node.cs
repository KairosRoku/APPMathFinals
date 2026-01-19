using UnityEngine;
using UnityEngine.EventSystems;

public class Node : MonoBehaviour
{
    public Color hoverColor;
    public Color occupiedColor; // Optional: used if we want to show it's occupied
    
    [Header("References")]
    public SpriteRenderer rend; // Changed to SpriteRenderer for 2D

    [HideInInspector]
    public GameObject turret;

    private Color startColor;
    private BuildManager buildManager;

    private void Start()
    {
        if(rend == null) rend = GetComponent<SpriteRenderer>();
        startColor = rend.color;
        buildManager = BuildManager.Instance;
    }

    private void OnMouseEnter()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (!buildManager.CanBuild) return;

        rend.color = hoverColor;
    }

    private void OnMouseExit()
    {
        rend.color = startColor;
    }

    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (turret != null)
        {
            buildManager.SelectNode(this);
            // Fusion Integration
            if(FusionManager.Instance != null) FusionManager.Instance.SelectForFusion(this);
            return;
        }

        if (!buildManager.CanBuild) return;

        buildManager.BuildTurretOn(this);
    }
}
