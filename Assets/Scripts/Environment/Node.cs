using UnityEngine;
using UnityEngine.EventSystems;

public class Node : MonoBehaviour
{
    public Color hoverColor;
    public Color occupiedColor;
    public Renderer rend;

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
            buildManager.SelectNode(this);
            if(FusionManager.Instance != null) FusionManager.Instance.SelectForFusion(this);
            return;
        }

        if (!buildManager.CanBuild)
        {
            return;
        }

        buildManager.BuildTurretOn(this);
    }
}
