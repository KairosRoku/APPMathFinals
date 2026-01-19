using UnityEngine;
using System.Collections.Generic;

public class PathController : MonoBehaviour
{
    public static PathController Instance;

    public Transform[] Waypoints;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void OnDrawGizmos()
    {
        if (Waypoints == null || Waypoints.Length < 2) return;

        Gizmos.color = Color.green;
        for (int i = 0; i < Waypoints.Length - 1; i++)
        {
            if (Waypoints[i] != null && Waypoints[i+1] != null)
            {
                Gizmos.DrawLine(Waypoints[i].position, Waypoints[i+1].position);
                Gizmos.DrawSphere(Waypoints[i].position, 0.2f);
            }
        }
        if (Waypoints[Waypoints.Length - 1] != null)
             Gizmos.DrawSphere(Waypoints[Waypoints.Length - 1].position, 0.2f);
    }
    
    public Transform GetStartPoint()
    {
        if(Waypoints.Length > 0) return Waypoints[0];
        return null;
    }
}
