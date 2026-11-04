using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BusinessSim;

// Explica obiectivul campaniei si regulile de baza.
public partial class HelpScreen : Control
{
    private readonly Content _content;
    private readonly Action _onBack;

    public HelpScreen(Content content, Action onBack)
    {
        _content = content;
        _onBack = onBack;
    }

    public override void _Ready()
    {
        Ui.Fill(this);
        var page = Ui.ScreenPage(this, 36);
        var col = Ui.Column(24);
        page.AddChild(col);
        col.AddChild(Ui.Title("Primul tău ciclu de afaceri", 36));
        col.AddChild(Ui.Paragraph("Un pas pe rând. Jocul așteaptă decizia ta.", 22, ThemeColors.TextDim));
        var row = Ui.Row(20);
        row.SizeFlagsVertical = SizeFlags.ExpandFill;
        col.AddChild(row);
        foreach (var item in new[] {
            ("play", "01  Alege un loc", "Apasă pe o parcelă sau pe un post de lucru. Alege cultura ori contractul. Resursele lipsă se cumpără în același pas."),
            ("ceas", "02  Încheie ziua", "Producția avansează cu o zi. Se plătesc întreținerea, salariile și rata creditului. Evenimentele îți arată efectele înainte de alegere."),
            ("moneda", "03  Încasează și crește", "Ferma păstrează recolta pentru piață. IT încasează automat la livrare. Investește apoi în ramurile de pe harta Dezvoltare.") })
        {
            var (panel, body) = Ui.Card("", ThemeColors.Accent);
            panel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            body.AddChild(Ui.Icon(item.Item1, 72));
            body.AddChild(Ui.Title(item.Item2, 25));
            body.AddChild(Ui.Paragraph(item.Item3, 20, ThemeColors.TextDim));
            row.AddChild(panel);
        }
        col.AddChild(Ui.Paragraph(Locale.F(
            "Câștigi cu {0:N0} {1} profit în cel mult {2} de zile. Un sold negativ înseamnă faliment.",
            _content.Config.TargetProfit, _content.Config.Currency, "12-30"), 22, ThemeColors.Gold));
        col.AddChild(Ui.Caption("1-6: schimbă secțiunea · Esc: închide detaliile sau deschide pauza · Clic dreapta pe obiect: acțiuni", 18));
        col.AddChild(Ui.Caption("Poți închide jocul și continua mai târziu. Salvarea este pe acest calculator.", 18));
        col.AddChild(Ui.Button("← Înapoi", _onBack, ButtonKind.Primary, 19));
    }

}
