using System;
using System.Collections.Generic;

namespace BusinessSim;

// Valideaza tranzactiile si calculeaza costurile, preturile, capacitatea si productia.

public partial class Session
{
    private double Bonus(string type)
    {
        double sum = 0;

        foreach (var uid in State.Upgrades)
        {
            var u = Scenario.FindUpgrade(uid);
            if (u?.Effect != null && u.Effect.Type == type)
                sum += u.Effect.Value;
        }

        foreach (var kv in State.Staff)
        {
            var st = Scenario.FindStaff(kv.Key);
            if (st?.Effect != null && st.Effect.Type == type)
                sum += st.Effect.Value * kv.Value;
        }

        if (_previewUpgradeId != null && !State.Upgrades.Contains(_previewUpgradeId))
        {
            var preview = Scenario.FindUpgrade(_previewUpgradeId);
            if (preview?.Effect?.Type == type)
                sum += preview.Effect.Value;
        }
        return sum;
    }

    private double Timed(string kind, string scope = "*")
    {
        double sum = 0;
        foreach (var m in State.Mods)
            if (m.Matches(kind, scope))
                sum += m.Value;
        return sum;
    }

    public int Slots => Scenario.Slots + (int)Bonus("slots");

    public int JobDays(ProductDef p) =>
        Math.Max(1, p.Days - (int)Bonus("reduceDays"));

    public int JobYield(ProductDef p) =>
        Math.Max(1, p.Yield + (int)Bonus("yield"));

    public int ResourceCost(string id)
    {
        var r = Scenario.FindResource(id);
        if (r == null)
            return 0;
        double m = (1 + Timed("cost", id)) * (1 - Math.Clamp(Bonus("costDown"), 0, 0.9));
        return Math.Max(1, (int)Math.Round(r.Price * m));
    }

    public int SellPrice(string id)
    {
        var p = Scenario.FindProduct(id);
        if (p == null)
            return 0;

        double phase = 0;
        foreach (var c in id)
            phase += c;

        double fluctuation = 1 + 0.12 * Math.Sin(State.Day * 0.9 + phase * 0.7);
        double m = (1 + Bonus("priceUp")) * (1 + Timed("price", id)) * fluctuation;
        return Math.Max(1, (int)Math.Round(p.BasePrice * m));
    }

    public int DailyUpkeep()
    {
        double fixedCost = Scenario.DailyUpkeep
            * (1 - Math.Clamp(Bonus("upkeepDown"), 0, 0.9))
            * (1 + Timed("upkeep"));

        int salaries = 0;
        foreach (var kv in State.Staff)
        {
            var st = Scenario.FindStaff(kv.Key);
            if (st != null)
                salaries += st.Salary * kv.Value;
        }

        return Math.Max(0, (int)Math.Round(fixedCost) + salaries);
    }

    public bool BuyResource(string id, int qty)
    {
        if (State.Finished)
            return Fail("Jocul s-a încheiat.");
        if (PendingEvent != null)
            return Fail("Rezolvă mai întâi evenimentul.");
        var r = Scenario.FindResource(id);
        if (r == null || qty <= 0)
            return false;

        long requestedCost = (long)ResourceCost(id) * qty;
        if (requestedCost > State.Balance || requestedCost > int.MaxValue)
            return Fail("Fonduri insuficiente.");
        int cost = (int)requestedCost;

        Spend(cost, $"Cumpărat {qty} × {r.Name}");
        State.Resources[id] = State.GetResource(id) + qty;
        Notice = "";
        Changed?.Invoke();
        return true;
    }

    public bool StartJob(string productId, int qty = 1, int slot = -1)
    {
        if (State.Finished)
            return Fail("Jocul s-a încheiat.");
        if (PendingEvent != null)
            return Fail("Rezolvă mai întâi evenimentul.");
        var p = Scenario.FindProduct(productId);
        if (p == null || qty <= 0)
            return false;

        if (qty > BatchLimit)
            return Fail($"Maximum {BatchLimit} loturi pe loc.");
        if (slot < -1 || slot >= Slots || (slot >= 0 && State.Jobs.Exists(j => j.Slot == slot)))
            return Fail("Locul ales nu este liber.");
        if (slot == -1)
        {
            slot = 0;
            while (slot < Slots && State.Jobs.Exists(j => j.Slot == slot))
                slot++;
        }

        if (State.Jobs.Count >= Slots)
            return Fail($"Nu mai ai {Scenario.SlotsLabel.ToLower()} libere ({Slots}/{Slots}).");

        long requestedInput = (long)p.Input * qty;
        if (requestedInput > int.MaxValue || State.GetResource(p.Resource) < requestedInput)
            return Fail($"Nu ai suficiente resurse pentru {p.Name}.");
        int need = (int)requestedInput;

        State.Resources[p.Resource] = State.GetResource(p.Resource) - need;
        int days = JobDays(p);
        State.Jobs.Add(new Job { Slot = slot, ProductId = productId, Qty = qty, DaysLeft = days, TotalDays = days });
        Log($"Ai început: {p.Name} × {qty} ({days} {(days == 1 ? "zi" : "zile")})");
        Notice = "";
        Changed?.Invoke();
        return true;
    }

