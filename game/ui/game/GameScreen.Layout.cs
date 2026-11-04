using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BusinessSim;

// Construieste scena, indicatorii campaniei si bara de navigare.
public partial class GameScreen
{
    public override void _Ready()
    {
        Locale.Changed += OnLocaleChanged;
        Ui.Fill(this);
        _businessWorld = new BusinessWorld(_session, SelectSlot, Navigate);
        Ui.Fill(_businessWorld);
        AddChild(_businessWorld);
        AddBottomAd("res://assets/ui/ad_gura_cainarului.png", left: true);
        AddBottomAd("res://assets/ui/ad_coca_cola.png", left: false);
        _sectionHost = new Control { MouseFilter = MouseFilterEnum.Ignore };
        Ui.Fill(_sectionHost);
        AddChild(_sectionHost);

        var hud = Ui.Page(22);
        hud.MouseFilter = MouseFilterEnum.Ignore;
        hud.AnchorRight = 1;
        hud.OffsetBottom = 116;
        AddChild(hud);
        var header = Ui.Column(8);
        header.MouseFilter = MouseFilterEnum.Ignore;
        hud.AddChild(header);
        var top = Ui.Row(18);
        top.MouseFilter = MouseFilterEnum.Ignore;
        top.AddChild(Ui.Icon(NavIcon("home"), 42));
        var brand = Ui.Column(0);
        brand.AddChild(Ui.Label(_session.Scenario.Id == "agro" ? "FERMA MEA" : "STUDIOUL MEU", 22));
        _dayLabel = Ui.Label("", 14, ThemeColors.TextDim);
        brand.AddChild(_dayLabel);
        top.AddChild(brand);
        top.AddChild(Ui.Spacer());
        top.AddChild(Ui.Icon("moneda", 30));
        _balanceLabel = Ui.Label("", 25);
        top.AddChild(_balanceLabel);
        var language = Ui.Button(Locale.Lang.ToUpperInvariant(), LanguageMenu, ButtonKind.Normal, 16, "action-language", 24);
        language.Name = "language-menu";
        language.CustomMinimumSize = new Vector2(88, OS.HasFeature("android") ? 56 : 42);
        language.TooltipText = Locale.T("Schimbă limba jocului");
        top.AddChild(language);
        header.AddChild(top);
        var objective = Ui.Row(12);
        _profitLabel = Ui.Label("", 15, ThemeColors.Gold);
        objective.AddChild(_profitLabel);
        _profitProgress = new ProgressBar
        {
            ShowPercentage = false,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(0, 8),
            SizeFlagsVertical = SizeFlags.ShrinkCenter,
            TooltipText = Locale.T("Profitul realizat fata de obiectivul campaniei.")
        };
        _profitProgress.AddThemeStyleboxOverride("background", Ui.PanelStyle(ThemeColors.PanelLight, ThemeColors.PanelLight, 4, 0, 0, 0));
        _profitProgress.AddThemeStyleboxOverride("fill", Ui.PanelStyle(Accent, Accent, 4, 0, 0, 0));
        objective.AddChild(_profitProgress);
        header.AddChild(objective);

        var dockCenter = new CenterContainer
        {
            MouseFilter = MouseFilterEnum.Ignore,
            AnchorTop = 1,
            AnchorRight = 1,
            AnchorBottom = 1,
            OffsetTop = -146,
            OffsetBottom = -34
        };
        AddChild(dockCenter);
        var dock = Ui.FloatingPanel(Accent, 10, 9);
        dock.Name = "scene-navigation";
        dockCenter.AddChild(dock);
        var nav = Ui.Row(7);
        dock.AddChild(nav);
        var menu = Ui.SceneButton("Meniu", "action-menu", "Pauza, salvare si meniul principal. Esc: inchide mai intai panoul activ.", PauseMenu);
        menu.CustomMinimumSize = new Vector2(86, 78);
        nav.AddChild(menu);
        nav.AddChild(new VSeparator());
        int shortcut = 1;
        foreach (var kv in _sectionTitles)
        {
            string key = kv.Key;
            var b = Ui.SceneButton(kv.Value, NavIcon(key), $"{shortcut++}: {SectionHint(key)}", () => Navigate(key));
            b.Name = "nav-" + key;
            _navigationButtons[key] = b;
            nav.AddChild(b);
        }
        nav.AddChild(new VSeparator());
        var end = Ui.SceneButton("Încheie ziua", "action-clock", "Avanseaza o zi: productia progreseaza si se platesc costurile zilnice.", OnEndDay);
        end.Name = "end-day";
        end.CustomMinimumSize = new Vector2(164, 78);
        Ui.ApplyButtonStyle(end, ButtonKind.Success);
        nav.AddChild(end);
        _statusLabel = Ui.Label(OS.HasFeature("android") ? "Atinge un obiect pentru productie. Meniurile sunt in bara de jos." : "Clic pe obiect: producție  |  Clic dreapta: acțiuni  |  1-6: meniuri", 14, ThemeColors.TextDim);
        _statusLabel.AnchorLeft = .13f;
        _statusLabel.AnchorRight = .87f;
        _statusLabel.AnchorTop = 1;
        _statusLabel.AnchorBottom = 1;
        _statusLabel.OffsetTop = -28;
        _statusLabel.OffsetBottom = -4;
        _statusLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _statusLabel.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        _statusLabel.ClipText = true;
        _statusLabel.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(_statusLabel);
        _session.Changed += RequestRefresh;
        BuildSection();
        Refresh();
        if (HasEvent)
            Callable.From(ShowEvent).CallDeferred();
    }

    private void AddBottomAd(string path, bool left)
    {
        if (!ResourceLoader.Exists(path))
            return;
        var tex = GD.Load<Texture2D>(path);
        if (tex == null)
            return;
        float h = OS.HasFeature("android") ? 60 : 112;
        float w = h * tex.GetSize().X / tex.GetSize().Y;
        var ad = new TextureRect
        {
            Texture = tex,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = MouseFilterEnum.Ignore, // nu blochează clicurile din spate
        };
        ad.AnchorTop = 1;
        ad.AnchorBottom = 1;
        ad.OffsetTop = OS.HasFeature("android") ? -218 : -146;
        ad.OffsetBottom = OS.HasFeature("android") ? -158 : -34;
        if (left)
        {
            ad.AnchorLeft = 0;
            ad.AnchorRight = 0;
            ad.OffsetLeft = 22;
            ad.OffsetRight = 22 + w;
        }
        else
        {
            ad.AnchorLeft = 1;
            ad.AnchorRight = 1;
            ad.OffsetLeft = -(22 + w);
            ad.OffsetRight = -22;
        }
        AddChild(ad);
    }
}
