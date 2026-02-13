using UnityEngine;

public static class BezierUtils
{
    public static Vector3 GetQuadraticBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return oneMinusT * oneMinusT * p0 + 2f * oneMinusT * t * p1 + t * t * p2;
    }

    public static Vector3 GetCubicBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return oneMinusT * oneMinusT * oneMinusT * p0 +
               3f * oneMinusT * oneMinusT * t * p1 +
               3f * oneMinusT * t * t * p2 +
               t * t * t * p3;
    }

    public static float GetQuadraticLength(Vector3 p0, Vector3 p1, Vector3 p2, int steps = 10)
    {
        float length = 0;
        Vector3 lastPos = p0;
        for (int i = 1; i <= steps; i++)
        {
            Vector3 currentPos = GetQuadraticBezierPoint(p0, p1, p2, i / (float)steps);
            length += Vector3.Distance(lastPos, currentPos);
            lastPos = currentPos;
        }
        return length;
    }

    public static float GetCubicLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int steps = 20)
    {
        float length = 0;
        Vector3 lastPos = p0;
        for (int i = 1; i <= steps; i++)
        {
            Vector3 currentPos = GetCubicBezierPoint(p0, p1, p2, p3, i / (float)steps);
            length += Vector3.Distance(lastPos, currentPos);
            lastPos = currentPos;
        }
        return length;
    }
}
