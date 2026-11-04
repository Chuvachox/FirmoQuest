using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BusinessSim;

// Deseneaza evolutia profitului in panoul financiar.
public partial class PriceSparkline : Control
{
    private readonly List<int> _values;
    public PriceSparkline(List<int> values)
    {
        _values = new List<int>(values);
        MouseFilter = MouseFilterEnum.Ignore;
    }
    public override void _Ready() => Resized += QueueRedraw;
    public override void _Draw()
    {
        if (_values.Count < 2)
            return;
        float min = _values.Min(), max = _values.Max(), span = Math.Max(1, max - min);
        var points = new Vector2[_values.Count];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vector2(8 + i * (Size.X - 16) / (points.Length - 1), Size.Y - 10 - (_values[i] - min) / span * (Size.Y - 20));
        DrawLine(new Vector2(8, Size.Y - 8), new Vector2(Size.X - 8, Size.Y - 8), ThemeColors.Border, 1);
        DrawPolyline(points, ThemeColors.Green, 3, true);
        DrawCircle(points[^1], 4, ThemeColors.Gold);
    }
}
