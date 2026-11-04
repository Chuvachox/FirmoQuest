using System;
using System.IO;
using System.Text.Json;

namespace BusinessSim;

// Verifica salvarea, reluarea si pastrarea regulilor campaniilor existente.

public static class CampaignTest
{
    public static void Run(Content c, Action<bool, string> check)
    {
        foreach (string scenario in new[] { "agro", "it" })
        {
            var s = Session.NewGame(c, scenario, "easy", 42);
            var p = s.Scenario.Products[0];
            int initial = s.State.Balance;
            check(s.PlanProduction(p.Id, 1, 1), scenario + ": productie pe locul ales");
            check(s.State.Jobs.Count == 1 && s.State.Jobs[0].Slot == 1 &&
                s.State.Balance == initial - s.ResourceCost(p.Resource) * p.Input, scenario + ": cost exact al resurselor lipsa");
            int balance = s.State.Balance;
            check(!s.PlanProduction(p.Id, 1, 1) && s.State.Balance == balance && s.State.Jobs.Count == 1,
                scenario + ": locul ocupat nu consuma bani");
            check(!s.PlanProduction(p.Id, int.MaxValue) && s.State.Balance == balance,
                scenario + ": loturile excesive nu consuma bani");
            check(!s.PlanProduction(p.Id, 1, 99) && s.State.Balance == balance, scenario + ": loc invalid refuzat atomic");
            s.State.Balance = 0;
            check(!s.PlanProduction(p.Id, 1, 0) && s.State.Resources[p.Resource] == 0 && s.State.Jobs.Count == 1,
                scenario + ": bani insuficienti fara mutatii");
            s.State.Balance = 20000;
            string first = scenario == "agro" ? "irrigation" : "workstations";
            string next = scenario == "agro" ? "fertilizer" : "automation";
            check(!s.BuyUpgrade(next) && s.State.Balance == 20000, scenario + ": cerinta dezvoltarii validata de sesiune");
            check(s.BuyUpgrade(first) && s.UpgradeUnlocked(next) && s.BuyUpgrade(next), scenario + ": ramura se deblocheaza");
            int after = s.State.Balance;
            check(!s.BuyUpgrade(next) && s.State.Balance == after, scenario + ": un upgrade se plateste o singura data");
            s.DismissGuide();
            s.State.PendingEventId = c.Events.Find(e => e.HasChoices && (e.Scenario == "any" || e.Scenario == scenario)).Id;
            int day = s.State.Day;
            check(s.EndDay()?.Id == s.State.PendingEventId && s.State.Day == day,
                scenario + ": evenimentul blocheaza avansul repetat al zilei");
            check(!s.PlanProduction(p.Id, 1, 0), scenario + ": evenimentul blocheaza productia");
            string dir = Path.Combine(Path.GetTempPath(), "businesssim-campaign-" + Guid.NewGuid().ToString("N"));
            string path = Path.Combine(dir, "campaign.json");
            try
            {
                var legacy = Session.NewGame(c, scenario, "hard", 42);
                legacy.State.DayLimit = 10;
                legacy.State.Day = 8;
                SaveGame.Write(legacy, out _, path);
                var extended = SaveGame.Read(c, out _, path);
                check(extended != null && extended.DayLimit == 12 && extended.State.Day == 8 &&
                    extended.State.Balance == legacy.State.Balance, scenario + ": partida activa primeste doua zile fara pierderea progresului");
                legacy.State.DayLimit = 30;
                SaveGame.Write(legacy, out _, path);
                check(SaveGame.Read(c, out _, path)?.DayLimit == 30, scenario + ": salvarea veche de 30 de zile ramane neschimbata");
                check(SaveGame.Write(s, out _, path), scenario + ": salvare atomica");
                var loaded = SaveGame.Read(c, out var error, path);
                check(loaded != null && error == "" && loaded.State.Balance == s.State.Balance &&
                    loaded.State.Jobs.Count == 1 && loaded.State.Jobs[0].Slot == 1 && loaded.State.Upgrades.Contains(next) &&
                    loaded.State.PendingEventId == s.State.PendingEventId && loaded.State.GuideDismissed,
                    scenario + ": salvarea pastreaza sesiunea si evenimentul");
                if (loaded != null)
                {
                    check(loaded.ResolvePendingEvent() && loaded.State.PendingEventId == "", scenario + ": alegere dupa continuare");
                    int income = loaded.State.TotalIncome, expense = loaded.State.TotalExpenses;
                    check(!loaded.ResolvePendingEvent() && loaded.State.TotalIncome == income && loaded.State.TotalExpenses == expense,
                        scenario + ": evenimentul nu se aplica de doua ori");
                    check(SaveGame.Write(loaded, out _, path) && File.Exists(path + ".bak"), scenario + ": salvarea anterioara are backup");
                }
                File.WriteAllText(path, "{invalid");
                check(SaveGame.Read(c, out error, path) == null && error != "" && File.ReadAllText(path) == "{invalid",
                    scenario + ": salvarea corupta este pastrata si refuzata");
                File.WriteAllText(path, "{\"Version\":999,\"State\":{}}");
                check(SaveGame.Read(c, out _, path) == null, scenario + ": versiunea necunoscuta este refuzata");
            }
            finally
            {
                if (Path.GetFullPath(dir).StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase))
                    Directory.Delete(dir, true);
            }
        }
    }
}
