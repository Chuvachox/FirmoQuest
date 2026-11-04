using System;
using Godot;

namespace BusinessSim;

public partial class Main
{
    private double _safeAreaElapsed;

    private void FitDesktopWindow()
    {
        if (OS.HasFeature("android") || DisplayServer.GetName() == "headless")
            return;
        var window = GetWindow();
        if (window.Mode != Window.ModeEnum.Windowed)
            return;
        var area = DisplayServer.ScreenGetUsableRect(DisplayServer.WindowGetCurrentScreen());
        var available = new Vector2I(Math.Max(1, area.Size.X - 32), Math.Max(1, area.Size.Y - 64));
        window.MinSize = new Vector2I(Math.Min(960, available.X), Math.Min(540, available.Y));
        var fitted = new Vector2I(Math.Min(window.Size.X, available.X), Math.Min(window.Size.Y, available.Y));
        if (fitted == window.Size)
            return;
        window.Size = fitted;
        window.Position = area.Position + (area.Size - fitted) / 2;
    }

    internal static Rect2 SafeContentRect(Rect2 visible, Transform2D screenTransform, Rect2I safePixels)
    {
        if (safePixels.Size.X <= 0 || safePixels.Size.Y <= 0)
            return visible;
        var inverse = screenTransform.AffineInverse();
        Vector2 start = inverse * (Vector2)safePixels.Position;
        Vector2 end = inverse * (Vector2)(safePixels.Position + safePixels.Size);
        var safe = new Rect2(start, end - start).Intersection(visible);
        return safe.HasArea() ? safe : visible;
    }

    private void ApplySafeArea()
    {
        if (_host == null || !OS.HasFeature("android"))
            return;
        var viewport = GetViewport();
        var visible = viewport.GetVisibleRect();
        var safe = SafeContentRect(visible, viewport.GetScreenTransform(), DisplayServer.GetDisplaySafeArea());
        if (safe.Size.X > 32 && safe.Size.Y > 32)
            safe = safe.Grow(-8);
        float left = Mathf.Ceil(safe.Position.X - visible.Position.X);
        float top = Mathf.Ceil(safe.Position.Y - visible.Position.Y);
        float right = -Mathf.Ceil(visible.End.X - safe.End.X);
        float bottom = -Mathf.Ceil(visible.End.Y - safe.End.Y);
        if (Mathf.IsEqualApprox(_host.OffsetLeft, left) && Mathf.IsEqualApprox(_host.OffsetTop, top) &&
            Mathf.IsEqualApprox(_host.OffsetRight, right) && Mathf.IsEqualApprox(_host.OffsetBottom, bottom))
            return;
        _host.OffsetLeft = left;
        _host.OffsetTop = top;
        _host.OffsetRight = right;
        _host.OffsetBottom = bottom;
    }

    private void RefreshSafeArea(double delta)
    {
        if (!OS.HasFeature("android"))
            return;
        _safeAreaElapsed += delta;
        if (_safeAreaElapsed < .25)
            return;
        _safeAreaElapsed = 0;
        ApplySafeArea();
    }
}
