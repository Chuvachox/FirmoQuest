using System.Collections.Generic;
using Godot;

namespace BusinessSim;

// Citeste si valideaza configuratia, scenariile si evenimentele din JSON.

public class Content
{
    public GameConfig Config = new();
    public Dictionary<string, ScenarioDef> Scenarios = new();
    public List<EventDef> Events = new();

    public ScenarioDef GetScenario(string id) =>
        Scenarios.TryGetValue(id, out var s) ? s : null;

    public static Content Load()
    {
        var content = new Content();
        content.Config = ParseConfig(Read("res://data/economy/config.json"));
        content.Scenarios["agro"] = ParseScenario(Read("res://data/economy/agro.json"));
        content.Scenarios["it"] = ParseScenario(Read("res://data/economy/it.json"));
        content.Events = ParseEvents(Read("res://data/events/events.json"));
        return content;
    }

    private static Variant Read(string path)
    {
        if (!FileAccess.FileExists(path))
        {
            GD.PrintErr($"Lipsă fișier de conținut: {path}");
            throw new System.InvalidOperationException($"Conținut lipsă sau invalid: {path}");
        }

        using var f = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        var v = Json.ParseString(f.GetAsText());
        if (v.VariantType == Variant.Type.Nil)
        {
            GD.PrintErr($"JSON invalid: {path}");
            throw new System.InvalidOperationException($"Conținut lipsă sau invalid: {path}");
        }
        return v;
    }

    private static Variant At(Variant obj, string key)
    {
        if (obj.VariantType != Variant.Type.Dictionary)
            return default;
        var d = obj.AsGodotDictionary();
        return d.ContainsKey(key) ? d[key] : default;
    }

    private static string S(Variant obj, string key, string def = "")
    {
        var v = At(obj, key);
        return v.VariantType == Variant.Type.Nil ? def : v.AsString();
    }

    private static int I(Variant obj, string key, int def = 0)
    {
        var v = At(obj, key);
        return v.VariantType == Variant.Type.Nil ? def : (int)v.AsDouble();
    }

    private static double F(Variant obj, string key, double def = 0)
    {
        var v = At(obj, key);
        return v.VariantType == Variant.Type.Nil ? def : v.AsDouble();
    }

    private static bool B(Variant obj, string key, bool def = false)
    {
        var v = At(obj, key);
        return v.VariantType == Variant.Type.Nil ? def : v.AsBool();
    }

    private static List<Variant> A(Variant obj, string key)
    {
        var res = new List<Variant>();
        var v = At(obj, key);
        if (v.VariantType != Variant.Type.Array)
            return res;
        foreach (var item in v.AsGodotArray())
            res.Add(item);
        return res;
    }

    private static EffectDef ParseEffect(Variant v) => new()
    {
        Type = S(v, "type"),
        Value = F(v, "value"),
    };

    private static EventEffectDef ParseEventEffect(Variant v) => new()
    {
        Balance = I(v, "balance"),
        PriceMod = F(v, "priceMod"),
        CostMod = F(v, "costMod"),
        UpkeepMod = F(v, "upkeepMod"),
        Duration = System.Math.Max(1, I(v, "duration", 1)),
        Scope = S(v, "scope", "*"),
    };

    private static GameConfig ParseConfig(Variant root)
    {
        var cfg = new GameConfig();
        if (root.VariantType == Variant.Type.Nil)
            return cfg;

        cfg.Title = S(root, "title", cfg.Title);
        cfg.Subtitle = S(root, "subtitle");
        cfg.Currency = S(root, "currency", "MDL");
        cfg.DaysTotal = I(root, "daysTotal", 30);
        cfg.TargetProfit = I(root, "targetProfit", 4000);
        cfg.EventChance = F(root, "eventChance", 0.4);

        foreach (var it in A(root, "difficulties"))
        {
            cfg.Difficulties.Add(new DifficultyDef
            {
                Id = S(it, "id"),
                Label = S(it, "label"),
                Short = S(it, "short"),
                Description = S(it, "description"),
                StartBalance = I(it, "startBalance"),
                Grant = I(it, "grant"),
                Loan = I(it, "loan"),
                LoanInstallment = I(it, "loanInstallment"),
            });
        }
        return cfg;
    }

    private static ScenarioDef ParseScenario(Variant root)
    {
        var s = new ScenarioDef();
        if (root.VariantType == Variant.Type.Nil)
            return s;

        s.Id = S(root, "id");
        s.Name = S(root, "name");
        s.Short = S(root, "short", s.Name);
        s.Description = S(root, "description");
        s.Goal = S(root, "goal");
        s.Color = S(root, "color", s.Color);
        s.UpkeepLabel = S(root, "upkeepLabel", s.UpkeepLabel);
        s.SlotsLabel = S(root, "slotsLabel", s.SlotsLabel);
        s.DailyUpkeep = F(root, "dailyUpkeep");
        s.Slots = System.Math.Max(1, I(root, "slots", 1));

        foreach (var it in A(root, "resources"))
        {
            s.Resources.Add(new ResourceDef
            {
                Id = S(it, "id"),
                Name = S(it, "name"),
                Unit = S(it, "unit"),
                Desc = S(it, "desc"),
                Price = I(it, "price"),
            });
        }

        foreach (var it in A(root, "products"))
        {
            s.Products.Add(new ProductDef
            {
                Id = S(it, "id"),
                Name = S(it, "name"),
                Resource = S(it, "resource"),
                Desc = S(it, "desc"),
                Input = I(it, "input", 1),
                Days = System.Math.Max(1, I(it, "days", 1)),
                Yield = System.Math.Max(1, I(it, "yield", 1)),
                BasePrice = I(it, "basePrice"),
                AutoSell = B(it, "autoSell"),
            });
        }

        foreach (var it in A(root, "upgrades"))
        {
            s.Upgrades.Add(new UpgradeDef
            {
                Id = S(it, "id"),
                Name = S(it, "name"),
                Desc = S(it, "desc"),
                Price = I(it, "price"),
                Effect = ParseEffect(At(it, "effect")),
            });
        }

        foreach (var it in A(root, "staff"))
        {
            s.Staff.Add(new StaffDef
            {
                Id = S(it, "id"),
                Name = S(it, "name"),
                Desc = S(it, "desc"),
                Salary = I(it, "salary"),
                Effect = ParseEffect(At(it, "effect")),
            });
        }

        return s;
    }

    private static List<EventDef> ParseEvents(Variant root)
    {
        var list = new List<EventDef>();
        foreach (var it in A(root, "events"))
        {
            var ev = new EventDef
            {
                Id = S(it, "id"),
                Scenario = S(it, "scenario", "any"),
                Title = S(it, "title"),
                Text = S(it, "text"),
                MinDay = I(it, "minDay", 1),
                MaxDay = I(it, "maxDay", 999),
                Chance = F(it, "chance", 1.0),
                Effect = ParseEventEffect(At(it, "effect")),
            };

            foreach (var c in A(it, "choices"))
            {
                ev.Choices.Add(new EventChoiceDef
                {
                    Label = S(c, "label"),
                    Effect = ParseEventEffect(At(c, "effect")),
                });
            }

            list.Add(ev);
        }
        return list;
    }
}
