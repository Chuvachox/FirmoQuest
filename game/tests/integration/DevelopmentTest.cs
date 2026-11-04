using System;
using System.Linq;

namespace BusinessSim;

// Verifica efectele, dependentele si previzualizarea imbunatatirilor.

public static class DevelopmentTest
{
    public static void Run(Content c, Action<bool, string> check)
    {
        foreach (string scenario in new[] { "agro", "it" })
        {
            var s = Session.NewGame(c, scenario, "easy", 50);
            s.State.Balance = 100000;
            foreach (string id in scenario == "agro"
                ? new[] { "irrigation", "fertilizer", "tractor", "land", "bio", "solar" }
                : new[] { "workstations", "automation", "licenses", "team", "marketing", "remote" })
            {
                int balance = s.State.Balance, count = s.State.Upgrades.Count, notifications = 0;
                void Changed() => notifications++;
                s.Changed += Changed;
                var changes = s.PreviewUpgrade(id);
                s.Changed -= Changed;
                check(changes.Count > 0 && balance == s.State.Balance && count == s.State.Upgrades.Count && notifications == 0,
                    scenario + "/" + id + ": previzualizare fara mutatii");
                check(s.BuyUpgrade(id), scenario + "/" + id + ": imbunatatire activata");
                foreach (var change in changes)
                {
                    var p = s.Scenario.Products.Find(p => p.Name == change.Label);
                    var r = s.Scenario.Resources.Find(r => r.Name == change.Label);
                    int actual = change.Unit == "zile" ? s.JobDays(p) : change.Unit == "unit." ? s.JobYield(p) :
                        change.Label == "Loturi / loc" ? s.BatchLimit : change.Label == s.Scenario.SlotsLabel ? s.Slots :
                        change.Label == "Cost fix / zi" ? s.DailyUpkeep() : r != null ? s.ResourceCost(r.Id) : s.SellPrice(p.Id);
                    check(actual == change.After, scenario + "/" + id + ": efect real " + change.Label + " " + change.Unit);
                }
            }
            check(s.Slots == s.Scenario.Slots + 1, scenario + ": extinderea adauga un loc real");
            check(s.BatchLimit == (scenario == "agro" ? 3 : 1), scenario + ": limita finala de loturi");
        }
        var farm = Session.NewGame(c, "agro", "easy", 2);
        check(farm.BatchLimit == 1 && !farm.PlanProduction("wheat", 2), "ferma: doua loturi cer dezvoltare");
        farm.State.Balance = 100000;
        farm.BuyUpgrade("irrigation");
        farm.BuyUpgrade("fertilizer");
        check(farm.BatchLimit == 2 && farm.PlanProduction("wheat", 2), "ferma: doua loturi devin productie reala");
        farm.BuyUpgrade("tractor");
        farm.BuyUpgrade("land");
        check(farm.BatchLimit == 3 && farm.PlanProduction("corn", 3, 3), "ferma: trei loturi pe terenul nou");
    }
}
