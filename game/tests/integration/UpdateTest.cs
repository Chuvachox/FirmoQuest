using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;

namespace BusinessSim;

// Verifica ordonarea versiunilor, sursele de descarcare si starea meniului fara conexiune.

public static class UpdateTest
{
    private static object Release(string tag, bool draft = false, bool windows = true, bool android = true)
    {
        var assets = new List<object>();
        foreach (string filename in new[] { "FirmoQuest-Windows-x64.exe", "FirmoQuest-Android.apk" })
            if ((filename.EndsWith(".exe") && windows) || (filename.EndsWith(".apk") && android))
                assets.Add(new
                {
                    name = filename,
                    state = "uploaded",
                    size = 100,
                    browser_download_url = "https://github.com/Chuvachox/FirmoQuest/releases/download/" + tag + "/" + filename
                });
        return new
        {
            tag_name = tag,
            draft,
            prerelease = true,
            assets
        };
    }
    internal static string FutureFeed => JsonSerializer.Serialize(new[] { Release("v0.5.1-alpha") });

    public static void Run(Action<bool, string> check)
    {
        string feed = JsonSerializer.Serialize(new[] { Release("v0.4.9-alpha"), Release("v0.4.10-alpha"),
            Release("v0.9.0-alpha", true), Release("v1.0.0", windows: false, android: false) });
        check(UpdateChecker.SelectRelease(feed, "0.4.3-alpha", false)?.Version == "0.4.10-alpha",
            "actualizari: compara numeric si ignora draft/incomplet");
        check(UpdateChecker.SelectRelease(FutureFeed, "0.4.3-alpha", false)?.Url.EndsWith(".exe") == true &&
            UpdateChecker.SelectRelease(FutureFeed, "0.4.3-alpha", true)?.Url.EndsWith(".apk") == true,
            "actualizari: selecteaza EXE sau APK pentru platforma");
        check(UpdateChecker.SelectRelease(FutureFeed, "0.5.1-alpha", false) == null &&
            UpdateChecker.SelectRelease(FutureFeed, "1.0.0", true) == null, "actualizari: fara reinstalare sau downgrade");
        check(UpdateChecker.Newer("v0.4.3", "0.4.3-alpha") && UpdateChecker.Newer("0.4.3-beta", "0.4.3-alpha") &&
            UpdateChecker.Newer("0.4.3-alpha.2", "0.4.3-alpha.1"), "actualizari: ordoneaza versiunile alpha/beta/stabile");
        check(UpdateChecker.SelectRelease(FutureFeed.Replace("github.com/Chuvachox", "example.org/Chuvachox"), "0.4.3-alpha", false) == null &&
            UpdateChecker.SelectRelease(FutureFeed.Replace("https://", "http://"), "0.4.3-alpha", true) == null,
            "actualizari: descarca numai prin HTTPS din depozitul public");
        string missing = JsonSerializer.Serialize(new[] { Release("v0.5.1-alpha", android: false) });
        check(UpdateChecker.SelectRelease(missing, "0.4.3-alpha", true) == null,
            "actualizari: fara oferta Android cand lipseste APK");
    }

    public static async Task CheckNotice(Main main, Action<bool, string> check, Action<string> shot)
    {
        var checker = main.GetNode<UpdateChecker>("update-checker");
        var notice = main.FindChild("updates", true, false);
        async Task Frames()
        {
            for (int i = 0; i < 3; i++)
                await main.ToSignal(main.GetTree(), SceneTree.SignalName.ProcessFrame);
        }
        checker.Complete((long)HttpRequest.Result.Timeout, 0, Array.Empty<string>(), Array.Empty<byte>());
        await Frames();
        check(checker.Failed && notice.GetChildren().OfType<Button>().Any(b => b.Name == "check-updates" && !b.Disabled),
            "actualizari: offline permite reincercarea fara blocarea jocului");
        shot("qa_updates_offline");
        checker.Complete(0, 200, Array.Empty<string>(), Encoding.UTF8.GetBytes(FutureFeed));
        await Frames();
        check(!checker.Failed && checker.Available?.Version == "0.5.1-alpha" &&
            notice.GetChildren().OfType<Button>().Any(b => b.Name == "download-update" && b.Text.Contains("0.5.1")),
            "actualizari: raspunsul valid afiseaza descarcarea");
        shot("qa_updates_available");
        checker.Complete(0, 200, Array.Empty<string>(), Encoding.UTF8.GetBytes("{invalid"));
        check(checker.Failed && checker.Available?.Version == "0.5.1-alpha", "actualizari: JSON invalid nu prabuseste meniul");
        checker.Complete(0, 200, Array.Empty<string>(), Encoding.UTF8.GetBytes("[]"));
        await Frames();
    }

    public static void Probe(Main main)
    {
        var checker = new UpdateChecker { VersionOverride = "0.0.0-alpha" };
        main.AddChild(checker);
        checker.Changed += () =>
        {
            if (checker.Checking)
                return;
            GD.Print($"UPDATE_CHECK failed={checker.Failed} offer={checker.Available?.Version} url={checker.Available?.Url}");
            main.GetTree().Quit(checker.Failed || checker.Available == null ? 1 : 0);
        };
        checker.Check();
    }
}
