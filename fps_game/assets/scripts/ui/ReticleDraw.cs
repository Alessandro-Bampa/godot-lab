using Godot;
using System;
using System.Collections.Generic;

[GlobalClass][Tool]
public partial class ReticleDraw : Control
{
    private float _radius = 30.0f;
    [Export]
    public float Radius
    {
        get => _radius;
        set
        {
            _radius = value;
            UpdateCrosshair();
        }
    }

    // 2. THICKNESS
    private float _thickness = 1.8f;
    [Export]
    public float Thickness
    {
        get => _thickness;
        set
        {
            _thickness = value;
            UpdateCrosshair();
        }
    }

    // 3. COLOR
    private Color _color = Colors.White;
    [Export]
    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            UpdateCrosshair();
        }
    }

    // 4. GAP ANGLE
    private float _gapAngle = 45.8f;
    [Export]
    public float GapAngle
    {
        get => _gapAngle;
        set
        {
            _gapAngle = value;
            UpdateCrosshair();
        }
    }

    // 5. SEGMENTS
    private int _segments = 32;
    [Export]
    public int Segments
    {
        get => _segments;
        set
        {
            _segments = value;
            UpdateCrosshair();
        }
    }

    public override void _Draw()
    {
        this.DrawCircleCrosshair();
    }

    private void DrawCircleCrosshair()
    {
        float gapRad = Mathf.DegToRad(GapAngle);

        List<float[]> arcSegments = new List<float[]>
        {
            // Bottom-right quadrant (0 a 90)
            new float[] { gapRad / 2, Mathf.Pi / 2 - gapRad / 2 },
        
            // Bottom-left quadrant (90 a 180)
            new float[] { Mathf.Pi / 2 + gapRad / 2, Mathf.Pi - gapRad / 2 },
        
            // Top-left quadrant (180 a 270)
            new float[] { Mathf.Pi + gapRad / 2, 3 * Mathf.Pi / 2 - gapRad / 2 },
        
            // Top-right quadrant (270 a 360)
            new float[] { 3 * Mathf.Pi / 2 + gapRad / 2, Mathf.Tau - gapRad / 2 } // Mathf.Tau è 2*PI
        };

        foreach (float[] arc in arcSegments)
        {
            float startAngle = arc[0];
            float endAngle = arc[1];

            // Se il gap è troppo grande, l'angolo d'inizio supera la fine: saltiamo.
            if (startAngle >= endAngle) continue;

            var points = new List<Vector2>();
            float angleStep = (endAngle - startAngle) / Segments;

            for (int i = 0; i <= Segments; i++)
            {
                float angle = startAngle + (i * angleStep);
                float x = Radius * Mathf.Cos(angle);
                float y = Radius * Mathf.Sin(angle);
                points.Add(new Vector2(x, y));
            }
            if (points.Count > 1)
            {
                DrawPolyline(points.ToArray(), Color, Thickness, true);
            }
        }
    }

    private void UpdateCrosshair()
    {
        QueueRedraw();
    }
}
