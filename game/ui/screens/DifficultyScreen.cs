using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BusinessSim;

// Prezinta finantarea si termenul fiecarei dificultati.
public partial class DifficultyScreen : Control
{
    private readonly Content _content;
    private readonly Action<string> _onPick;
    private readonly Action _onBack;

    public DifficultyScreen(Content content, Action<string> onPick, Action onBack)
    {
        _content = content;
        _onPick = onPick;
        _onBack = onBack;
    }

    public override void _Ready()
    {
        Ui.Fill(this);
        var page = Ui.ScreenPage(this, 30);

        var col = Ui.Column(14);
        page.AddChild(col);

        col.AddChild(Ui.Title("Alege dificultatea", 30));

        var row = Ui.Row(16);
        row.SizeFlagsVertical = SizeFlags.ExpandFill;
        col.AddChild(row);

        foreach (var d in _content.Config.Difficulties)
            row.AddChild(BuildCard(d));

        col.AddChild(Ui.VerticalSpacer(4));
        var back = Ui.Button("← Înapoi", _onBack, ButtonKind.Ghost, 15);
        back.CustomMinimumSize = new Vector2(200, 44);
        col.AddChild(back);
    }

    private Control BuildCard(DifficultyDef d)
    {
        Color accent = d.Id switch
        {
            "easy" => ThemeColors.Green,
            "hard" => ThemeColors.Red,
            _ => ThemeColors.Gold,
        };

        string cur = _content.Config.Currency;
        var (panel, body) = Ui.Card(d.Label, accent, 20);
        panel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        panel.SizeFlagsVertical = SizeFlags.ExpandFill;

        body.AddChild(Ui.Paragraph(d.Short, 14, ThemeColors.TextDim));
        body.AddChild(Ui.Paragraph(d.Description, 15));
        body.AddChild(Ui.Paragraph(Locale.F("Durată actuală: {0} zile", _content.Config.DaysFor(d.Id)), 17, accent));
        body.AddChild(Ui.VerticalSpacer(4));
        body.AddChild(Ui.Stat("Bani proprii", Ui.Money(d.StartBalance, cur)));
        body.AddChild(Ui.Stat("Grant", d.Grant > 0 ? "+" + Ui.Money(d.Grant, cur) : "-",
            d.Grant > 0 ? ThemeColors.Green : ThemeColors.TextDark));
        body.AddChild(Ui.Stat("Credit", d.Loan > 0 ? Ui.Money(d.Loan, cur) : "-",
            d.Loan > 0 ? ThemeColors.Red : ThemeColors.TextDark));
        body.AddChild(Ui.Stat("Rambursare zilnică",
            d.Loan > 0 ? Ui.Money(d.LoanInstallment, cur) : "-",
            d.Loan > 0 ? ThemeColors.Red : ThemeColors.TextDark));
        body.AddChild(Ui.Stat("Lichidități la start", Ui.Money(d.TotalCash, cur), ThemeColors.Accent));
        body.AddChild(Ui.Stat("Profit țintă", Ui.Money(_content.Config.TargetProfit, cur), ThemeColors.Gold));

        body.AddChild(new Control { SizeFlagsVertical = SizeFlags.ExpandFill });
        var btn = Ui.Button(Locale.F("Pornește ({0})", Locale.T(d.Label)), () => _onPick(d.Id), ButtonKind.Primary, 16);
        btn.CustomMinimumSize = new Vector2(0, 48);
        btn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        body.AddChild(btn);

        return panel;
    }
}
