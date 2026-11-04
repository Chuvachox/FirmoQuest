using System;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Godot;

namespace BusinessSim;

// Verifica versiunile publicate si ofera descarcarea potrivita platformei.

public partial class UpdateChecker : Node
{
    private const string Repository = "https://github.com/Chuvachox/FirmoQuest";
    private const string Feed = "https://api.github.com/repos/Chuvachox/FirmoQuest/releases?per_page=10";
    public sealed record Release(string Version, string Url);
    private HttpRequest _request;
    private ulong _lastCheck;
    public bool Checking
    {
        get; private set;
    }
    public bool Failed
    {
        get; private set;
    }
    public Release Available
    {
        get; private set;
    }
    internal string VersionOverride;
    public string CurrentVersion => VersionOverride ?? ProjectSettings.GetSetting("application/config/version", "0.5.0-beta.1").AsString();
    public event Action Changed;

    private static (Version Number, int Stage, int Revision)? Parse(string value)
    {
        var match = Regex.Match(value ?? "", @"^v?(\d+\.\d+\.\d+)(?:-(alpha|beta|rc)(?:\.(\d+))?)?$");
        if (!match.Success || !Version.TryParse(match.Groups[1].Value, out var number))
            return null;
        int stage = match.Groups[2].Value switch
        {
            "alpha" => 0,
            "beta" => 1,
            "rc" => 2,
            _ => 3
        };
        int revision = 0;
        if (match.Groups[3].Success && !int.TryParse(match.Groups[3].Value, out revision))
            return null;
        return (number, stage, revision);
    }

    internal static bool Newer(string candidate, string current)
    {
        var a = Parse(candidate);
        var b = Parse(current);
        if (a == null || b == null)
            return false;
        int compare = a.Value.Number.CompareTo(b.Value.Number);
        if (compare != 0)
            return compare > 0;
        if (a.Value.Stage != b.Value.Stage)
            return a.Value.Stage > b.Value.Stage;
        return a.Value.Revision > b.Value.Revision;
    }

    internal static Release SelectRelease(string json, string current, bool android)
    {
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
            throw new JsonException();
        string filename = android ? "FirmoQuest-Android.apk" : "FirmoQuest-Windows-x64.exe";
        Release best = null;
        foreach (var release in document.RootElement.EnumerateArray())
        {
            if (!release.TryGetProperty("draft", out var draft) || draft.ValueKind != JsonValueKind.False ||
                !release.TryGetProperty("tag_name", out var tag) || tag.ValueKind != JsonValueKind.String ||
                !release.TryGetProperty("assets", out var assets) || assets.ValueKind != JsonValueKind.Array)
                continue;
            string version = tag.GetString();
            if (!Newer(version, best?.Version ?? current))
                continue;
            string expected = Repository + "/releases/download/" + Uri.EscapeDataString(version) + "/" + filename;
            foreach (var asset in assets.EnumerateArray())
                if (asset.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String && name.GetString() == filename &&
                    asset.TryGetProperty("state", out var state) && state.ValueKind == JsonValueKind.String && state.GetString() == "uploaded" &&
                    asset.TryGetProperty("size", out var size) && size.TryGetInt64(out long bytes) && bytes > 0 &&
                    asset.TryGetProperty("browser_download_url", out var url) && url.ValueKind == JsonValueKind.String && url.GetString() == expected)
                    best = new Release(version.TrimStart('v'), expected);
        }
        return best;
    }

    public void Check()
    {
        if (Checking || (_lastCheck != 0 && Time.GetTicksMsec() - _lastCheck < 3000))
            return;
        _lastCheck = Time.GetTicksMsec();
        Checking = true;
        Failed = false;
        if (_request == null)
        {
            _request = new HttpRequest { Timeout = 6, BodySizeLimit = 524288, MaxRedirects = 0, UseThreads = true };
            AddChild(_request);
            _request.RequestCompleted += Complete;
        }
        Changed?.Invoke();
        var error = _request.Request(Feed, new[] { "User-Agent: FirmoQuest/" + CurrentVersion,
            "Accept: application/vnd.github+json", "X-GitHub-Api-Version: 2026-03-10" });
        if (error != Error.Ok)
            Complete(-1, 0, Array.Empty<string>(), Array.Empty<byte>());
    }

    internal void Complete(long result, long responseCode, string[] headers, byte[] body)
    {
        Checking = false;
        Failed = result != (long)HttpRequest.Result.Success || responseCode != 200;
        if (!Failed)
        {
            try
            {
                Available = SelectRelease(Encoding.UTF8.GetString(body), CurrentVersion, OS.HasFeature("android"));
            }
            catch (Exception error) when (error is JsonException or InvalidOperationException or FormatException)
            {
                Failed = true;
            }
        }
        Changed?.Invoke();
    }

    private static string Text(string ro, string ru, string en) => Locale.Lang switch { Locale.Ru => ru, Locale.En => en, _ => ro };

    public Control CreateNotice()
    {
        var box = Ui.Column(4);
        box.Name = "updates";
        void Render()
        {
            foreach (var child in box.GetChildren())
            {
                box.RemoveChild(child);
                child.QueueFree();
            }
            string status = Checking ? Text("Se verifică actualizările...", "Проверка обновлений...", "Checking for updates...") :
                Failed ? Text("Verificare indisponibilă. Poți juca offline.", "Проверка недоступна. Можно играть без интернета.", "Check unavailable. You can play offline.") :
                Available != null ? Text("Versiune nouă", "Новая версия", "New version") : Text("Versiune actualizată", "Последняя версия", "Up to date");
            box.AddChild(Ui.Caption("v" + CurrentVersion + " · " + status, 12));
            string caption = Available != null ? Text("Descarcă ", "Скачать ", "Download ") + Available.Version :
                Text("Verifică actualizările", "Проверить обновления", "Check for updates");
            var button = Ui.Button(caption, () =>
            {
                if (Available != null)
                    OS.ShellOpen(Available.Url);
                else
                    Check();
            }, Available != null ? ButtonKind.Primary : ButtonKind.Ghost, 14);
            button.Name = Available != null ? "download-update" : "check-updates";
            button.CustomMinimumSize = new Vector2(0, OS.HasFeature("android") ? 40 : 28);
            button.Disabled = Checking;
            box.AddChild(button);
        }
        box.TreeEntered += () => { Changed += Render; Locale.Changed += Render; Render(); };
        box.TreeExiting += () => { Changed -= Render; Locale.Changed -= Render; };
        return box;
    }

    public override void _ExitTree()
    {
        if (_request == null)
            return;
        _request.RequestCompleted -= Complete;
        _request.CancelRequest();
    }
}
