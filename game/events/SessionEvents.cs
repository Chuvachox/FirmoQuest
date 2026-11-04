using System;
using System.Collections.Generic;

namespace BusinessSim;

// Selecteaza evenimentele eligibile si aplica efectele lor asupra sesiunii.

public partial class Session
{
    private EventDef RollEvent()
    {
        if (_rng.NextDouble() > Config.EventChance)
            return null;

        var pool = new List<EventDef>();
        foreach (var e in Content.Events)
        {
            bool rightScenario = e.Scenario == "any" || e.Scenario == Scenario.Id;
            if (rightScenario && State.Day >= e.MinDay && State.Day <= e.MaxDay)
                pool.Add(e);
        }

        if (pool.Count == 0)
            return null;
        var pick = pool[_rng.Next(pool.Count)];
        return _rng.NextDouble() <= pick.Chance ? pick : null;
    }

    public void ApplyEffect(EventEffectDef fx)
    {
        if (fx == null || State.Finished)
            return;
        State.PendingEventId = "";

        if (fx.Balance > 0)
            Earn(fx.Balance, "Eveniment");
        else if (fx.Balance < 0)
            Spend(-fx.Balance, "Eveniment");

        if (fx.PriceMod != 0)
            AddMod("price", fx);
        if (fx.CostMod != 0)
            AddMod("cost", fx);
        if (fx.UpkeepMod != 0)
            AddMod("upkeep", fx);

        CheckEnd();
        Changed?.Invoke();
    }

    public void ApplyChoice(EventChoiceDef choice)
    {
        if (choice == null)
            return;
        ApplyEffect(choice.Effect);
    }

    private void AddMod(string kind, EventEffectDef fx)
    {
        double value = kind switch
        {
            "price" => fx.PriceMod,
            "cost" => fx.CostMod,
            _ => fx.UpkeepMod,
        };

        State.Mods.Add(new TimedMod
        {
            Kind = kind,
            Scope = fx.Scope ?? "*",
            Value = value,
            DaysLeft = fx.Duration,
            Source = kind,
        });
    }
}
