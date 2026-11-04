using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BusinessSim;

// Afiseaza piata, dezvoltarea, echipa, finantele si jurnalul folosind regulile sesiunii.
public partial class GameScreen
{
    private void BuildMarket(VBoxContainer body)
    {
        body.AddChild(Ui.Caption(_session.Scenario.Id == "it" ? "Contractele se încasează automat. Aici vezi ofertele și cumperi resurse în avans." :
            "Prețurile se schimbă zilnic. Marfa rămâne în depozit până alegi să vinzi.", 18));
        var row = Ui.Row(16);
        foreach (var p in _session.Scenario.Products)
        {
            var (card, col) = Ui.Card("", ThemeColors.Gold);
            card.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            col.AddChild(Ui.Icon(ProductIcon(p.Id), 62));
            col.AddChild(Ui.Title(p.Name, 23));
            int price = _session.SellPrice(p.Id);
            card.TooltipText = Locale.F("{0}: resurse {1} {2}, durata {3} zile.",
                Locale.T(p.Name), _session.ResourceCost(p.Resource), _session.Currency, _session.JobDays(p));
            col.AddChild(Ui.Label(Ui.Money(price, _session.Currency), 26, ThemeColors.Gold));
            col.AddChild(Ui.Caption(p.AutoSell ? "per contract livrat" : "per unitate", 16));
            int change = (int)Math.Round(100.0 * (price - p.BasePrice) / p.BasePrice);
            col.AddChild(Ui.Paragraph(Locale.F("{0:+0;-0;0}% față de prețul de bază", change), 16, change >= 0 ? ThemeColors.Green : ThemeColors.Gold));
            if (!p.AutoSell)
            {
                int stock = _session.State.GetGood(p.Id);
                col.AddChild(Ui.Stat("În depozit", stock.ToString()));
                var sell = Ui.Button(Locale.F("Vinde tot · {0:N0} {1}", (long)stock * price, _session.Currency), () =>
                {
                    if (_session.Sell(p.Id, _session.State.GetGood(p.Id)))
                        Message("Marfa a fost vândută.");
                }, ButtonKind.Success, 17);
                sell.Name = "sell-" + p.Id;
                sell.Disabled = stock == 0;
                col.AddChild(sell);
            }
            else
                col.AddChild(Ui.Caption(Locale.F("Durată actuală: {0} zile", _session.JobDays(p)), 17));
            row.AddChild(card);
        }
        body.AddChild(row);
        body.AddChild(Ui.Title("Resurse în avans", 22));
        body.AddChild(Ui.Caption("Opțional: producția cumpără automat numai resursele lipsă. Cumpără aici dacă vrei să pregătești stocul.", 16));
        foreach (var resource in _session.Scenario.Resources)
        {
            var r = Ui.Row(12);
            var text = Ui.Label(resource.Name, 18);
            text.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            r.AddChild(Ui.Icon(_session.Scenario.Id == "agro" ? "samanta" : resource.Id == "server" ? "server" : "echipament", 28));
            r.AddChild(text);
            r.AddChild(Ui.Label(Locale.F("Stoc: {0}", _session.State.GetResource(resource.Id)), 17, ThemeColors.TextDim));
            var buy = Ui.Button(Locale.F("Cumpără 1 · {0} {1}", _session.ResourceCost(resource.Id), _session.Currency), () => _session.BuyResource(resource.Id, 1), ButtonKind.Normal, 17);
            buy.Name = "buy-resource-" + resource.Id;
            buy.Disabled = _session.ResourceCost(resource.Id) > _session.State.Balance;
            r.AddChild(buy);
            body.AddChild(r);
        }
    }
    private void BuildDevelopment(VBoxContainer body)
    {
        var summary = Ui.Row(12);
        summary.AddChild(Ui.Chip($"{_session.State.Upgrades.Count} / {_session.Scenario.Upgrades.Count}", NavIcon("development"), Accent, 17));
        summary.AddChild(Ui.Chip($"{_session.Slots} {Locale.T(_session.Scenario.SlotsLabel)}", NavIcon("home"), null, 17));
        if (_session.Scenario.Id == "agro")
            summary.AddChild(Ui.Chip(Locale.F("{0} loturi / parcela", _session.BatchLimit), "samanta", null, 17));
        body.AddChild(summary);
        body.AddChild(Ui.Caption("Compara efectul inainte de cumparare. Viteza se aplica lucrarilor noi; recolta, preturile si costurile se recalculeaza automat.", 17));
        string[][] branches = _session.Scenario.Id == "agro" ? new[] {
            new[] { "irrigation", "fertilizer" }, new[] { "tractor", "land" }, new[] { "bio", "solar" }
        } : new[] { new[] { "workstations", "automation" }, new[] { "licenses", "team" }, new[] { "marketing", "remote" } };
        string[] titles = { "01  Productivitate", "02  Capacitate", "03  Valoare și eficiență" };
        for (int i = 0; i < branches.Length; i++)
        {
            body.AddChild(Ui.Label(titles[i], 17, ThemeColors.TextDim));
            var row = Ui.Row(20);
            foreach (string id in branches[i])
            {
                if (row.GetChildCount() > 0)
                    row.AddChild(Ui.Label("→", 30, ThemeColors.Green));
                var u = _session.Scenario.FindUpgrade(id);
                bool owned = _session.State.Upgrades.Contains(id), unlocked = _session.UpgradeUnlocked(id);
                var (card, col) = Ui.Card("", owned ? ThemeColors.Green : ThemeColors.Accent);
                card.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                var heading = Ui.Row(10);
                heading.AddChild(Ui.Icon(owned ? "action-check" : unlocked ? NavIcon("development") : "action-lock", 32));
                var name = Ui.Paragraph(u.Name, 22, owned ? ThemeColors.Green : ThemeColors.Text);
                name.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                heading.AddChild(name);
                col.AddChild(heading);
                col.AddChild(Ui.Paragraph(u.Desc, 17, ThemeColors.TextDim));
                if (!owned)
                    foreach (var effect in _session.PreviewUpgrade(id))
                        col.AddChild(Ui.Paragraph($"{Locale.T(effect.Label)}: {effect.Before} → {effect.After} {Locale.T(effect.Unit)}", 16, Accent));
                var buy = Ui.Button(owned ? "Deținut" : !unlocked ? "Blocat" :
                    Locale.F("Dezvoltă · {0:N0} {1}", u.Price, _session.Currency),
                    () => { if (_session.BuyUpgrade(id)) Message(Locale.F("Îmbunătățire activată: {0}", Locale.T(u.Name))); }, owned ? ButtonKind.Success : ButtonKind.Primary, 17);
                buy.Name = "upgrade-" + id;
                buy.Disabled = owned || !unlocked || _session.State.Balance < u.Price;
                string prerequisite = _session.UpgradePrerequisite(id);
                string availability = owned ? "Activ permanent" : !unlocked ?
                    Locale.F("Necesită: {0}", Locale.T(_session.Scenario.FindUpgrade(prerequisite).Name)) :
                    _session.State.Balance < u.Price ? Locale.F("Mai ai nevoie de {0:N0} {1}", u.Price - _session.State.Balance, _session.Currency) : "Disponibil acum";
                col.AddChild(Ui.Caption(availability, 14));
                buy.TooltipText = Locale.T(availability);
                col.AddChild(buy);
                row.AddChild(card);
            }
            body.AddChild(row);
        }
    }
    private void BuildTeam(VBoxContainer body)
    {
        body.AddChild(Ui.Caption("Bonusurile au un salariu zilnic. Angajarea cere un salariu în avans.", 18));
        foreach (var staff in _session.Scenario.Staff)
        {
            var (card, col) = Ui.Card(staff.Name, Accent, 22);
            col.AddChild(Ui.Icon(NavIcon("team"), 36));
            col.AddChild(Ui.Paragraph(staff.Desc, 18));
            var row = Ui.Row(15);
            row.AddChild(Ui.Label(Locale.F("{0} {1}/zi · Angajați: {2}",
                staff.Salary, _session.Currency, _session.State.GetStaff(staff.Id)), 18, ThemeColors.TextDim));
            row.AddChild(Ui.Spacer());
            var hire = Ui.Button("Angajează", () => _session.Hire(staff.Id), ButtonKind.Primary, 17, NavIcon("team"), 24);
            hire.TooltipText = Locale.T("Platesti un salariu acum, apoi salariul zilnic. Bonusul se aplica imediat.");
            hire.Disabled = staff.Salary > _session.State.Balance;
            row.AddChild(hire);
            var fire = Ui.Button("Concediază", () => _session.Fire(staff.Id), ButtonKind.Ghost, 17);
            fire.Disabled = _session.State.GetStaff(staff.Id) == 0;
            row.AddChild(fire);
            col.AddChild(row);
            body.AddChild(card);
        }
        body.AddChild(Ui.Stat("Cost zilnic cu personal și întreținere", Ui.Money(_session.DailyUpkeep(), _session.Currency), ThemeColors.Gold));
    }
    private void BuildFinance(VBoxContainer body)
    {
        body.AddChild(Ui.Caption("Soldul este suma disponibilă. Profitul scade datoria rămasă și capitalul inițial, inclusiv grantul.", 18));
        foreach (var item in new[] {
            ("Sold disponibil", _session.State.Balance), ("Venituri totale", _session.State.TotalIncome),
            ("Cheltuieli totale", _session.State.TotalExpenses), ("Datorie rămasă", _session.State.LoanLeft),
            ("Capital inițial", _session.State.InitialEquity), ("Profit realizat", _session.State.Profit) })
        {
            var stat = Ui.Panel(14, 10, ThemeColors.PanelLight);
            stat.AddChild(Ui.Stat(item.Item1, Ui.Money(item.Item2, _session.Currency),
                item.Item1 == "Profit realizat" ? (item.Item2 >= 0 ? ThemeColors.Green : ThemeColors.Red) : ThemeColors.Text, 18));
            body.AddChild(stat);
        }
        int burn = _session.DailyUpkeep() + Math.Min(_session.State.LoanInstallment, _session.State.LoanLeft);
        body.AddChild(Ui.Stat("Plata estimată la sfârșitul zilei", Ui.Money(burn, _session.Currency), ThemeColors.Gold));
        body.AddChild(Ui.Caption(burn > 0 ? Locale.F("Rezervă fără venituri: circa {0} zile la costul actual.",
            Math.Max(0, _session.State.Balance / burn)) : "Fără costuri zilnice.", 18));
        if (_session.State.ProfitHistory.Count > 1)
            body.AddChild(new PriceSparkline(_session.State.ProfitHistory) { CustomMinimumSize = new Vector2(0, 120) });
        body.AddChild(Ui.Title("Efecte active", 22));
        if (_session.State.Mods.Count == 0)
            body.AddChild(Ui.Caption("Niciun efect temporar.", 17));
        foreach (var mod in _session.State.Mods)
            body.AddChild(Ui.Paragraph(Locale.F("{0} {1:+0%;-0%} · {2} zile",
            ModName(mod.Kind), mod.Value, mod.DaysLeft), 17, ThemeColors.Gold));
    }
    private void BuildJournal(VBoxContainer body)
    {
        int complete = _session.Objectives.Count(o => o.done), total = _session.Objectives.Count();
        body.AddChild(Ui.Label(Locale.F("Obiective · {0} / {1}", complete, total), 21, Accent));
        body.AddChild(new ProgressBar
        {
            MaxValue = total,
            Value = complete,
            ShowPercentage = false,
            CustomMinimumSize = new Vector2(0, 10),
            TooltipText = Locale.T("Obiective atinse in partida curenta.")
        });
        foreach (var obj in _session.Objectives)
        {
            var card = Ui.Panel(12, 10, ThemeColors.PanelLight);
            var row = Ui.Row(12);
            card.AddChild(row);
            row.AddChild(Ui.Icon(obj.done ? "action-check" : NavIcon("journal"), 28));
            var text = Ui.Column(3);
            text.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            text.AddChild(Ui.Paragraph(obj.title, 18, obj.done ? ThemeColors.Green : ThemeColors.Text));
            text.AddChild(Ui.Caption(obj.detail, 15));
            row.AddChild(text);
            body.AddChild(card);
        }
        body.AddChild(Ui.Title("Ultimele acțiuni", 22));
        foreach (string entry in _session.State.Log.AsEnumerable().Reverse().Take(45))
        {
            var row = Ui.Row(10);
            row.AddChild(Ui.Icon("action-clock", 20));
            var line = Ui.Paragraph(Locale.Log(entry), 17, ThemeColors.TextDim);
            line.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            row.AddChild(line);
            body.AddChild(row);
        }
    }
}
