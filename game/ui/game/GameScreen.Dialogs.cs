using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BusinessSim;

// Gestioneaza pauza, alegerea limbii, confirmarea zilei si evenimentele.
public partial class GameScreen
{
    private void OnEndDay()
    {
        if (HasEvent || _session.State.Finished || _activeOverlay != null)
            return;
        if (_session.State.Jobs.Count == 0 || _session.State.Balance < _session.DailyUpkeep() + Math.Min(_session.State.LoanLeft, _session.State.LoanInstallment))
        {
            var col = Modal("Închei ziua?");
            col.AddChild(Ui.Paragraph(_session.State.Jobs.Count == 0 ? "Nu ai producție activă. Vei plăti costurile zilnice fără ca o lucrare să avanseze." :
                "Soldul actual nu acoperă plățile estimate. Livrările IT de astăzi pot aduce venit; un sold final negativ înseamnă faliment.", 19, ThemeColors.Gold));
            col.AddChild(Ui.Button("Continuă ziua următoare", () => { CloseOverlay(); AdvanceDay(); }, ButtonKind.Warning, 18));
            col.AddChild(Ui.Button("Înapoi la afacere", CloseOverlay, ButtonKind.Ghost, 17));
        }
        else
            AdvanceDay();
    }
    private void AdvanceDay()
    {
        int income = _session.State.TotalIncome, expenses = _session.State.TotalExpenses, debt = _session.State.LoanLeft;
        var ev = _session.EndDay();
        Message(Locale.F("Zi încheiată · venit +{0:N0} · cheltuieli -{1:N0} · credit -{2:N0} {3}",
            _session.State.TotalIncome - income, _session.State.TotalExpenses - expenses,
            debt - _session.State.LoanLeft, _session.Currency));
        if (ev != null)
            ShowEvent();
    }
    private VBoxContainer Modal(string title, string icon = null)
    {
        _businessWorld.CloseContextMenu();
        CloseOverlay();
        var overlay = new ColorRect { Color = new Color(.025f, .04f, .05f, .82f), MouseFilter = MouseFilterEnum.Stop };
        Ui.Fill(overlay);
        AddChild(overlay);
        _activeOverlay = overlay;
        var panel = Ui.FloatingPanel(Accent, 26, 22);
        panel.Name = "modal-panel";
        panel.AnchorLeft = .27f;
        panel.AnchorRight = .73f;
        panel.AnchorTop = .18f;
        panel.AnchorBottom = .76f;
        overlay.AddChild(panel);
        var frame = Ui.Column(14);
        panel.AddChild(frame);
        var heading = Ui.Row(12);
        heading.AddChild(Ui.Icon(icon ?? (HasEvent ? "action-warning" : "action-help"), 36));
        var titleLabel = Ui.Paragraph(title, 27);
        titleLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        heading.AddChild(titleLabel);
        frame.AddChild(heading);
        frame.AddChild(new HSeparator());
        var scroll = Ui.Scroll();
        scroll.SizeFlagsVertical = SizeFlags.ExpandFill;
        frame.AddChild(scroll);
        var col = Ui.Column(14);
        col.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        scroll.AddChild(col);
        return col;
    }
    private void CloseOverlay()
    {
        if (_activeOverlay == null)
            return;
        var old = _activeOverlay;
        _activeOverlay = null;
        RemoveChild(old);
        old.QueueFree();
    }
    private void ShowEvent()
    {
        var ev = _session.PendingEvent;
        if (ev == null)
            return;
        var col = Modal(ev.Title);
        col.AddChild(Ui.Paragraph(ev.Text, 19, ThemeColors.TextDim));
        if (ev.HasChoices)
            for (int i = 0; i < ev.Choices.Count; i++)
            {
                int choice = i;
                col.AddChild(Ui.Paragraph(EffectText(ev.Choices[i].Effect), 16, ThemeColors.Gold));
                var b = Ui.Button(ev.Choices[i].Label, () => ResolveEvent(choice), ButtonKind.Primary, 18);
                b.Name = "event-choice-" + i;
                col.AddChild(b);
            }
        else
        {
            col.AddChild(Ui.Paragraph(EffectText(ev.Effect), 17, ThemeColors.Gold));
            var b = Ui.Button("Am înțeles", () => ResolveEvent(), ButtonKind.Primary, 18);
            b.Name = "event-continue";
            col.AddChild(b);
        }
    }
    public void ResolveEvent(int choice = 0)
    {
        if (!HasEvent)
            return;
        CloseOverlay();
        _session.ResolvePendingEvent(choice);
    }
    public void AutoTurn()
    {
        if (HasEvent)
        {
            ResolveEvent();
            return;
        }
        CloseOverlay();
        _session.DismissGuide();
        if (AutoPlayer.Turn(_session) != null)
            ShowEvent();
    }
    private void PauseMenu()
    {
        if (HasEvent)
            return;
        var col = Modal("Partidă în pauză");
        _activeOverlay.Name = "pause-overlay";
        col.AddChild(Ui.Caption("Timpul avansează numai la încheierea zilei. Progresul se salvează local automat.", 18));
        col.AddChild(Ui.Button("Limba jocului", LanguageMenu, ButtonKind.Normal, 18, "action-language", 24));
        col.AddChild(Ui.Button("Continuă jocul", CloseOverlay, ButtonKind.Success, 18));
        col.AddChild(Ui.Button("Salvează și mergi la meniu", () => { if (SaveProgress()) _returnToMenu(); }, ButtonKind.Normal, 18));
    }
    private void LanguageMenu()
    {
        if (HasEvent)
            return;
        var col = Modal("Limba jocului", "action-language");
        _activeOverlay.Name = "language-overlay";
        var panel = _activeOverlay.GetNode<PanelContainer>("modal-panel");
        panel.AnchorLeft = panel.AnchorRight = 1;
        panel.AnchorTop = panel.AnchorBottom = 0;
        panel.OffsetLeft = -342;
        panel.OffsetRight = -22;
        panel.OffsetTop = 94;
        panel.OffsetBottom = 490;
        ((ColorRect)_activeOverlay).Color = new Color(.025f, .04f, .05f, .35f);
        _activeOverlay.GuiInput += input =>
        {
            if (input is InputEventMouseButton mouse && mouse.Pressed && mouse.ButtonIndex == MouseButton.Left &&
                !panel.GetGlobalRect().HasPoint(mouse.GlobalPosition))
                CloseOverlay();
        };
        foreach (string code in Locale.Codes)
        {
            var button = Ui.Button(Locale.Name(code), () => { CloseOverlay(); Locale.Set(code); },
                code == Locale.Lang ? ButtonKind.Primary : ButtonKind.Normal, 21);
            button.Name = "choose-language-" + code;
            button.CustomMinimumSize = new Vector2(0, 56);
            col.AddChild(button);
        }
        var close = Ui.Button("Închide", CloseOverlay, ButtonKind.Ghost, 18, "action-close", 22);
        close.Name = "close-language";
        col.AddChild(close);
    }
    private static string ModName(string kind) => kind switch
    {
        "price" => Locale.T("Preț de vânzare"),
        "cost" => Locale.T("Cost resurse"),
        _ => Locale.T("Întreținere")
    };
    private static string EffectText(EventEffectDef fx)
    {
        var parts = new List<string>();
        if (fx.Balance != 0)
            parts.Add(Locale.F("Bani {0:+0;-0}", fx.Balance));
        if (fx.PriceMod != 0)
            parts.Add(Locale.F("Preț {0:+0%;-0%}", fx.PriceMod));
        if (fx.CostMod != 0)
            parts.Add(Locale.F("Resurse {0:+0%;-0%}", fx.CostMod));
        if (fx.UpkeepMod != 0)
            parts.Add(Locale.F("Cost zilnic {0:+0%;-0%}", fx.UpkeepMod));
        if (fx.PriceMod != 0 || fx.CostMod != 0 || fx.UpkeepMod != 0)
            parts.Add(Locale.F("{0} zile", fx.Duration));
        return parts.Count == 0 ? Locale.T("Fără cost suplimentar") : string.Join(" · ", parts);
    }
    private static string ProductIcon(string id) => id switch
    {
        "wheat" => "grau",
        "corn" => "porumb",
        "veg" => "legume",
        "site" => "site",
        "app" => "aplicatie",
        _ => "mentenanta"
    };
}
