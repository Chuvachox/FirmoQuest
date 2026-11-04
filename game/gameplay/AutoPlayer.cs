using Godot;

namespace BusinessSim;

// Executa o strategie reproductibila pentru verificari si demonstratii automate.

public static class AutoPlayer
{
    public static EventDef Turn(Session s)
    {
        if (s == null || s.State.Finished)
            return null;

        var sc = s.Scenario;
        var st = s.State;

        int reserve = (int)(s.DailyUpkeep() * 2.5) + 300;

        int free = s.Slots - st.Jobs.Count;
        if (free > 0)
        {
            var p = sc.Products[st.Day % sc.Products.Count];

            int missing = p.Input * free - st.GetResource(p.Resource);
            for (int i = 0; i < missing; i++)
            {
                int cost = s.ResourceCost(p.Resource);
                if (st.Balance - cost < reserve)
                    break;
                if (!s.BuyResource(p.Resource, 1))
                    break;
            }

            while (st.Jobs.Count < s.Slots && st.GetResource(p.Resource) >= p.Input)
                if (!s.StartJob(p.Id, 1))
                    break;
        }

        foreach (var g in sc.Products)
        {
            if (g.AutoSell)
                continue;
            int qty = st.GetGood(g.Id);
            if (qty > 0)
                s.Sell(g.Id, qty);
        }

        return s.EndDay();
    }

    public static void Resolve(Session s, EventDef ev)
    {
        if (ev == null)
            return;
        if (ev.HasChoices)
            s.ApplyChoice(ev.Choices[0]);
        else
            s.ApplyEffect(ev.Effect);
    }
}
