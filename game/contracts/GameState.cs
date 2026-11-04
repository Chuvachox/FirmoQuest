using System.Collections.Generic;

namespace BusinessSim;

// Pastreaza progresul serializabil si indicatorii financiari ai partidei.

public class Job
{
    public int Slot;
    public string ProductId = "";
    public int Qty = 1;
    public int DaysLeft;
    public int TotalDays;
}

public class TimedMod
{
    public string Kind = "";     // price | cost | upkeep
    public string Scope = "*";   // id produs/resursă sau "*"
    public double Value;
    public int DaysLeft;
    public string Source = "";

    public bool Matches(string kind, string scope) =>
        Kind == kind && (Scope == "*" || Scope == scope);
}

public class GameState
{
    public string ScenarioId = "";
    public string DifficultyId = "";
    public int Day = 1;
    public int DayLimit = 30; // Salvarile vechi isi pastreaza termenul initial.

    public int StartBalance;
    public int Grant;
    public int Balance;
    public int LoanLeft;
    public int LoanInstallment;

    public int TotalIncome;
    public int TotalExpenses;

    public Dictionary<string, int> Resources = new();
    public Dictionary<string, int> Goods = new();
    public HashSet<string> Upgrades = new();
    public Dictionary<string, int> Staff = new();
    public List<Job> Jobs = new();
    public List<TimedMod> Mods = new();
    public List<string> Log = new();

    public Dictionary<string, List<int>> PriceHistory = new();
    public int HistoryDay;

    public string PendingEventId = "";
    public bool GuideDismissed;
    public int CompletedJobs;

    public List<int> ProfitHistory = new();

    public bool Finished;
    public bool Won;
    public string EndTitle = "";
    public string EndText = "";

    public int InitialEquity => StartBalance + Grant;

    public int Equity => Balance - LoanLeft;

    public int Profit => Equity - InitialEquity;

    public int GetResource(string id) => Resources.TryGetValue(id, out var v) ? v : 0;
    public int GetGood(string id) => Goods.TryGetValue(id, out var v) ? v : 0;
    public int GetStaff(string id) => Staff.TryGetValue(id, out var v) ? v : 0;
}
