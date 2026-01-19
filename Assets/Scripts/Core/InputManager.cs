using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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
        // Block input if clicking on UI
        if (EventSystem.current.IsPointerOverGameObject()) return;

        HandleNodeInteraction();
    }

    private void HandleNodeInteraction()
    {
        Vector3 mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0; // Force 2D plane

        Node foundNode = null;
        float nodeRadius = 0.5f; // Assuming 1x1 sprite, radius 0.5 checking distance

        // Optimized: Could use Grid lookup if we had coordinates, but array loop is fine for < 1000 nodes
        foreach (var node in _allNodes)
        {
            if (Vector3.Distance(mousePos, node.transform.position) <= nodeRadius)
            {
                foundNode = node;
                break;
            }
        }

        // Hover Logic
        if (foundNode != _currentNode)
        {
            if (_currentNode != null) _currentNode.OnHoverExit();
            _currentNode = foundNode;
            if (_currentNode != null) _currentNode.OnHoverEnter();
        }

        // Click Logic
        if (Input.GetMouseButtonDown(0) && _currentNode != null)
        {
            _currentNode.OnClick();
        }
    }
}
