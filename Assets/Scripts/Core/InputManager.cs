using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private Node[] _allNodes;
    private Node _currentNode;
    private Camera _cam;

    [Header("Hotkey Tower Blueprints")]
    public TowerBlueprint Tower1; // Usually Fire
    public TowerBlueprint Tower2; // Usually Ice
    public TowerBlueprint Tower3; // Usually Lightning

    private void Start()
    {
        _cam = Camera.main;
        _allNodes = FindObjectsByType<Node>(FindObjectsSortMode.None);
        Debug.Log($"[InputManager] Started. Found {_allNodes.Length} nodes.");
    }

    private void Update()
    {
        // Toggle Pause
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (GameUI.Instance != null) GameUI.Instance.TogglePause();
        }

        // Hotkeys for Tower Selection
        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame && Tower1 != null) 
            {
                Debug.Log("[InputManager] Hotkey 1: Selecting Tower 1");
                BuildManager.Instance.SelectTowerToBuild(Tower1);
            }
            if (Keyboard.current.digit2Key.wasPressedThisFrame && Tower2 != null) 
            {
                Debug.Log("[InputManager] Hotkey 2: Selecting Tower 2");
                BuildManager.Instance.SelectTowerToBuild(Tower2);
            }
            if (Keyboard.current.digit3Key.wasPressedThisFrame && Tower3 != null) 
            {
                Debug.Log("[InputManager] Hotkey 3: Selecting Tower 3");
                BuildManager.Instance.SelectTowerToBuild(Tower3);
            }
        }

        // Block input if clicking on UI
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

        // Hover Logic
        if (foundNode != _currentNode)
        {
            if (_currentNode != null) _currentNode.OnHoverExit();
            _currentNode = foundNode;
            if (_currentNode != null) _currentNode.OnHoverEnter();
        }

        // Click Logic
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (_currentNode != null)
            {
                Debug.Log($"[InputManager] Clicked Node: {_currentNode.gameObject.name}");
                _currentNode.OnClick();
            }
            else
            {
                Debug.Log("[InputManager] Clicked but no Node found under mouse.");
            }
        }
    }
}
