using UnityEngine;
using System.Collections;

public class LightningBoltJitter : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    public int Segments = 5;
    public float JitterAmount = 0.2f;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    public void DrawBolt(Vector3 start, Vector3 end)
    {
        if (_lineRenderer == null) return;

        _lineRenderer.positionCount = Segments + 1;
        _lineRenderer.SetPosition(0, start);
        _lineRenderer.SetPosition(Segments, end);

        for (int i = 1; i < Segments; i++)
        {
            float t = (float)i / Segments;
            Vector3 pos = Vector3.Lerp(start, end, t);
            
            // Add perpendicular jitter
            Vector3 dir = (end - start).normalized;
            Vector3 perp = Vector3.Cross(dir, Vector3.forward).normalized;
            if (perp == Vector3.zero) perp = Vector3.up; // Fallback

            pos += perp * Random.Range(-JitterAmount, JitterAmount);
            _lineRenderer.SetPosition(i, pos);
        }
    }
}
