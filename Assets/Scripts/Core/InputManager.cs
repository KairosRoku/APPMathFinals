using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private Node[] _allNodes;
    private Node _currentNode;
    private Camera _cam;

    private void Start()
    {
        _cam = Camera.main;
        // Cache all nodes once at start. 
        // Note: If nodes are spawned dynamically, this needs to be recalled.
        _allNodes = FindObjectsByType<Node>(FindObjectsSortMode.None);
    }

    private void Update()
    {
        // Toggle Pause
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (GameUI.Instance != null) GameUI.Instance.TogglePause();
        }

        // Block input if clicking on UI
        if (EventSystem.current.IsPointerOverGameObject()) return;

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
        if (Mouse.current.leftButton.wasPressedThisFrame && _currentNode != null)
        {
            _currentNode.OnClick();
        }
    }
}
