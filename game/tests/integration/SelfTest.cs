using System;
using Godot;

namespace BusinessSim;

// Executa verificarile comune ale economiei si mecanicilor jocului.

public static class SelfTest
{
    private static int _failed;
    private static int _passed;

    private static void Check(bool ok, string name)
    {
        if (ok)
        {
            _passed++;
            GD.Print($"  PASS  {name}");
        }
        else
        {
            _failed++;
            GD.Print($"  FAIL  {name}");
        }
    }

    public static int Run(Content c)
    {
        GD.Print("=== BUSINESS SIMULATOR - TEST AUTOMAT ===");
        _failed = 0;
        _passed = 0;

        TestContent(c);
        TestIntegration(c);
        TestEvents(c);
        CampaignTest.Run(c, Check);
        DevelopmentTest.Run(c, Check);
        DisplayTest.Run(Check);
        LocalizationTest.Run(c, Check);
        UpdateTest.Run(Check);
        foreach (var diff in new[] { "easy", "medium", "hard" })
            foreach (var sc in new[] { "agro", "it" })
                TestPlaythrough(c, sc, diff);

        GD.Print($"--- Rezultat: {_passed} treceri, {_failed} eșecuri ---");
        return _failed;
    }

    private static void CheckEffect(Session s, EventDef ev, string label) =>
        CheckEffect(s, ev.HasChoices ? ev.Choices[0].Effect : ev.Effect, label);

    private static void CheckEffect(Session s, EventEffectDef fx, string label)
    {
        int log0 = s.State.Log.Count;
        int mods0 = s.State.Mods.Count;
        int inc0 = s.State.TotalIncome;
        int exp0 = s.State.TotalExpenses;

        s.ApplyEffect(fx);

        bool changed = s.State.Log.Count > log0
                    || s.State.Mods.Count > mods0
                    || s.State.TotalIncome != inc0
                    || s.State.TotalExpenses != exp0;
        Check(changed || (fx != null && fx.IsEmpty), $"efect aplicat: {label}");
    }

    private static void TestEvents(Content c)
    {
        GD.Print("[Evenimente - aplicare directă]");
        foreach (var ev in c.Events)
        {
            string scId = ev.Scenario == "any" ? "agro" : ev.Scenario;
            var s = Session.NewGame(c, scId, "medium");

            if (ev.HasChoices)
            {
                for (int i = 0; i < ev.Choices.Count; i++)
                    CheckEffect(s, ev.Choices[i].Effect, $"„{ev.Title}” - alegerea {i + 1}");
            }
            else
            {
                CheckEffect(s, ev.Effect, $"„{ev.Title}”");
            }
        }
    }

    private static void TestContent(Content c)
    {
        GD.Print("[Conținut]");
        Check(c.Config != null, "config încărcat");
        Check(c.Config.DaysTotal == 30, $"30 de zile (am: {c.Config.DaysTotal})");
        Check(c.Config.TargetProfit > 0, $"țintă de profit definită ({c.Config.TargetProfit})");
        Check(c.Config.Difficulties.Count == 3, $"3 dificultăți (am: {c.Config.Difficulties.Count})");
        Check(c.Scenarios.Count == 2, $"2 scenarii (am: {c.Scenarios.Count})");
        Check(c.Events.Count >= 10, $"minim 10 evenimente (am: {c.Events.Count})");

        foreach (var kv in c.Scenarios)
        {
            var s = kv.Value;
            GD.Print($"[Scenariu: {s.Name}]");
            Check(s.Resources.Count >= 2, $"minim 2 resurse (am: {s.Resources.Count})");
            Check(s.Products.Count >= 3, $"minim 3 produse (am: {s.Products.Count})");
            Check(s.Upgrades.Count >= 4, $"minim 4 îmbunătățiri (am: {s.Upgrades.Count})");
            Check(s.Staff.Count >= 2, $"minim 2 posturi (am: {s.Staff.Count})");
            Check(s.Slots >= 1, $"minim 1 loc de producție (am: {s.Slots})");

            foreach (var p in s.Products)
            {
                var res = s.FindResource(p.Resource);
                Check(res != null, $"{p.Name}: resursa „{p.Resource}” există");
                Check(p.BasePrice > 0, $"{p.Name}: preț de vânzare > 0");
                Check(p.Days >= 1, $"{p.Name}: durată ≥ 1 zi");
            }
        }
    }

