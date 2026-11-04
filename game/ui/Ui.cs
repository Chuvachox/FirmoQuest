using System;
using System.Collections.Generic;
using Godot;

namespace BusinessSim;

// Construieste controalele comune, stilurile si comportamentul de derulare.
public static class Ui
{
    public static StyleBoxFlat PanelStyle(Color bg, Color border, int radius = 8,
        int padH = 14, int padV = 8, int borderWidth = 1)
    {
        var sb = new StyleBoxFlat
        {
            BgColor = bg,
            BorderColor = border,
            BorderWidthTop = borderWidth,
            BorderWidthBottom = borderWidth,
            BorderWidthLeft = borderWidth,
            BorderWidthRight = borderWidth,
            CornerRadiusTopLeft = radius,
            CornerRadiusTopRight = radius,
            CornerRadiusBottomLeft = radius,
            CornerRadiusBottomRight = radius,
        };
        sb.ContentMarginLeft = padH;
        sb.ContentMarginRight = padH;
        sb.ContentMarginTop = padV;
        sb.ContentMarginBottom = padV;
        return sb;
    }

    private static StyleBoxFlat Clear()
    {
        var sb = new StyleBoxFlat { BgColor = new Color(0, 0, 0, 0) };
        sb.ContentMarginLeft = 0;
        sb.ContentMarginRight = 0;
        sb.ContentMarginTop = 0;
        sb.ContentMarginBottom = 0;
        return sb;
    }

    public static StyleBoxFlat Gilt(StyleBoxFlat src, Color color, int width = 2)
    {
        var sb = (StyleBoxFlat)src.Duplicate();
        sb.BorderColor = color;
        sb.BorderWidthTop = width;
        sb.BorderWidthBottom = width;
        sb.BorderWidthLeft = width;
        sb.BorderWidthRight = width;
        return sb;
    }

    public static void Glow(Control c, Color color, int width = 2)
    {
        string key = c is Button ? "normal" : "panel";
        if (c.GetThemeStylebox(key) is StyleBoxFlat cur)
            c.AddThemeStyleboxOverride(key, Gilt(cur, color, width));
    }

