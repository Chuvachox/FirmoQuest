using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Godot;

namespace BusinessSim;

// Scrie salvarea locala atomic si valideaza datele la reluarea partidei.

public static class SaveGame
{
    private sealed class Envelope
    {
        public int Version = 1;
        public GameState State;
    }

    private static readonly JsonSerializerOptions Options = new() { IncludeFields = true };
    public static string DefaultPath => ProjectSettings.GlobalizePath("user://campaign.json");

    public static bool Write(Session session, out string error, string path = null)
    {
        error = "";
        try
        {
            path ??= DefaultPath;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string json = JsonSerializer.Serialize(new Envelope { State = session.State }, Options);
            System.IO.File.WriteAllText(path + ".tmp", json);
            if (System.IO.File.Exists(path))
                System.IO.File.Replace(path + ".tmp", path, path + ".bak");
            else
                System.IO.File.Move(path + ".tmp", path);
            return true;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException)
        {
            GD.PushWarning("SaveGame: " + e.Message);
            error = "Salvarea nu a reușit. Verifică spațiul și accesul la folderul jocului.";
            return false;
        }
    }

    public static Session Read(Content content, out string error, string path = null)
    {
        error = "";
        path ??= DefaultPath;
        try
        {
            if (!System.IO.File.Exists(path))
                return null;
            if (new FileInfo(path).Length > 2_000_000)
                throw new JsonException();
            var data = JsonSerializer.Deserialize<Envelope>(System.IO.File.ReadAllText(path), Options);
            var st = data?.State;
            var sc = st == null ? null : content.GetScenario(st.ScenarioId);
            if (data?.Version != 1 || sc == null || content.Config.FindDifficulty(st.DifficultyId) == null ||
                st.DayLimit < 1 || st.DayLimit > content.Config.DaysTotal || st.Day < 1 || st.Day > st.DayLimit + 1 ||
                st.Resources == null || st.Goods == null || st.Upgrades == null || st.Staff == null ||
                st.Jobs == null || st.Mods == null || st.Log == null || st.PriceHistory == null ||
                st.ProfitHistory == null || st.PendingEventId == null ||
                st.Jobs.Count > sc.Slots + 1 || st.LoanLeft < 0 ||
                st.Resources.Any(x => sc.FindResource(x.Key) == null || x.Value < 0) ||
                st.Goods.Any(x => sc.FindProduct(x.Key) == null || x.Value < 0) ||
                st.Upgrades.Any(x => sc.FindUpgrade(x) == null) ||
                st.Staff.Any(x => sc.Staff.Find(y => y.Id == x.Key) == null || x.Value < 0) ||
                st.Jobs.Any(x => x == null || sc.FindProduct(x.ProductId) == null || x.Qty < 1 ||
                    x.Qty > (sc.Id == "agro" ? 3 : 1) || x.Slot < 0 || x.Slot >= sc.Slots + 1 ||
                    x.DaysLeft < 1 || x.TotalDays < x.DaysLeft) ||
                st.Jobs.Select(x => x.Slot).Distinct().Count() != st.Jobs.Count ||
                st.Mods.Any(x => x == null || !double.IsFinite(x.Value)) ||
                (st.PendingEventId != "" && !content.Events.Exists(e => e.Id == st.PendingEventId &&
                    (e.Scenario == "any" || e.Scenario == st.ScenarioId))))
                throw new JsonException();
            if (!st.Finished && st.DifficultyId == "hard" && st.DayLimit == 10)
                st.DayLimit = content.Config.DaysFor("hard");
            return Session.Restore(content, st);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException or NotSupportedException)
        {
            error = "Salvarea nu poate fi citită. Fișierul a fost păstrat.";
            return null;
        }
    }
}

public partial class Session
{
    internal static Session Restore(Content content, GameState state) => new()
    {
        Content = content,
        State = state,
        Scenario = content.GetScenario(state.ScenarioId),
        Difficulty = content.Config.FindDifficulty(state.DifficultyId),
    };
}
