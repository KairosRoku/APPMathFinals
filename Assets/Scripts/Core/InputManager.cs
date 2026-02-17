using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private Node[] _allNodes;
    private Node _currentNode;
    private Camera _cam;

    public TowerBlueprint Tower1;
    public TowerBlueprint Tower2; 
    public TowerBlueprint Tower3; 

    private void Start()
    {
        EnsureCamera();
        _allNodes = FindObjectsByType<Node>(FindObjectsSortMode.None);
    }

    private bool EnsureCamera()
    {
        if (_cam == null) 
        {
            _cam = Camera.main;
        }
        return _cam != null;
    }

    private void Update()
    {
        if (!EnsureCamera()) return;
        
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (GameUI.Instance != null) GameUI.Instance.TogglePause();
        }

        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame && Tower1 != null) 
            {
                BuildManager.Instance.SelectTowerToBuild(Tower1);
            }
            if (Keyboard.current.digit2Key.wasPressedThisFrame && Tower2 != null) 
            {
                BuildManager.Instance.SelectTowerToBuild(Tower2);
            }
            if (Keyboard.current.digit3Key.wasPressedThisFrame && Tower3 != null) 
            {
                BuildManager.Instance.SelectTowerToBuild(Tower3);
            }
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) 
        {
             return;
        }

        HandleNodeInteraction();
    }

    private void HandleNodeInteraction()
    {
        if (Mouse.current == null) return;
        
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = _cam.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        Node foundNode = null;

        if (Physics.Raycast(ray, out hit))
        {
            foundNode = hit.transform.GetComponent<Node>();
        }

        if (foundNode != _currentNode)
        {
            if (_currentNode != null) _currentNode.OnHoverExit();
            _currentNode = foundNode;
            if (_currentNode != null) _currentNode.OnHoverEnter();
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (_currentNode != null)
            {
                _currentNode.OnClick();
            }
        }
    }
}
