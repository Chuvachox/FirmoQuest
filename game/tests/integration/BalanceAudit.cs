using System;
using System.Linq;
using Godot;

namespace BusinessSim;

// Compara strategii automate pe scenarii, dificultati si secvente reproductibile de evenimente.

public static class BalanceAudit
{
    private static double Capacity(Session s) => s.Scenario.Products.Max(p =>
        ((p.AutoSell ? 1 : s.JobYield(p)) * s.SellPrice(p.Id) - p.Input * s.ResourceCost(p.Resource))
        * (double)s.BatchLimit / s.JobDays(p)) * s.Slots;

    public static void Turn(Session s, bool invest)
    {
        if (s.PendingEvent != null)
        {
            s.ResolvePendingEvent();
            return;
        }
        foreach (var p in s.Scenario.Products)
            if (!p.AutoSell && s.State.GetGood(p.Id) > 0)
                s.Sell(p.Id, s.State.GetGood(p.Id));
        if (invest)
        {
            double current = Capacity(s), best = 0;
            string candidate = null;
            foreach (var u in s.Scenario.Upgrades)
            {
                if (s.State.Upgrades.Contains(u.Id) || !s.UpgradeUnlocked(u.Id))
                    continue;
                int reserve = (s.DailyUpkeep() + Math.Min(s.State.LoanLeft, s.State.LoanInstallment)) * 3
                    + s.Slots * s.ResourceCost(s.Scenario.Products[0].Resource) * s.BatchLimit;
                if (s.State.Balance - u.Price < reserve)
                    continue;
                s.State.Upgrades.Add(u.Id);
                double gain = Capacity(s) - current;
                s.State.Upgrades.Remove(u.Id);
                if (u.Effect.Type == "upkeepDown")
                    gain += s.Scenario.DailyUpkeep * u.Effect.Value;
                double ratio = gain / u.Price;
                if (ratio > best && gain * (s.DayLimit - s.State.Day - 2) > u.Price)
                {
                    best = ratio;
                    candidate = u.Id;
                }
            }
            if (candidate != null)
                s.BuyUpgrade(candidate);
        }
        foreach (var p in s.Scenario.Products.OrderByDescending(p =>
            ((p.AutoSell ? 1 : s.JobYield(p)) * s.SellPrice(p.Id) - p.Input * s.ResourceCost(p.Resource))
            * (double)s.BatchLimit / s.JobDays(p)))
        {
            for (int slot = 0; slot < s.Slots; slot++)
            {
                if (s.State.Jobs.Any(j => j.Slot == slot))
                    continue;
                int reserve = (s.DailyUpkeep() + Math.Min(s.State.LoanLeft, s.State.LoanInstallment)) * (s.JobDays(p) + 1);
                int qty = s.BatchLimit;
                while (qty > 0 && p.Input * qty * s.ResourceCost(p.Resource) + reserve > s.State.Balance)
                    qty--;
                if (qty > 0)
                    s.PlanProduction(p.Id, qty, slot);
            }
        }
        s.EndDay();
    }

    public static int Run(Content c, int seedStart = 0)
    {
        foreach (string sc in new[] { "agro", "it" })
            foreach (string diff in new[] { "easy", "medium", "hard" })
                foreach (bool invest in new[] { false, true })
                {
                    int wins = 0, bankrupt = 0, days = 0, upgrades = 0, profit = 0;
                    for (int seed = seedStart; seed < seedStart + 40; seed++)
                    {
                        var s = Session.NewGame(c, sc, diff, seed);
                        for (int i = 0; i < 100 && !s.State.Finished; i++)
                            Turn(s, invest);
                        if (s.State.Won)
                            wins++;
                        if (s.State.Balance < 0)
                            bankrupt++;
                        days += s.State.Day - 1;
                        upgrades += s.State.Upgrades.Count;
                        profit += s.State.Profit;
                    }
                    GD.Print($"BALANCE {sc}/{diff} invest={invest} wins={wins}/40 bankrupt={bankrupt} days={days / 40.0:F1} upgrades={upgrades / 40.0:F1} profit={profit / 40}");
                }
        return 0;
    }

    public static int CheckDeadlines(Content c)
    {
        int failed = 0;
        foreach (string scenario in new[] { "agro", "it" })
            foreach (var (difficulty, limit) in new[] { ("easy", 30), ("medium", 20), ("hard", 12) })
            {
                var s = Session.NewGame(c, scenario, difficulty, 0);
                s.State.Day = limit;
                s.EndDay();
                bool valid = s.DayLimit == limit && s.State.Finished && s.State.EndTitle == "Timpul a expirat";
                if (!valid)
                    failed++;
                GD.Print($"DEADLINE {scenario}/{difficulty}: {limit} days, expires={valid}");
            }
        foreach (string scenario in new[] { "agro", "it" })
        {
            var s = Session.NewGame(c, scenario, "hard", 0);
            for (int i = 0; i < 30 && !s.State.Finished; i++)
                Turn(s, true);
            if (!s.State.Won || s.State.Day > s.DayLimit + 1)
                failed++;
            GD.Print($"SMOKE {scenario}/hard: won={s.State.Won}, day={s.State.Day - 1}, upgrades={s.State.Upgrades.Count}");
        }
        GD.Print($"Deadline checks: {8 - failed}/8");
        return failed;
    }

    public static int CompareHard(Content c)
    {
        foreach (string scenario in new[] { "agro", "it" })
            foreach (int limit in new[] { 10, 12, 14 })
                foreach (string policy in new[] { "none", "upgrades", "staff" })
                {
                    int wins = 0, bankrupt = 0, winDays = 0;
                    for (int seed = 0; seed < 40; seed++)
                    {
                        var s = Session.NewGame(c, scenario, "hard", seed);
                        s.State.DayLimit = limit;
                        for (int step = 0; step < 100 && !s.State.Finished; step++)
                        {
                            if (policy == "staff" && s.PendingEvent != null)
                            {
                                var ev = s.PendingEvent;
                                int choice = 0;
                                if (ev.HasChoices)
                                {
                                    double Score(EventEffectDef fx)
                                    {
                                        int days = Math.Min(fx.Duration, Math.Max(0, limit - s.State.Day));
                                        return fx.Balance - days * (s.DailyUpkeep() * fx.UpkeepMod +
                                            s.Slots * s.ResourceCost(s.Scenario.Products[0].Resource) / 2.0 * fx.CostMod);
                                    }
                                    for (int i = 1; i < ev.Choices.Count; i++)
                                        if (Score(ev.Choices[i].Effect) > Score(ev.Choices[choice].Effect))
                                            choice = i;
                                }
                                s.ResolvePendingEvent(choice);
                                continue;
                            }
                            if (policy == "staff" && s.State.Day == 1)
                                s.Hire(scenario == "it" ? "junior" : "agronom");
                            Turn(s, policy != "none");
                        }
                        if (s.State.Won)
                        {
                            wins++;
                            winDays += s.State.Day - 1;
                        }
                        if (s.State.Balance < 0)
                            bankrupt++;
                    }
                    GD.Print($"HARD {scenario} limit={limit} policy={policy} wins={wins}/40 bankrupt={bankrupt} winDays={(wins == 0 ? 0 : winDays / (double)wins):F1}");
                }
        return 0;
    }
}
