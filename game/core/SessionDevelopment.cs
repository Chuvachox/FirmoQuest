using System;
using System.Collections.Generic;

namespace BusinessSim;

// Aplica dependentele upgrade-urilor si previzualizeaza efectele prin formulele economiei.

public partial class Session
{
    public string UpgradePrerequisite(string id) => id switch
    {
        "fertilizer" => "irrigation",
        "land" => "tractor",
        "solar" => "bio",
        "automation" => "workstations",
        "team" => "licenses",
        "remote" => "marketing",
        _ => "",
    };

    public bool UpgradeUnlocked(string id)
    {
        string required = UpgradePrerequisite(id);
        return required == "" || State.Upgrades.Contains(required);
    }

    private string _previewUpgradeId;
    private bool HasDevelopment(string id) => State.Upgrades.Contains(id) || _previewUpgradeId == id;
    public int BatchLimit => Scenario.Id != "agro" ? 1 : HasDevelopment("land") ? 3 : HasDevelopment("fertilizer") ? 2 : 1;

    public readonly record struct DevelopmentChange(string Label, int Before, int After, string Unit);

    private List<(string label, int value, string unit)> DevelopmentValues()
    {
        var values = new List<(string, int, string)> { (Scenario.SlotsLabel, Slots, ""), ("Loturi / loc", BatchLimit, ""),
            ("Cost fix / zi", DailyUpkeep(), Currency) };
        foreach (var p in Scenario.Products)
        {
            values.Add((p.Name, JobDays(p), "zile"));
            if (!p.AutoSell)
                values.Add((p.Name, JobYield(p), "unit."));
            values.Add((p.Name, SellPrice(p.Id), Currency));
        }
        foreach (var r in Scenario.Resources)
            values.Add((r.Name, ResourceCost(r.Id), Currency));
        return values;
    }

    public IReadOnlyList<DevelopmentChange> PreviewUpgrade(string id)
    {
        if (State.Upgrades.Contains(id) || Scenario.FindUpgrade(id) == null)
            return Array.Empty<DevelopmentChange>();
        var before = DevelopmentValues();
        try
        {
            _previewUpgradeId = id;
            var after = DevelopmentValues();
            var changes = new List<DevelopmentChange>();
            for (int i = 0; i < before.Count; i++)
                if (before[i].value != after[i].value)
                    changes.Add(new(before[i].label, before[i].value, after[i].value, after[i].unit));
            return changes;
        }
        finally { _previewUpgradeId = null; }
    }

    public bool PlanProduction(string productId, int qty, int slot = -1)
    {
        var p = Scenario.FindProduct(productId);
        if (State.Finished || State.PendingEventId != "" || p == null || qty < 1 || qty > BatchLimit)
            return Fail("Acțiunea nu este disponibilă acum.");
        if (State.Jobs.Count >= Slots)
            return Fail("Toate locurile sunt ocupate.");
        if (slot >= Slots || slot < -1 || (slot >= 0 && State.Jobs.Exists(j => j.Slot == slot)))
            return Fail("Locul ales nu este liber.");
        int need = p.Input * qty;
        int missing = Math.Max(0, need - State.GetResource(p.Resource));
        long cost = (long)missing * ResourceCost(p.Resource);
        if (cost > State.Balance)
            return Fail("Fonduri insuficiente pentru resurse.");
        if (missing > 0 && !BuyResource(p.Resource, missing))
            return false;
        return StartJob(productId, qty, slot);
    }

    public IReadOnlyList<(string id, string title, string detail, bool done)> Objectives => new[]
    {
        ("start", "Prima producție", "Pornește o cultură sau un contract.", State.Jobs.Count > 0 || State.CompletedJobs > 0),
        ("delivery", "Prima livrare", "Încheie zilele până când o lucrare este gata.", State.CompletedJobs > 0),
        ("income", "Primul venit", "Vinde recolta; contractele IT se plătesc la livrare.", State.TotalIncome > 0),
        ("develop", "Afacere în creștere", "Cumpără o îmbunătățire de pe harta dezvoltării.", State.Upgrades.Count > 0),
        ("profit", "Pe profit", "Acoperă cheltuielile și depășește capitalul inițial.", State.Profit > 0),
    };

    public void DismissGuide()
    {
        State.GuideDismissed = true;
        Changed?.Invoke();
    }

    public EventDef PendingEvent => Content.Events.Find(e => e.Id == State.PendingEventId);

    public bool ResolvePendingEvent(int choice = 0)
    {
        var ev = PendingEvent;
        if (ev == null || State.Finished)
            return false;
        if (ev.HasChoices && (choice < 0 || choice >= ev.Choices.Count))
            return false;
        State.PendingEventId = "";
        if (ev.HasChoices)
            ApplyChoice(ev.Choices[choice]);
        else
            ApplyEffect(ev.Effect);
        return true;
    }
}
