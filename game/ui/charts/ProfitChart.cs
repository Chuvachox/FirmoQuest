using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BusinessSim;

// Deseneaza evolutia profitului si obiectivul campaniei.
public partial class ProfitChart : Control
{
    private readonly System.Collections.Generic.List<int> _values;
    private readonly int _target;

    public ProfitChart(System.Collections.Generic.List<int> values, int target)
    {
        _values = values ?? new System.Collections.Generic.List<int>();
        _target = target;
        CustomMinimumSize = new Vector2(0, 150);
        SizeFlagsHorizontal = SizeFlags.ExpandFill;
        MouseFilter = MouseFilterEnum.Ignore;
    }

    public override void _Ready()
    {
        Resized += QueueRedraw;
        QueueRedraw();
    }

    public override void _Draw()
    {
        float w = Size.X, h = Size.Y;
        if (w < 40 || h < 40 || _values.Count == 0)
            return;

        int maxV = System.Math.Max(1, _target);
        int minV = 0;
        foreach (var v in _values)
        {
            if (v > maxV)
                maxV = v;
            if (v < minV)
                minV = v;
        }

        float span = System.Math.Max(1, maxV - minV);
        float Y(float v) => h - 6f - (v - minV) / span * (h - 12f);

        DrawLine(new Vector2(0, Y(_target)), new Vector2(w, Y(_target)), ThemeColors.Gold, 1.5f);
        DrawLine(new Vector2(0, Y(0)), new Vector2(w, Y(0)), ThemeColors.Border, 1f);

        int n = _values.Count;
        float bw = w / n;
        int prev = 0;
        for (int i = 0; i < n; i++)
        {
            int v = _values[i];
            float top = Y(System.Math.Max(v, 0));
            float bottom = Y(System.Math.Min(v, 0));
            var rect = new Rect2(i * bw + 2f, top,
                System.Math.Max(2f, bw - 4f),
                System.Math.Max(1f, bottom - top));
            DrawRect(rect, v >= prev ? ThemeColors.Green : ThemeColors.Red, true);
            prev = v;
        }
    }
}
