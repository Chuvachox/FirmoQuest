using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BusinessSim;

// Paleta comuna si variantele vizuale ale butoanelor.
public static class ThemeColors
{
    private static Color C(string hex) => Hex.ToColor(hex, new Color(1f, 0f, 1f));

    public static readonly Color Bg = C("#11151b");
    public static readonly Color Panel = C("#1a2028");
    public static readonly Color PanelLight = C("#232b35");
    public static readonly Color PanelDark = C("#151a21");
    public static readonly Color Border = C("#2e3846");
    public static readonly Color Text = C("#e8edf3");
    public static readonly Color TextDim = C("#93a1b3");
    public static readonly Color TextDark = C("#6c7a8c");
    public static readonly Color Accent = C("#3d8bfd");
    public static readonly Color Green = C("#37a05a");
    public static readonly Color Red = C("#d0524a");
    public static readonly Color Gold = C("#e6b23c");
    public static readonly Color Purple = C("#8b6fd0");
}

public enum ButtonKind
{
    Normal, Primary, Success, Danger, Warning, Ghost
}