    public static Label Paragraph(string text, int size = 16, Color? color = null, bool bold = false)
    {
        var l = new Label
        {
            Text = Locale.T(text),
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
        };
        l.AddThemeFontSizeOverride("font_size", size);
        l.AddThemeColorOverride("font_color", color ?? ThemeColors.Text);
        if (bold)
            l.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.4f));
        return l;
    }

    public static Label Caption(string text, int size = 13) => Paragraph(text, size, ThemeColors.TextDim);

    public static Label Label(string text, int size = 16, Color? color = null)
    {
        var l = new Label
        {
            Text = Locale.T(text),
            AutowrapMode = TextServer.AutowrapMode.Off,
        };
        l.AddThemeFontSizeOverride("font_size", size);
        l.AddThemeColorOverride("font_color", color ?? ThemeColors.Text);
        return l;
    }

    public static Label Title(string text, int size = 26) => Paragraph(text, size, ThemeColors.Text);

    public static Button Button(string text, Action onClick, ButtonKind kind = ButtonKind.Normal,
                                int size = 15, string icon = null, int iconSize = 18)
    {
        var b = new Button { Text = Locale.T(text), FocusMode = Control.FocusModeEnum.None };
        if (OS.HasFeature("android"))
            b.CustomMinimumSize = new Vector2(0, 56);
        b.AddThemeFontSizeOverride("font_size", size);
        ApplyButtonStyle(b, kind);
        if (onClick != null)
            b.Pressed += onClick;
        if (icon != null && IconTex(icon) is Texture2D tex)
        {
            b.Icon = tex;
            b.AddThemeConstantOverride("icon_max_width", iconSize);
            b.AddThemeConstantOverride("icon_separation", 6);
        }
        return b;
    }

    public static Button SceneButton(string text, string icon, string hint, Action action)
    {
        var b = Button(text, action, ButtonKind.Normal, 16, icon, 32);
        b.IconAlignment = HorizontalAlignment.Center;
        b.VerticalIconAlignment = VerticalAlignment.Top;
        b.CustomMinimumSize = new Vector2(112, 78);
        b.TooltipText = Locale.T(hint);
        b.AddThemeConstantOverride("icon_separation", 5);
        return b;
    }

    public static PanelContainer FloatingPanel(Color accent, int padH = 22, int padV = 18)
    {
        var panel = Panel(padH, padV);
        var style = PanelStyle(new Color(.07f, .105f, .14f, .97f), accent.Darkened(.48f), 16, padH, padV);
        style.BorderWidthTop = 3;
        style.ShadowColor = new Color(0, 0, 0, .35f);
        style.ShadowSize = 18;
        style.ShadowOffset = new Vector2(0, 8);
        panel.AddThemeStyleboxOverride("panel", style);
        return panel;
    }

    public static void ApplyButtonStyle(Button b, ButtonKind kind)
    {
        Color bg, bgHover, border, font;

        switch (kind)
        {
            case ButtonKind.Primary:
                bg = ThemeColors.Accent;
                bgHover = ThemeColors.Accent.Lightened(0.12f);
                border = ThemeColors.Accent.Darkened(0.35f);
                font = new Color(1, 1, 1);
                break;
            case ButtonKind.Success:
                bg = ThemeColors.Green;
                bgHover = ThemeColors.Green.Lightened(0.12f);
                border = ThemeColors.Green.Darkened(0.35f);
                font = new Color(1, 1, 1);
                break;
            case ButtonKind.Danger:
                bg = ThemeColors.Red;
                bgHover = ThemeColors.Red.Lightened(0.12f);
                border = ThemeColors.Red.Darkened(0.35f);
                font = new Color(1, 1, 1);
                break;
            case ButtonKind.Warning:
                bg = ThemeColors.Gold;
                bgHover = ThemeColors.Gold.Lightened(0.1f);
                border = ThemeColors.Gold.Darkened(0.35f);
                font = new Color(0.08f, 0.07f, 0.03f);
                break;
            case ButtonKind.Ghost:
                bg = new Color(0, 0, 0, 0);
                bgHover = ThemeColors.PanelLight;
                border = ThemeColors.Border;
                font = ThemeColors.TextDim;
                break;
            default:
                bg = ThemeColors.PanelLight;
                bgHover = ThemeColors.PanelLight.Lightened(0.15f);
                border = ThemeColors.Border;
                font = ThemeColors.Text;
                break;
        }

        b.AddThemeStyleboxOverride("normal", PanelStyle(bg, border, 7, 12, 7));
        b.AddThemeStyleboxOverride("hover", PanelStyle(bgHover, border.Lightened(0.15f), 7, 12, 7));
        b.AddThemeStyleboxOverride("pressed", PanelStyle(bg.Darkened(0.15f), border, 7, 12, 7));
        b.AddThemeStyleboxOverride("disabled", PanelStyle(ThemeColors.PanelDark, ThemeColors.Border, 7, 12, 7));
        b.AddThemeColorOverride("font_color", font);
        b.AddThemeColorOverride("font_hover_color", new Color(1, 1, 1));
        b.AddThemeColorOverride("font_pressed_color", font.Darkened(0.2f));
        b.AddThemeColorOverride("font_disabled_color", ThemeColors.TextDark);
    }

    public static PanelContainer Panel(int padH = 16, int padV = 12, Color? bg = null)
    {
        var p = new PanelContainer();
        p.AddThemeStyleboxOverride("panel",
            PanelStyle(bg ?? ThemeColors.Panel, ThemeColors.Border, 10, padH, padV));
        return p;
    }

    public static (PanelContainer, VBoxContainer) Card(string title, Color accent, int titleSize = 15)
    {
        var panel = Panel();
        var col = new VBoxContainer();
        col.AddThemeConstantOverride("separation", 8);
        panel.AddChild(col);

        if (!string.IsNullOrEmpty(title))
        {
            var t = Paragraph(Locale.T(title).ToUpperInvariant(), titleSize, accent);
            t.AddThemeFontSizeOverride("font_size", titleSize);
            col.AddChild(t);
            col.AddChild(new HSeparator());
        }

        return (panel, col);
    }

    public static VBoxContainer Column(int separation = 8)
    {
        var v = new VBoxContainer();
        v.AddThemeConstantOverride("separation", separation);
        return v;
    }

    public static HBoxContainer Row(int separation = 8)
    {
        var h = new HBoxContainer();
        h.AddThemeConstantOverride("separation", separation);
        return h;
    }

    public static Control Spacer() =>
        new Control { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };

    public static Control VerticalSpacer(int height = 0) =>
        new Control { CustomMinimumSize = new Vector2(0, height) };

    public static ScrollContainer Scroll()
    {
        var s = new ScrollContainer
        {
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            ScrollDeadzone = 12,
        };
        s.ChildEnteredTree += EnableScrollInput;
        return s;
    }

    private static void EnableScrollInput(Node node)
    {
        if (node is not Control control || node is ScrollBar || node is ScrollContainer ||
            node.HasMeta("_firmoquest_scroll_input"))
            return;
        node.SetMeta("_firmoquest_scroll_input", true);
        if (control.MouseFilter != Control.MouseFilterEnum.Ignore)
            control.MouseFilter = Control.MouseFilterEnum.Pass;
        node.ChildEnteredTree += EnableScrollInput;
        foreach (var child in node.GetChildren())
            EnableScrollInput(child);
    }

    public static MarginContainer ScreenPage(Control screen, int margin)
    {
        var page = Page(margin);
        Fill(page);
        screen.AddChild(page);
        var scroll = Scroll();
        scroll.Name = "screen-scroll";
        page.AddChild(scroll);
        var content = new MarginContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            MouseFilter = Control.MouseFilterEnum.Pass,
        };
        scroll.AddChild(content);
        return content;
    }

    public static string TouchScrollHint => Locale.T("Glisează în sus sau în jos pentru a derula. Înapoi: închide.");

    public static MarginContainer Page(int margin = 22)
    {
        var m = new MarginContainer();
        m.AddThemeConstantOverride("margin_left", margin);
        m.AddThemeConstantOverride("margin_right", margin);
        m.AddThemeConstantOverride("margin_top", margin);
        m.AddThemeConstantOverride("margin_bottom", margin);
        return m;
    }

    public static void Fill(Control c)
    {
        c.AnchorLeft = 0f;
        c.OffsetLeft = 0f;
        c.AnchorTop = 0f;
        c.OffsetTop = 0f;
        c.AnchorRight = 1f;
        c.OffsetRight = 0f;
        c.AnchorBottom = 1f;
        c.OffsetBottom = 0f;
        c.GrowHorizontal = Control.GrowDirection.Both;
        c.GrowVertical = Control.GrowDirection.Both;
    }

    public static HBoxContainer LanguageRow(int size = 14)
    {
        var row = Row(6);
        foreach (var code in Locale.Codes)
        {
            bool active = code == Locale.Lang;
            var b = Button(Locale.Name(code), () => Locale.Set(code),
                active ? ButtonKind.Primary : ButtonKind.Ghost, size);
            b.Name = "lang-" + code;
            b.CustomMinimumSize = new Vector2(0, 34);
            b.TooltipText = Locale.T("Schimbă limba jocului");
            row.AddChild(b);
        }
        return row;
    }

    public static HBoxContainer Stat(string label, string value, Color? valueColor = null, int size = 15)
    {
        var row = Row(6);
        row.AddChild(Label(label, size, ThemeColors.TextDim));
        row.AddChild(Spacer());
        var v = Label(value, size, valueColor ?? ThemeColors.Text);
        v.HorizontalAlignment = HorizontalAlignment.Right;
        v.SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd;
        row.AddChild(v);
        return row;
    }

    public static string Money(int amount, string currency) =>
        $"{amount:N0} {currency}";

    public static string Signed(int amount, string currency) =>
        $"{(amount >= 0 ? "+" : "-")}{Math.Abs(amount):N0} {currency}";

    private static readonly Dictionary<string, Texture2D> _icons = new();

    internal static void ReleaseIcons()
    {
        foreach (var texture in _icons.Values)
            texture?.Dispose();
        _icons.Clear();
    }

    public static Texture2D IconTex(string name)
    {
        if (_icons.TryGetValue(name, out var t))
            return t;
        string path = $"res://assets/ui/icons/{name}.png";
        if (!ResourceLoader.Exists(path))
            path = $"res://assets/ui/icons/{name}.svg";
        t = ResourceLoader.Exists(path) ? GD.Load<Texture2D>(path) : null;
        _icons[name] = t;
        return t;
    }

    public static TextureRect Icon(string name, int size, Color? modulate = null)
    {
        var tr = new TextureRect
        {
            CustomMinimumSize = new Vector2(size, size),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        var tex = IconTex(name);
        if (tex != null)
            tr.Texture = tex;
        if (modulate is Color c)
            tr.Modulate = c;
        return tr;
    }

    public static Control Chip(string text, string icon = null, Color? color = null, int size = 13)
    {
        var p = Panel(9, 4, ThemeColors.PanelDark);
        var row = Row(5);
        if (icon != null)
        {
            var i = Icon(icon, 15);
            if (i.Texture != null)
                row.AddChild(i);
        }
        row.AddChild(Label(text, size, color ?? ThemeColors.TextDim));
        p.AddChild(row);
        return p;
    }

    public static HBoxContainer Stepper(int value, int max, Action<int> onChange, int width = 150)
    {
        var row = Row(0);
        row.AddThemeConstantOverride("separation", 0);
        row.CustomMinimumSize = new Vector2(width, 40);

        var minus = Button("-", null, ButtonKind.Normal, 17);
        minus.CustomMinimumSize = new Vector2(38, 40);
        var val = Label(value.ToString(), 16, ThemeColors.Text);
        val.HorizontalAlignment = HorizontalAlignment.Center;
        val.VerticalAlignment = VerticalAlignment.Center;
        val.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        var plus = Button("+", null, ButtonKind.Success, 17);
        plus.CustomMinimumSize = new Vector2(38, 40);

        int low = max >= 1 ? 1 : 0;
        void Set(int v)
        {
            value = Mathf.Clamp(v, low, Mathf.Max(low, max));
            val.Text = value.ToString();
            onChange?.Invoke(value);
        }

        minus.Pressed += () => Set(value - 1);
        plus.Pressed += () => Set(value + 1);
        minus.Disabled = max < 1;
        plus.Disabled = max < 1;

        row.AddChild(minus);
        row.AddChild(val);
        row.AddChild(plus);
        return row;
    }
}
