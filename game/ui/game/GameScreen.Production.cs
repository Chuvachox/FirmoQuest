using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BusinessSim;

// Afiseaza locul selectat, lucrarile disponibile si ghidul contextual.
public partial class GameScreen
{
    public void SelectSlot(int slot)
    {
        if (_activeOverlay != null || HasEvent || _activeSection != "home")
            return;
        _selectedSlot = slot;
        _guideOpen = false;
        BuildSection();
    }
    private void RefreshDetails()
    {
        if (_productionDetails == null)
            return;
        int previousScroll = _detailsScroll?.ScrollVertical ?? 0;
        Clear(_productionDetails);
        if (_detailsScroll != null)
            RestoreScroll(_detailsScroll, previousScroll);
        if (_selectedSlot < 0)
        {
            BuildAdvisor();
            return;
        }
        var heading = Ui.Row();
        heading.AddChild(Ui.Label(Locale.F(_session.Scenario.Id == "agro" ? "Parcelă {0}" : "Post de lucru {0}", _selectedSlot + 1), 22));
        heading.AddChild(Ui.Spacer());
        heading.AddChild(Ui.Button("", () => SelectSlot(-1), ButtonKind.Ghost, 15, "action-close", 22));
        _productionDetails.AddChild(heading);
        if (_selectedSlot >= _session.Slots)
        {
            _productionDetails.AddChild(Ui.Paragraph("Extinde afacerea pentru încă un loc de producție.", 17, ThemeColors.TextDim));
            _productionDetails.AddChild(Ui.Button("Vezi harta dezvoltării", () => Navigate("development"), ButtonKind.Primary, 17));
            return;
        }
        var job = _session.State.Jobs.Find(j => j.Slot == _selectedSlot);
        if (job != null)
        {
            var p = _session.Scenario.FindProduct(job.ProductId);
            _productionDetails.AddChild(Ui.Icon(ProductIcon(p.Id), 64));
            _productionDetails.AddChild(Ui.Title(p.Name, 24));
            _productionDetails.AddChild(Ui.Paragraph(Locale.F("{0} loturi · {1} zile până la livrare", job.Qty, job.DaysLeft), 18));
            _productionDetails.AddChild(new ProgressBar { MaxValue = job.TotalDays, Value = job.TotalDays - job.DaysLeft, ShowPercentage = false });
            _productionDetails.AddChild(Ui.Caption(p.AutoSell ? "Plata se încasează automat la livrare." :
                Locale.F("Vei recolta {0} unități. Apoi le vinzi pe piață.", _session.JobYield(p) * job.Qty), 17));
            _productionDetails.AddChild(Ui.Caption("Timpul avansează numai când închei ziua.", 16));
            return;
        }
        _productionDetails.AddChild(Ui.Caption(_session.Scenario.Id == "agro" ? "Alege ce plantezi pe acest teren." : "Alege contractul pentru acest post.", 17));
        if (_session.BatchLimit > 1)
        {
            var qty = Ui.Row();
            qty.AddChild(Ui.Label("Loturi", 16));
            var spin = new SpinBox
            {
                MinValue = 1,
                MaxValue = _session.BatchLimit,
                Value = _batchCount,
                Step = 1,
                CustomMinimumSize = new Vector2(110, 40),
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            spin.ValueChanged += v => { _batchCount = (int)v; Callable.From(RefreshDetails).CallDeferred(); };
            qty.AddChild(spin);
            _productionDetails.AddChild(qty);
        }
        foreach (var p in _session.Scenario.Products)
        {
            var (card, body) = Ui.Card("", ThemeColors.Green);
            var title = Ui.Row();
            title.AddChild(Ui.Icon(ProductIcon(p.Id), 30));
            var name = Ui.Paragraph(p.Name, 19);
            name.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            title.AddChild(name);
            body.AddChild(title);
            int missing = Math.Max(0, p.Input * _batchCount - _session.State.GetResource(p.Resource));
            int cost = missing * _session.ResourceCost(p.Resource);
            int units = p.AutoSell ? _batchCount : _session.JobYield(p) * _batchCount;
            body.AddChild(Ui.Caption(Locale.F("{0} zile · estimat {1:N0} {2}",
                _session.JobDays(p), units * _session.SellPrice(p.Id), _session.Currency), 15));
            var facts = Ui.Row(6);
            facts.AddChild(Ui.Chip(Locale.F("{0} resurse lipsă", missing), _session.Scenario.Id == "agro" ? "samanta" : "echipament"));
            facts.AddChild(Ui.Chip(Locale.F("{0} zile", _session.JobDays(p)), "action-clock"));
            body.AddChild(facts);
            var button = Ui.Button(Locale.F(p.AutoSell ? "Acceptă · {0:N0} {1}" : "Plantează · {0:N0} {1}", cost, _session.Currency), () =>
            {
                if (_session.PlanProduction(p.Id, _batchCount, _selectedSlot))
                    Message(Locale.F("{0}: producția a început.", Locale.T(p.Name)));
            }, ButtonKind.Success, 16);
            button.Name = "plan-" + p.Id;
            button.Disabled = cost > _session.State.Balance;
            button.TooltipText = Locale.F("Include {0} resurse lipsă. Prețul final de vânzare poate varia.", missing);
            if (button.Disabled)
                body.AddChild(Ui.Caption(Locale.F("Fonduri insuficiente: lipsesc {0:N0} {1}.",
                cost - _session.State.Balance, _session.Currency), 14));
            body.AddChild(button);
            _productionDetails.AddChild(card);
        }
    }
    private void BuildAdvisor()
    {
        var title = Ui.Row(8);
        title.AddChild(Ui.Icon("action-help", 28));
        title.AddChild(Ui.Label("GHIDUL TĂU", 15, Accent));
        title.AddChild(Ui.Spacer());
        var close = Ui.Button("", () => { _guideOpen = false; BuildSection(); }, ButtonKind.Ghost, 15, "action-close", 22);
        close.TooltipText = Locale.T("Inchide ghidul. Il poti redeschide din scena.");
        title.AddChild(close);
        _productionDetails.AddChild(title);
        _productionDetails.AddChild(Ui.Caption("URMĂTORUL PAS", 13));
        bool sell = _session.State.Goods.Values.Any(v => v > 0), empty = _session.State.Jobs.Count < _session.Slots;
        _productionDetails.AddChild(Ui.Title(sell ? "Recolta este gata" : empty ? "Pune afacerea în mișcare" : "Lucrările sunt în curs", 26));
        _productionDetails.AddChild(Ui.Paragraph(sell ? "Ai marfă în depozit. Vinde pe piață pentru a încasa venitul." : empty ?
            "Apasă pe un loc liber din scenă. Alege producția, iar resursele necesare se cumpără împreună cu ea." :
            "Încheie ziua pentru a avansa producția. Costurile zilnice se plătesc la fiecare pas.", 18, ThemeColors.TextDim));
        _productionDetails.AddChild(Ui.Button(sell ? "Deschide piața" : empty ? "Alege un loc liber" : "Avansează producția", () =>
        {
            if (sell)
                Navigate("market");
            else if (empty)
                SelectSlot(Enumerable.Range(0, _session.Slots).First(i => !_session.State.Jobs.Any(j => j.Slot == i)));
            else
                OnEndDay();
        }, ButtonKind.Primary, 17));
        _productionDetails.AddChild(new HSeparator());
        _productionDetails.AddChild(Ui.Stat("Locuri active", $"{_session.State.Jobs.Count} / {_session.Slots}"));
        _productionDetails.AddChild(Ui.Stat("Costuri / zi", Ui.Money(_session.DailyUpkeep(), _session.Currency), ThemeColors.Gold));
        if (_session.State.LoanLeft > 0)
            _productionDetails.AddChild(Ui.Stat("Rată / zi", Ui.Money(Math.Min(_session.State.LoanLeft, _session.State.LoanInstallment), _session.Currency), ThemeColors.Gold));
        if (!_session.State.GuideDismissed)
        {
            _productionDetails.AddChild(Ui.Caption("PRIMII PAȘI", 13));
            foreach (var objective in _session.Objectives.Take(3))
                _productionDetails.AddChild(Ui.Paragraph((objective.done ? "✓  " : "○  ") + Locale.T(objective.title), 17, objective.done ? ThemeColors.Green : ThemeColors.TextDim));
            _productionDetails.AddChild(Ui.Button("Ascunde ghidul", _session.DismissGuide, ButtonKind.Ghost, 14));
        }
        _productionDetails.AddChild(Ui.Caption("Clic dreapta pe obiect: acțiuni. Dezvoltare: îmbunătățiri legate între ele.", 14));
    }
}
