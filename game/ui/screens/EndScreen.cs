using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BusinessSim;

// Prezinta rezultatul partidei, istoricul financiar si optiunile de reluare.
public partial class EndScreen : Control
{
    private readonly Session _session;
    private readonly Action _onRetry;
    private readonly Action _onMenu;

    public EndScreen(Session session, Action onRetry, Action onMenu)
    {
        _session = session;
        _onRetry = onRetry;
        _onMenu = onMenu;
    }

    public override void _Ready()
    {
        Ui.Fill(this);
        var page = Ui.ScreenPage(this, 40);

        var center = new CenterContainer();
        Ui.Fill(center);
        page.AddChild(center);

        var s = _session.State;
        string cur = _session.Currency;

        var col = Ui.Column(10);
        col.CustomMinimumSize = new Vector2(640, 0);
        center.AddChild(col);

        var head = Ui.Paragraph(s.EndTitle, 44, s.Won ? ThemeColors.Green : ThemeColors.Red);
        head.HorizontalAlignment = HorizontalAlignment.Center;
        col.AddChild(head);

        string endText = s.EndTitle switch
        {
            "Ținta atinsă!" => Locale.F("Ai obținut {0} {1} profit în {2} zile. Felicitări!",
                s.Profit, _session.Currency, s.Day - 1),
            "Timpul a expirat" => Locale.F("Ai încheiat cu {0} {1} profit, iar ținta era de {2} {1}.",
                s.Profit, _session.Currency, _session.Config.TargetProfit),
            _ => Locale.T(s.EndText)
        };
        var text = Ui.Paragraph(endText, 17, ThemeColors.TextDim);
        text.HorizontalAlignment = HorizontalAlignment.Center;
        col.AddChild(text);

        col.AddChild(Ui.VerticalSpacer(8));

        if (s.ProfitHistory.Count > 0)
        {
            var (chartPanel, chartBody) = Ui.Card("Profitul față de țintă, zi cu zi", ThemeColors.Gold, 14);
            chartBody.AddChild(new ProfitChart(s.ProfitHistory, _session.Config.TargetProfit));

            var legend = Ui.Row(14);
            legend.AddChild(Ui.Label("■ zi cu profit", 12, ThemeColors.Green));
            legend.AddChild(Ui.Label("■ zi cu pierdere", 12, ThemeColors.Red));
            legend.AddChild(Ui.Label(Locale.F("-- ținta {0}", Ui.Money(_session.Config.TargetProfit, cur)), 12, ThemeColors.Gold));
            chartBody.AddChild(legend);
            col.AddChild(chartPanel);

            col.AddChild(Ui.VerticalSpacer(8));
        }

        var (panel, body) = Ui.Card("Rezultatul financiar",
            s.Won ? ThemeColors.Green : ThemeColors.Gold, 16);
        body.AddChild(Ui.Stat("Profit realizat", Ui.Signed(s.Profit, cur),
            s.Profit >= 0 ? ThemeColors.Green : ThemeColors.Red));
        body.AddChild(Ui.Stat("Țintă", Ui.Money(_session.Config.TargetProfit, cur), ThemeColors.Gold));
        body.AddChild(Ui.Stat("Venituri totale", Ui.Money(s.TotalIncome, cur), ThemeColors.Green));
        body.AddChild(Ui.Stat("Cheltuieli totale", Ui.Money(s.TotalExpenses, cur), ThemeColors.Red));
        body.AddChild(Ui.Stat("Sold final", Ui.Money(s.Balance, cur), ThemeColors.Accent));
        if (s.LoanLeft > 0)
            body.AddChild(Ui.Stat("Credit rămas", Ui.Money(s.LoanLeft, cur), ThemeColors.Red));
        body.AddChild(Ui.Stat("Zile folosite", $"{System.Math.Min(s.Day - 1, _session.DayLimit)} / {_session.DayLimit}"));
        body.AddChild(Ui.Stat("Dificultate", _session.Difficulty.Label));
        body.AddChild(Ui.Stat("Scenariu", _session.Scenario.Name));
        col.AddChild(panel);

        col.AddChild(Ui.VerticalSpacer(8));

        var row = Ui.Row(10);
        var again = Ui.Button("Joacă din nou", _onRetry, ButtonKind.Primary, 16);
        again.CustomMinimumSize = new Vector2(0, 48);
        again.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        row.AddChild(again);

        var menu = Ui.Button("Meniu principal", _onMenu, ButtonKind.Normal, 16);
        menu.CustomMinimumSize = new Vector2(0, 48);
        menu.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        row.AddChild(menu);
        col.AddChild(row);
    }
}
