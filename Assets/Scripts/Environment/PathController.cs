using UnityEngine;
using System.Collections.Generic;

public enum PathType { Linear, QuadraticBezier, CubicBezier }

public class PathController : MonoBehaviour
{
    public static PathController Instance;

    public PathType type = PathType.Linear;
    public Transform[] Waypoints;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void OnDrawGizmos()
    {
        if (Waypoints == null || Waypoints.Length < 2) return;

        Gizmos.color = Color.green;
        
        switch (type)
        {
            case PathType.Linear:
                DrawLinearPath();
                break;
            case PathType.QuadraticBezier:
                DrawQuadraticPath();
                break;
            case PathType.CubicBezier:
                DrawCubicPath();
                break;
        }

        Gizmos.color = Color.white;
        foreach (var wp in Waypoints)
        {
            if (wp != null) Gizmos.DrawSphere(wp.position, 0.15f);
        }
    }

    private void DrawLinearPath()
    {
        for (int i = 0; i < Waypoints.Length - 1; i++)
        {
            if (Waypoints[i] != null && Waypoints[i+1] != null)
            {
                Gizmos.DrawLine(Waypoints[i].position, Waypoints[i+1].position);
            }
        }
    }

    private void DrawQuadraticPath()
    {
        if (Waypoints.Length < 3)
        {
            DrawLinearPath();
            return;
        }

        for (int i = 0; i < Waypoints.Length - 1; i += 2)
        {
            if (i + 2 >= Waypoints.Length) break;
            
            Vector3 p0 = Waypoints[i].position;
            Vector3 p1 = Waypoints[i+1].position;
            Vector3 p2 = Waypoints[i+2].position;

            Vector3 lastPos = p0;
            int steps = 20;
            for (int k = 1; k <= steps; k++)
            {
                Vector3 currentPos = BezierUtils.GetQuadraticBezierPoint(p0, p1, p2, k / (float)steps);
                Gizmos.DrawLine(lastPos, currentPos);
                lastPos = currentPos;
            }
        }
    }

    private void DrawCubicPath()
    {
        if (Waypoints.Length < 4)
        {
            DrawLinearPath();
            return;
        }

        for (int i = 0; i < Waypoints.Length - 1; i += 3)
        {
            if (i + 3 >= Waypoints.Length) break;

            Vector3 p0 = Waypoints[i].position;
            Vector3 p1 = Waypoints[i+1].position;
            Vector3 p2 = Waypoints[i+2].position;
            Vector3 p3 = Waypoints[i+3].position;

            Vector3 lastPos = p0;
            int steps = 20;
            for (int k = 1; k <= steps; k++)
            {
                Vector3 currentPos = BezierUtils.GetCubicBezierPoint(p0, p1, p2, p3, k / (float)steps);
                Gizmos.DrawLine(lastPos, currentPos);
                lastPos = currentPos;
            }
        }
    }
    
    public Transform GetStartPoint()
    {
        if(Waypoints.Length > 0) return Waypoints[0];
        return null;
    }
}
