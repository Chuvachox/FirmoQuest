using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BusinessSim;

// Prezinta ferma si compania IT inainte de pornirea unei partide.
public partial class ScenarioScreen : Control
{
    private readonly Content _content;
    private readonly string _difficultyId;
    private readonly Action<ScenarioDef> _onPick;
    private readonly Action _onBack;

    public ScenarioScreen(Content content, string difficultyId,
        Action<ScenarioDef> onPick, Action onBack)
    {
        _content = content;
        _difficultyId = difficultyId;
        _onPick = onPick;
        _onBack = onBack;
    }

    public override void _Ready()
    {
        Ui.Fill(this);
        var page = Ui.ScreenPage(this, 30);

        var col = Ui.Column(14);
        page.AddChild(col);

        col.AddChild(Ui.Title("Alege scenariul", 30));
        var diff = _content.Config.FindDifficulty(_difficultyId);
        if (diff != null)
            col.AddChild(Ui.Caption(Locale.F("Dificultate selectată: {0} - {1}", Locale.T(diff.Label), Locale.T(diff.Description)), 14));

        var row = Ui.Row(16);
        row.SizeFlagsVertical = SizeFlags.ExpandFill;
        col.AddChild(row);

        foreach (var sc in _content.Scenarios.Values)
            row.AddChild(BuildCard(sc));

        col.AddChild(Ui.VerticalSpacer(4));
        var back = Ui.Button("← Înapoi", _onBack, ButtonKind.Ghost, 15);
        back.CustomMinimumSize = new Vector2(200, 44);
        col.AddChild(back);
    }

    private Control BuildCard(ScenarioDef sc)
    {
        var accent = sc.AccentColor;
        var (panel, body) = Ui.Card(sc.Name, accent, 18);
        panel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        panel.SizeFlagsVertical = SizeFlags.ExpandFill;

        var preview = new BusinessWorld(Session.NewGame(_content, sc.Id, _difficultyId));
        preview.CustomMinimumSize = new Vector2(0, OS.HasFeature("android") ? 160 : 200);
        preview.SizeFlagsVertical = SizeFlags.ExpandFill;
        body.AddChild(preview);
        body.AddChild(Ui.Paragraph(sc.Description, 19, ThemeColors.TextDim));
        body.AddChild(Ui.VerticalSpacer(4));
        body.AddChild(Ui.Stat("Costuri fixe pe zi", Ui.Money((int)sc.DailyUpkeep, _content.Config.Currency), ThemeColors.Red));
        body.AddChild(Ui.Stat(sc.SlotsLabel, sc.Slots.ToString(), ThemeColors.Accent));

        body.AddChild(Ui.VerticalSpacer(6));
        var chips = Ui.Row(6);
        foreach (var p in sc.Products)
        {
            var chip = Ui.Label(p.Name, 12, ThemeColors.TextDim);
            chips.AddChild(chip);
        }
        body.AddChild(chips);

        body.AddChild(new Control { SizeFlagsVertical = SizeFlags.ExpandFill });
        var btn = Ui.Button(Locale.F("Joacă: {0}", Locale.T(sc.Short)), () => _onPick(sc), ButtonKind.Primary, 16);
        btn.CustomMinimumSize = new Vector2(0, 48);
        btn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        body.AddChild(btn);

        return panel;
    }
}
