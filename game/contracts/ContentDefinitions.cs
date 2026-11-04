using System.Collections.Generic;
using Godot;

namespace BusinessSim;

// Defineste structura datelor pentru scenarii, produse, dificultati si evenimente.

public class DifficultyDef
{
    public string Id = "";
    public string Label = "";
    public string Short = "";
    public string Description = "";
    public int StartBalance;
    public int Grant;
    public int Loan;
    public int LoanInstallment;

    public int TotalCash => StartBalance + Grant + Loan;
    public int InitialEquity => StartBalance + Grant;
}

public class ResourceDef
{
    public string Id = "";
    public string Name = "";
    public string Unit = "";
    public string Desc = "";
    public int Price;
}

public class ProductDef
{
    public string Id = "";
    public string Name = "";
    public string Resource = "";
    public string Desc = "";
    public int Input;
    public int Days;
    public int Yield = 1;
    public int BasePrice;
    public bool AutoSell;
}

public class EffectDef
{
    public string Type = "";
    public double Value;
}

public class UpgradeDef
{
    public string Id = "";
    public string Name = "";
    public string Desc = "";
    public int Price;
    public EffectDef Effect = new();
}

public class StaffDef
{
    public string Id = "";
    public string Name = "";
    public string Desc = "";
    public int Salary;
    public EffectDef Effect = new();
}

public class ScenarioDef
{
    public string Id = "";
    public string Name = "";
    public string Short = "";
    public string Description = "";
    public string Goal = "";
    public string Color = "#3c8d4a";
    public string UpkeepLabel = "Costuri fixe";
    public string SlotsLabel = "Locuri";
    public double DailyUpkeep;
    public int Slots = 1;
    public List<ResourceDef> Resources = new();
    public List<ProductDef> Products = new();
    public List<UpgradeDef> Upgrades = new();
    public List<StaffDef> Staff = new();

    public ResourceDef FindResource(string id) => Resources.Find(r => r.Id == id);
    public ProductDef FindProduct(string id) => Products.Find(p => p.Id == id);
    public UpgradeDef FindUpgrade(string id) => Upgrades.Find(u => u.Id == id);
    public StaffDef FindStaff(string id) => Staff.Find(s => s.Id == id);
    public Color AccentColor => Hex.ToColor(Color, new Color(0.24f, 0.55f, 0.29f));
}

public static class Hex
{
    public static Color ToColor(string hex, Color fallback)
    {
        if (string.IsNullOrWhiteSpace(hex))
            return fallback;
        var s = hex.TrimStart('#');
        if (s.Length != 6)
            return fallback;

        int r = System.Convert.ToInt32(s.Substring(0, 2), 16);
        int g = System.Convert.ToInt32(s.Substring(2, 2), 16);
        int b = System.Convert.ToInt32(s.Substring(4, 2), 16);
        return new Color(r / 255f, g / 255f, b / 255f);
    }
}

public class EventEffectDef
{
    public int Balance;
    public double PriceMod;
    public double CostMod;
    public double UpkeepMod;
    public int Duration = 1;
    public string Scope = "*";

    public bool IsEmpty =>
        Balance == 0 && PriceMod == 0 && CostMod == 0 && UpkeepMod == 0;
}

public class EventChoiceDef
{
    public string Label = "";
    public EventEffectDef Effect = new();
}

public class EventDef
{
    public string Id = "";
    public string Scenario = "any";
    public string Title = "";
    public string Text = "";
    public int MinDay = 1;
    public int MaxDay = 999;
    public double Chance = 1.0;
    public List<EventChoiceDef> Choices = new();
    public EventEffectDef Effect = new();

    public bool HasChoices => Choices.Count > 0;
}

public class GameConfig
{
    public string Title = "FirmoQuest";
    public string Subtitle = "";
    public string Currency = "MDL";
    public int DaysTotal = 30;
    public int DaysFor(string difficultyId) => difficultyId switch { "medium" => 20, "hard" => 12, _ => DaysTotal };
    public int TargetProfit = 4000;
    public double EventChance = 0.4;
    public List<DifficultyDef> Difficulties = new();

    public DifficultyDef FindDifficulty(string id) => Difficulties.Find(d => d.Id == id);
}