    public bool Sell(string productId, int qty)
    {
        if (State.Finished)
            return Fail("Jocul s-a încheiat.");
        if (PendingEvent != null)
            return Fail("Rezolvă mai întâi evenimentul.");
        var p = Scenario.FindProduct(productId);
        if (p == null)
            return false;

        int have = State.GetGood(productId);
        qty = Math.Min(qty, have);
        if (qty <= 0)
            return Fail("Nu ai marfă pe stoc.");

        int unit = SellPrice(productId);
        int total = unit * qty;
        State.Goods[productId] = have - qty;
        Earn(total, $"Vândut {qty} × {p.Name} la {unit} {Currency}/buc");
        Notice = "";
        Changed?.Invoke();
        return true;
    }

    public bool BuyUpgrade(string id)
    {
        if (State.Finished)
            return Fail("Jocul s-a încheiat.");
        if (PendingEvent != null)
            return Fail("Rezolvă mai întâi evenimentul.");
        var u = Scenario.FindUpgrade(id);
        if (u == null)
            return false;
        if (!UpgradeUnlocked(id))
            return Fail("Cumpără mai întâi îmbunătățirea anterioară de pe hartă.");
        if (State.Upgrades.Contains(id))
            return Fail("Deja deținut.");

        if (u.Price > State.Balance)
            return Fail("Fonduri insuficiente.");
        Spend(u.Price, $"Îmbunătățire: {u.Name}");
        State.Upgrades.Add(id);
        Log($"Ai cumpărat: {u.Name}.");
        Notice = "";
        Changed?.Invoke();
        return true;
    }

    public bool Hire(string id)
    {
        if (State.Finished)
            return Fail("Jocul s-a încheiat.");
        if (PendingEvent != null)
            return Fail("Rezolvă mai întâi evenimentul.");
        var s = Scenario.FindStaff(id);
        if (s == null)
            return false;
        if (s.Salary > State.Balance)
            return Fail("Fonduri insuficiente pentru salariu.");

        Spend(s.Salary, $"Angajat: {s.Name} (avans)");
        State.Staff[id] = State.GetStaff(id) + 1;
        Log($"Ai angajat: {s.Name} ({State.GetStaff(id)} bucăți, {s.Salary} {Currency}/zi).");
        Notice = "";
        Changed?.Invoke();
        return true;
    }

    public bool Fire(string id)
    {
        if (State.Finished)
            return Fail("Jocul s-a încheiat.");
        if (PendingEvent != null)
            return Fail("Rezolvă mai întâi evenimentul.");
        var s = Scenario.FindStaff(id);
        if (s == null || State.GetStaff(id) <= 0)
            return false;

        State.Staff[id] = State.GetStaff(id) - 1;
        if (State.Staff[id] <= 0)
            State.Staff.Remove(id);
        Log($"Ai concediat: {s.Name}.");
        Notice = "";
        Changed?.Invoke();
        return true;
    }

    public void Earn(int amount, string reason)
    {
        if (amount <= 0)
            return;
        State.Balance += amount;
        State.TotalIncome += amount;
        Log($"+{amount} {Currency} - {reason}");
    }

    public void Spend(int amount, string reason)
    {
        if (amount <= 0)
            return;
        State.Balance -= amount;
        State.TotalExpenses += amount;
        Log($"-{amount} {Currency} - {reason}");
    }

    private bool Fail(string msg)
    {
        Notice = msg;
        Changed?.Invoke();
        return false;
    }

    private void Log(string msg)
    {
        State.Log.Add($"[Ziua {State.Day}] {msg}");
        if (State.Log.Count > 200)
            State.Log.RemoveAt(0);
    }

    private void Complete(Job j)
    {
        var p = Scenario.FindProduct(j.ProductId);
        if (p == null)
            return;
        State.CompletedJobs++;

        if (p.AutoSell)
        {
            int unit = SellPrice(p.Id);
            int total = unit * j.Qty;
            Earn(total, $"Contract livrat: {p.Name} × {j.Qty}");
        }
        else
        {
            int amount = JobYield(p) * j.Qty;
            State.Goods[p.Id] = State.GetGood(p.Id) + amount;
            Log($"Recoltă gata: +{amount} {p.Name} pe stoc.");
        }
    }

    private void RepayLoan()
    {
        int pay = Math.Min(State.LoanLeft, State.LoanInstallment);
        if (pay <= 0)
            return;
        State.Balance -= pay;
        State.LoanLeft -= pay;
        Log($"-{pay} {Currency} - rambursare credit (mai ai {State.LoanLeft} {Currency})");
    }
}
