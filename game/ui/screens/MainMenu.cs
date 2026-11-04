using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BusinessSim;

// Afiseaza pornirea, continuarea partidei, limbile si starea actualizarilor.
public partial class MainMenu : Control
{
    private readonly Content _content;
    private readonly Action _onPlay;
    private readonly Action _onHelp;
    private readonly Action _onQuit;
    private readonly Action _onContinue;
    private readonly UpdateChecker _updates;

    public MainMenu(Content content, Action onPlay, Action onHelp, Action onQuit, Action onContinue = null, UpdateChecker updates = null)
    {
        _content = content;
        _onPlay = onPlay;
        _onHelp = onHelp;
        _onQuit = onQuit;
        _onContinue = onContinue;
        _updates = updates;
    }

    public override void _Ready()
    {
        Locale.Changed += OnLocaleChanged;
        Ui.Fill(this);
        var page = Ui.ScreenPage(this, 32);
        var row = Ui.Row(30);
        page.AddChild(row);
        var visual = Ui.Column(12);
        visual.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        row.AddChild(visual);
        visual.AddChild(Ui.VerticalSpacer(12));
        visual.AddChild(Ui.Label("CONSTRUIEȘTE CEVA AL TĂU", 17, ThemeColors.Gold));
        visual.AddChild(Ui.Title("De la prima idee\nla o afacere în creștere.", 36));
        var preview = new BusinessWorld(Session.NewGame(_content, "agro", "medium"));
        preview.SizeFlagsVertical = SizeFlags.ExpandFill;
        visual.AddChild(preview);
        visual.AddChild(Ui.Caption("Planifică. Produce. Dezvoltă. Fiecare zi este o decizie.", 19));
        var panel = Ui.Panel(24, 20);
        panel.CustomMinimumSize = new Vector2(500, 0);
        panel.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        row.AddChild(panel);
        var col = Ui.Column(10);
        col.CustomMinimumSize = new Vector2(440, 0);
        panel.AddChild(col);
        var adTex = GD.Load<Texture2D>("res://assets/ui/ad_gura_cainarului.png");
        if (adTex != null)
            visual.AddChild(new TextureRect
            {
                Texture = adTex,
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                CustomMinimumSize = new Vector2(0, 60),
                MouseFilter = MouseFilterEnum.Ignore,
            });
        var branding = Ui.Row(12);
        branding.AddChild(Ui.Icon("logo", 48));
        branding.AddChild(Ui.Label("FirmoQuest", 32));
        col.AddChild(branding);
        col.AddChild(Ui.Paragraph("Ferma ta sau studioul tău IT. O singură afacere, o mulțime de alegeri.", 18, ThemeColors.TextDim));
        col.AddChild(Ui.Caption("Limba jocului", 14));
        col.AddChild(Ui.LanguageRow());
        if (_onContinue != null)
        {
            var resume = Ui.Button("Continuă partida", _onContinue, ButtonKind.Success, 21);
            resume.CustomMinimumSize = new Vector2(0, 54);
            col.AddChild(resume);
            col.AddChild(Ui.Caption("O partidă nouă înlocuiește salvarea curentă.", 14));
        }
        var play = Ui.Button("Joacă", _onPlay, ButtonKind.Primary, 21, "play", 24);
        play.CustomMinimumSize = new Vector2(0, 58);
        col.AddChild(play);
        col.AddChild(Ui.Button("Cum se joacă?", _onHelp, ButtonKind.Normal, 18));
        col.AddChild(Ui.Button("Ieșire", _onQuit, ButtonKind.Ghost, 17));
        col.AddChild(Ui.Caption(Locale.F("{0} de zile · 2 scenarii · 3 dificultăți", "12-30"), 16));
        if (_updates != null)
            col.AddChild(_updates.CreateNotice());
        else
            col.AddChild(Ui.Caption("Beta 0.5 · Salvare locală automată", 14));
        col.AddChild(Ui.Caption("Modele 3D: Kenney · CC0", 14));
    }

    private void OnLocaleChanged() => Callable.From(Rebuild).CallDeferred();

    private void Rebuild()
    {
        if (!GodotObject.IsInstanceValid(this) || !IsInsideTree())
            return;
        Locale.Changed -= OnLocaleChanged; // _Ready abonează din nou
        foreach (var ch in GetChildren())
        {
            RemoveChild(ch);
            ch.QueueFree();
        }
        _Ready();
    }

    public override void _ExitTree() => Locale.Changed -= OnLocaleChanged;
}