    private static void TestPlaythrough(Content c, string scenarioId, string difficultyId)
    {
        var sc = c.GetScenario(scenarioId);
        var d = c.Config.FindDifficulty(difficultyId);
        GD.Print($"[Joc: {sc.Short} / {d.Label}]");

        var s = Session.NewGame(c, scenarioId, difficultyId);

        Check(s.State.Balance == d.StartBalance + d.Grant + d.Loan,
            $"lichidități de start = {d.StartBalance + d.Grant + d.Loan} (am: {s.State.Balance})");
        Check(s.State.LoanLeft == d.Loan, $"creditul de start = {d.Loan} (am: {s.State.LoanLeft})");
        Check(s.State.Profit == 0, $"profitul de start = 0 (am: {s.State.Profit})");

        var r0 = sc.Resources[0];
        int cost1 = s.ResourceCost(r0.Id);
        int balanceBefore = s.State.Balance;
        bool bought = s.BuyResource(r0.Id, 2);
        Check(bought, "cumpărare resurse");
        Check(s.State.Balance == balanceBefore - cost1 * 2, "scăderea soldului = preț × cantitate");
        Check(s.State.GetResource(r0.Id) == 2, "stocul de resurse a crescut");

        Check(cost1 >= 1, "prețul resursei ≥ 1");

        var p0 = sc.Products[0];
        int resBefore = s.State.GetResource(p0.Resource);
        bool started = s.StartJob(p0.Id, 1);
        Check(started, "pornirea unei producții");
        Check(s.State.GetResource(p0.Resource) == resBefore - p0.Input, "resursele consumate la pornire");
        Check(s.State.Jobs.Count == 1, "producția apare în lista „în lucru”");

        int daysNeeded = s.JobDays(p0);
        Check(daysNeeded >= 1 && daysNeeded <= p0.Days, $"durata calculată = {daysNeeded} (bază {p0.Days})");

        int dayBefore = s.State.Day;
        int expensesBefore = s.State.TotalExpenses;
        var ev = s.EndDay();
        Check(s.State.Day == dayBefore + 1, "ziua a avansat");
        Check(s.State.TotalExpenses > expensesBefore, "costurile fixe au fost înregistrate");
        if (ev != null)
        {
            GD.Print($"         eveniment: {ev.Title}");
            int balanceBeforeEvent = s.State.Balance;
            var effect = ev.HasChoices ? ev.Choices[0].Effect : ev.Effect;
            CheckEffect(s, ev, $"„{ev.Title}” (ziua {s.State.Day})");
            Check(s.State.Balance == balanceBeforeEvent + effect.Balance,
                "efectul financiar al evenimentului este aplicat o data");
        }

        if (d.Loan > 0 && !s.State.Finished)
        {
            int loanBefore = s.State.LoanLeft;
            int expected = loanBefore - Math.Min(loanBefore, d.LoanInstallment);
            var ev2 = s.EndDay();
            Check(s.State.LoanLeft == expected,
                $"creditul se rambursează zilnic ({loanBefore} → {s.State.LoanLeft}, așteptat {expected})");
            if (ev2 != null)
                CheckEffect(s, ev2, $"„{ev2.Title}” (ziua {s.State.Day})");
        }

        int guard = 0;
        while (!s.State.Finished && guard++ < 80)
        {
            var evDay = AutoPlayer.Turn(s);
            if (evDay != null)
            {
                GD.Print($"         eveniment (partidă): {evDay.Title}");
                CheckEffect(s, evDay, $"„{evDay.Title}” (ziua {s.State.Day})");
            }
        }

        Check(s.State.Finished, $"jocul se încheie (ziua {s.State.Day}, finisat: {s.State.Finished})");
        Check(s.State.Day <= s.DayLimit + 1, $"nu depășește numărul de zile (ziua {s.State.Day})");
        Check(s.State.Balance >= 0 || !s.State.Won, $"sold final = {s.State.Balance}");
        GD.Print($"         profit final: {s.State.Profit} {s.Currency} | venituri {s.State.TotalIncome} | cheltuieli {s.State.TotalExpenses} | {s.State.EndTitle}");
    }

    private static void TestIntegration(Content c)
    {
        foreach (var scenarioId in new[] { "agro", "it" })
        {
            var s = Session.NewGame(c, scenarioId, "medium");
            string resourceId = s.Scenario.Resources[0].Id;
            int cash = s.State.Balance;
            int expenses = s.State.TotalExpenses;
            int changes = 0;
            s.Changed += () => changes++;
            Check(!s.BuyResource(resourceId, int.MaxValue), $"{scenarioId}: cantitate prea mare refuzata");
            Check(s.State.Balance == cash && s.State.TotalExpenses == expenses && s.State.GetResource(resourceId) == 0,
                $"{scenarioId}: refuzul nu schimba banii sau stocul");
            changes = 0;
            Check(s.BuyResource(resourceId, 1), $"{scenarioId}: cumparare valida");
            Check(changes == 1 && s.State.GetResource(resourceId) == 1,
                $"{scenarioId}: o actiune, o notificare, o unitate");
            Check(!s.StartJob(s.Scenario.Products[0].Id, int.MaxValue) && s.State.Jobs.Count == 0,
                $"{scenarioId}: productie imposibila refuzata fara lucrare");

            s.ApplyEffect(new EventEffectDef { Balance = -s.State.Balance - 1 });
            Check(s.State.Finished && !s.State.Won, $"{scenarioId}: faliment imediat dupa eveniment");
            int finalCash = s.State.Balance;
            s.ApplyEffect(new EventEffectDef { Balance = 1000 });
            Check(s.State.Balance == finalCash, $"{scenarioId}: partida incheiata nu mai aplica efecte");

            var fresh = Session.NewGame(c, scenarioId, "medium");
            Check(!fresh.State.Finished && fresh.State.Day == 1 && fresh.State.Resources.Count == 0 && fresh.State.Jobs.Count == 0,
                $"{scenarioId}: restart fara stare veche");
            fresh.ApplyEffect(new EventEffectDef { Balance = c.Config.TargetProfit });
            Check(fresh.State.Finished && fresh.State.Won, $"{scenarioId}: tinta atinsa prin eveniment");

            var modifiers = Session.NewGame(c, scenarioId, "medium");
            modifiers.ApplyEffect(new EventEffectDef { CostMod = 0.25, Duration = 1 });
            Check(modifiers.State.Mods.Count == 1, $"{scenarioId}: modificator temporar adaugat");
            modifiers.EndDay();
            Check(modifiers.State.Mods.Count == 0, $"{scenarioId}: modificator expirat dupa o zi");
        }
    }
}
