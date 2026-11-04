using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;

namespace BusinessSim;

// Verifica alegerea limbii si pastrarea sesiunii din butoanele reale ale interfetei.

public static class LanguageUiTest
{
    public static async Task Run(Main main, Action<bool, string> check)
    {
        Locale.T("Joacă");
        string previousLanguage = Locale.Lang;
        string languagePath = ProjectSettings.GlobalizePath("user://language.txt");
        byte[] previousFile = System.IO.File.Exists(languagePath) ? System.IO.File.ReadAllBytes(languagePath) : null;
        GameScreen game = null;
        IEnumerable<Node> Walk(Node parent)
        {
            yield return parent;
            foreach (var child in parent.GetChildren())
            foreach (var nested in Walk(child))
                yield return nested;
        }
        async Task Frames(int count)
        {
            for (int i = 0; i < count; i++)
                await main.ToSignal(main.GetTree(), SceneTree.SignalName.ProcessFrame);
        }
        async Task<bool> Click(Button button)
        {
            if (button == null || button.Disabled)
                return false;
            for (Node parent = button.GetParent(); parent != null; parent = parent.GetParent())
                if (parent is ScrollContainer scroll)
                    scroll.EnsureControlVisible(button);
            await Frames(4);
            var point = button.GetGlobalRect().GetCenter();
            var viewport = main.GetViewport();
            if (!viewport.GetVisibleRect().HasPoint(point))
                return false;
            viewport.PushInput(new InputEventMouseMotion { Position = point, GlobalPosition = point }, true);
            foreach (bool pressed in new[] { true, false })
                viewport.PushInput(new InputEventMouseButton
                {
                    Position = point,
                    GlobalPosition = point,
                    ButtonIndex = MouseButton.Left,
                    Pressed = pressed
                }, true);
            await Frames(7);
            return true;
        }
        Button Named(string name) => Walk(game).OfType<Button>().FirstOrDefault(b => b.Name == name);
        try
        {
            foreach (string scenario in new[] { "agro", "it" })
            {
                var session = Session.NewGame(Content.Load(), scenario, "medium", 97);
                game = new GameScreen(session, () => { }, () => { }, () => true);
                main.AddChild(game);
                await Frames(6);
                game.Navigate("development");
                await Frames(5);
                string snapshot = JsonSerializer.Serialize(session.State, new JsonSerializerOptions { IncludeFields = true });
                foreach (string code in new[] { Locale.Ru, Locale.En, Locale.Ro })
                {
                    bool opened = await Click(Named("language-menu"));
                    var panel = Walk(game).OfType<PanelContainer>().FirstOrDefault(p => p.Name == "modal-panel");
                    check(opened && Named("choose-language-" + code) != null && panel != null &&
                        panel.GetGlobalRect().Position.X > main.GetViewport().GetVisibleRect().Size.X / 2 &&
                        main.GetViewport().GetVisibleRect().Encloses(panel.GetGlobalRect()),
                        scenario + "/" + code + ": meniul separat se deschide din colt");
                    check(await Click(Named("choose-language-" + code)) && Locale.Lang == code && Named("choose-language-" + code) == null,
                        scenario + "/" + code + ": butonul schimba limba si inchide meniul");
                    check(game.CurrentSection == "development" &&
                        snapshot == JsonSerializer.Serialize(session.State, new JsonSerializerOptions { IncludeFields = true }),
                        scenario + "/" + code + ": sesiunea si sectiunea sunt pastrate");
                    string[] labels = Walk(game).OfType<Label>().Select(l => l.Text).ToArray();
                    string product = scenario == "agro" ? code == Locale.Ru ? "Пшеница:" : code == Locale.En ? "Wheat:" : "Grâu:" :
                        code == Locale.Ru ? "Сайт-визитка:" : code == Locale.En ? "Landing site:" : "Site de prezentare:";
                    string capacity = code == Locale.Ru ? "одновременные контракты" : code == Locale.En ? "concurrent contracts" : "Contracte simultane";
                    check(labels.Any(t => t.StartsWith(product)) &&
                        (scenario != "it" || labels.Any(t => t.Contains(capacity, StringComparison.OrdinalIgnoreCase))),
                        scenario + "/" + code + ": efectele si capacitatea sunt traduse la afisare");
                    game.Navigate("home");
                    game.SelectSlot(0);
                    await Frames(5);
                    game.SelectSlot(-1);
                    game.Navigate("journal");
                    await Frames(5);
                    string objective = code == Locale.Ru ? "Первое производство" : code == Locale.En ? "First production" : "Prima producție";
                    check(Walk(game).OfType<Label>().Any(l => l.Text.Contains(objective)), scenario + "/" + code + ": obiective traduse");
                    game.Navigate("development");
                    await Frames(5);
                }
                main.RemoveChild(game);
                game.QueueFree();
                game = null;
                await Frames(3);
            }
        }
        finally
        {
            if (game != null)
            {
                main.RemoveChild(game);
                game.QueueFree();
            }
            Locale.Set(previousLanguage);
            await Frames(4);
            if (previousFile != null)
                System.IO.File.WriteAllBytes(languagePath, previousFile);
            else if (System.IO.File.Exists(languagePath))
                System.IO.File.Delete(languagePath);
        }
    }
}
