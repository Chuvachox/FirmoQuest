using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace BusinessSim;

// Parcurge meniurile, actiunile si rezultatele pentru fiecare scenariu si dificultate.

public static class QaUiTest
{
    private static Main _main;
    private static Action<string> _shot;
    private static int _failed;
    private static int _passed;
    private static string _ctx = "";
    private static readonly List<string> _lines = new();

    public static async void Run(Main main, Action<string> shot)
    {
        _main = main;
        string qaDirectory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "businesssim-ui-" + Guid.NewGuid().ToString("N"));
        main.QaSavePath = System.IO.Path.Combine(qaDirectory, "campaign.json");
        _shot = shot;
        _failed = 0;
        _passed = 0;
        _lines.Clear();

        GD.Print("=== TEST DE PARCURS AL INTERFEȚEI (--qatest) ===");
        if (DisplayServer.GetName() == "headless")
            main.GetViewport().NotifyMouseEntered();

        try
        {
            await Frames(5);                    // lasă meniul să se așeze
            await WalkMenuAndHelp();
            await UpdateTest.CheckNotice(main, Check, _shot);
            await TouchScrollTest.Run(main, Check);
            await LanguageUiTest.Run(main, Check);

            var combos = new[]
            {
                ("agro", "Agricultură", "easy", "Ușor"),
                ("agro", "Agricultură", "medium", "Mediu"),
                ("agro", "Agricultură", "hard", "Complex"),
                ("it", "Companie IT", "easy", "Ușor"),
                ("it", "Companie IT", "medium", "Mediu"),
                ("it", "Companie IT", "hard", "Complex"),
            };
            for (int i = 0; i < combos.Length; i++)
            {
                var (scId, scName, diffId, diffLabel) = combos[i];
                await WalkGame(scId, scName, diffId, diffLabel, useRetry: i % 2 == 0);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr("QA - excepție: " + e);
            _failed++;
            _lines.Add("EXCEPȚIE: " + e.Message);
        }

        string summary = $"QA: {_passed} treceri, {_failed} eșecuri";
        GD.Print($"--- {summary} ---");
        SaveResult(summary);
        if (System.IO.Directory.Exists(qaDirectory) && System.IO.Path.GetFullPath(qaDirectory).StartsWith(
            System.IO.Path.GetFullPath(System.IO.Path.GetTempPath()), StringComparison.OrdinalIgnoreCase))
            System.IO.Directory.Delete(qaDirectory, true);
        main.QaSavePath = null;
        await _main.Shutdown(_failed);
    }

    private static void SaveResult(string summary)
    {
        try
        {
            System.IO.File.WriteAllText(
                ProjectSettings.GlobalizePath("user://qa_result.txt"),
                summary + System.Environment.NewLine + string.Join(System.Environment.NewLine, _lines));
        }
        catch (Exception e)
        {
            GD.PrintErr("QA - nu am putut salva rezultatul: " + e.Message);
        }
    }

    private static void Check(bool ok, string name)
    {
        string line = (ok ? "PASS  " : "FAIL  ") + name;
        _lines.Add(line);
        if (ok)
            _passed++;
        else
            _failed++;
        GD.Print($"  {line}");
    }

    private static async Task Frames(int n)
    {
        for (int i = 0; i < n; i++)
            await _main.ToSignal(_main.GetTree(), SceneTree.SignalName.ProcessFrame);
    }

    private static void Walk(Node n, Action<Node> visit)
    {
        if (n == null || !GodotObject.IsInstanceValid(n))
            return;
        visit(n);
        for (int i = 0; i < n.GetChildCount(); i++)
            Walk(n.GetChild(i), visit);
    }

    private static List<Button> Buttons(string text)
    {
        var res = new List<Button>();
        Walk(_main, n => { if (n is Button b && (b.Text == text || b.Text == Locale.T(text))) res.Add(b); });
        return res;
    }

    private static bool VisibleButton(string text) =>
        Buttons(text).Exists(b => b.IsVisibleInTree());

    private static GameScreen FindGame()
    {
        GameScreen found = null;
        Walk(_main, n => { if (found == null && n is GameScreen g) found = g; });
        return found;
    }

    private static async Task<bool> ClickButton(Button b)
    {
        if (b == null || !GodotObject.IsInstanceValid(b))
            return false;
        if (!b.IsVisibleInTree() || b.Disabled)
            return false;

        for (Node parent = b.GetParent(); parent != null; parent = parent.GetParent())
            if (parent is ScrollContainer scroll)
            {
                scroll.EnsureControlVisible(b);
                await Frames(2);
            }

        var rect = b.GetGlobalRect();
        var pos = rect.GetCenter();

        var vp = _main.GetViewport().GetVisibleRect();
        if (!vp.Grow(-30).HasPoint(pos))
            return false;

        var viewport = _main.GetViewport();
        viewport.PushInput(new InputEventMouseMotion { Position = pos, GlobalPosition = pos }, true);
        string hovered = viewport.GuiGetHoveredControl()?.GetPath().ToString() ?? "none";

        bool pressed = false;
        void OnPressed() => pressed = true;
        b.Pressed += OnPressed;
        viewport.PushInput(new InputEventMouseButton
        {
            ButtonIndex = MouseButton.Left,
            Pressed = true,
            Position = pos,
            GlobalPosition = pos
        }, true);
        viewport.PushInput(new InputEventMouseButton
        {
            ButtonIndex = MouseButton.Left,
            Pressed = false,
            Position = pos,
            GlobalPosition = pos
        }, true);
        if (GodotObject.IsInstanceValid(b))
            b.Pressed -= OnPressed;

        await Frames(4);
        if (!pressed)
            GD.Print($"  CLICK MISSED: {b.Text}, rect={rect}, hovered={hovered}");
        return pressed;
    }

    private static async Task<bool> Click(string text, int index = 0)
    {
        var list = Buttons(text);
        return index < list.Count && await ClickButton(list[index]);
    }

    private static async Task<bool> Named(string name)
    {
        Button found = null;
        Walk(_main, n => { if (n is Button b && b.Name == name) found = b; });
        return await ClickButton(found);
    }

    private static async Task<bool> RightClick(string name)
    {
        Button found = null;
        Walk(_main, n => { if (n is Button b && b.Name == name) found = b; });
        if (found == null)
            return false;
        var pos = found.GetGlobalRect().GetCenter();
        var viewport = _main.GetViewport();
        viewport.PushInput(new InputEventMouseMotion { Position = pos, GlobalPosition = pos }, true);
        viewport.PushInput(new InputEventMouseButton { ButtonIndex = MouseButton.Right, Pressed = true, Position = pos, GlobalPosition = pos }, true);
        viewport.PushInput(new InputEventMouseButton { ButtonIndex = MouseButton.Right, Pressed = false, Position = pos, GlobalPosition = pos }, true);
        await Frames(4);
        return VisibleButton("Gestionează obiectul");
    }

    private static async Task WalkMenuAndHelp()
    {
        Check(Buttons("Joacă").All(b => FullyInside(b, _main)), "comanda de joc incape in ecran");
        Check(Buttons("Ieșire").All(b => FullyInside(b, _main)), "comenzile principale incap in ecran");
        Check(await Click("Cum se joacă?"), "ajutorul se deschide");
        Check(await Click("← Înapoi"), "ajutorul revine la meniu");
    }

    private static async Task WalkGame(string scId, string scName, string diffId, string diffLabel, bool useRetry)
    {
        _ctx = scId + "/" + diffId;
        Check(await Click("Joacă"), _ctx + ": joc nou");
        Check(await Click(Locale.F("Pornește ({0})", Locale.T(diffLabel))), _ctx + ": dificultate");
        Check(await Click(Locale.F("Joacă: {0}", Locale.T(scName))), _ctx + ": scenariu");
        var session = _main.Session;
        Check(session != null && session.State.Day == 1, _ctx + ": ziua 1");
        var g = FindGame();
        Check(g?.CurrentSection == "home", _ctx + ": numai afacerea este deschisa");
        Check(!VisibleButton("Angajează"), _ctx + ": echipa nu aglomereaza scena");
        await CheckSceneNavigation(scId);
        if (diffId == "easy")
            await CheckMarketScroll();
        _shot("qa_" + scId + "_" + diffId + "_home");

        Check(await Click("Dezvoltare"), _ctx + ": harta dezvoltarii");
        Check(FullyInside(FindControl("section-panel"), FindGame()), _ctx + ": meniul este in interiorul scenei");
        string first = scId == "agro" ? "irrigation" : "workstations";
        string next = scId == "agro" ? "fertilizer" : "automation";
        Check(!await Named("upgrade-" + next), _ctx + ": nodul avansat este blocat");
        Check(await Named("upgrade-" + first), _ctx + ": cumparare primul nod");
        Check(session.State.Upgrades.Contains(first), _ctx + ": efectul a fost activat");
        _shot("qa_" + scId + "_development");
        Check(await Click("Afacerea"), _ctx + ": revenire scena");
        Check(await RightClick("world-slot-0"), _ctx + ": meniu contextual prin clic dreapta");
        Check(FullyInside(FindControl("world-context"), g), _ctx + ": contextul este integral in scena");
        _shot("qa_" + scId + "_context_menu");
        Check(await Click("Gestionează obiectul"), _ctx + ": selectie obiect din context");
        int cash = session.State.Balance;
        string product = scId == "agro" ? "wheat" : "site";
        Check(await Named("plan-" + product), _ctx + ": productie din context");
        Check(session.State.Jobs.Count == 1 && session.State.Jobs[0].Slot == 0 && session.State.Balance < cash,
            _ctx + ": un singur cost si lucrare pe locul ales");
        _shot("qa_" + scId + "_context");
        await RightClick("world-slot-0");
        _main.GetViewport().PushInput(new InputEventKey { Keycode = Key.Escape, Pressed = true }, true);
        _main.GetViewport().PushInput(new InputEventKey { Keycode = Key.Escape, Pressed = false }, true);
        await Frames(4);
        Check(!VisibleButton("Gestionează obiectul") && !VisibleButton("Continuă jocul"),
            _ctx + ": Esc inchide numai contextul activ");
        Check(await Click("Echipă"), _ctx + ": echipa");
        int staff0 = session.State.Staff.Values.Sum();
        Check(await Click("Angajează"), _ctx + ": angajare");
        Check(session.State.Staff.Values.Sum() == staff0 + 1, _ctx + ": personalul a crescut");
        Check(await Click("Concediază"), _ctx + ": concediere");
        Check(session.State.Staff.Values.Sum() == staff0, _ctx + ": personalul a revenit");
        Check(await Click("Finanțe"), _ctx + ": finante");
        Check(!VisibleButton("Angajează"), _ctx + ": o singura sectiune activa");
        Check(await Click("Afacerea"), _ctx + ": inapoi la productie");

        int events = 0, maxJobs = 1;
        for (int turn = 0; turn < 80 && !session.State.Finished; turn++)
        {
            g = FindGame();
            if (g == null)
                break;
            if (g.HasEvent)
            {
                int before = session.State.Balance;
                bool clicked = await Named("event-choice-0") || await Named("event-continue");
                Check(clicked && session.State.PendingEventId == "", _ctx + ": eveniment rezolvat prin UI");
                events++;
                continue;
            }
            if (scId == "agro" && session.State.Goods.Values.Any(v => v > 0))
            {
                await Click("Piață");
                foreach (var p in session.Scenario.Products)
                    if (session.State.GetGood(p.Id) > 0)
                        await Named("sell-" + p.Id);
                await Click("Afacerea");
            }
            if (session.State.Jobs.Count < session.Slots)
            {
                for (int slot = 0; slot < session.Slots; slot++)
                {
                    if (session.State.Jobs.Exists(j => j.Slot == slot))
                        continue;
                    if (session.State.Balance < session.ResourceCost(session.Scenario.Products[0].Resource) + session.DailyUpkeep())
                        break;
                    await Named("world-slot-" + slot);
                    await Named("plan-" + product);
                }
            }
            maxJobs = Math.Max(maxJobs, session.State.Jobs.Count);
            int day = session.State.Day;
            await Click("Încheie ziua");
            if (VisibleButton("Continuă ziua următoare"))
                await Click("Continuă ziua următoare");
            Check(session.State.Day == day + 1, _ctx + ": timpul avanseaza o singura zi");
        }
        await Frames(6);
        Check(session.State.Finished, _ctx + ": partida s-a incheiat");
        Check(session.State.CompletedJobs > 0 && session.State.TotalIncome > 0, _ctx + ": productie si venituri reale");
        Check(events > 0, _ctx + ": evenimente parcurse");
        Check(VisibleButton("Joacă din nou"), _ctx + ": rezultat afisat");
        _shot("qa_" + scId + "_" + diffId + "_end");
        if (useRetry)
        {
            Check(await Click("Joacă din nou"), _ctx + ": reincepere");
            Check(!ReferenceEquals(session, _main.Session) && _main.Session.State.Day == 1 && _main.Session.State.CompletedJobs == 0,
                _ctx + ": sesiune noua curata");
            Check(await Click("Meniu"), _ctx + ": pauza");
            Check(await Click("Continuă jocul"), _ctx + ": pauza se inchide");
            await Click("Meniu");
            Check(await Click("Salvează și mergi la meniu"), _ctx + ": revenire meniu");
            Check(await Click("Continuă partida"), _ctx + ": continua salvarea locala");
            Check(_main.Session.State.Day == 1 && _main.Session.State.CompletedJobs == 0,
                _ctx + ": continuarea pastreaza datele");
            await Click("Meniu");
            await Click("Salvează și mergi la meniu");
        }
        else
            Check(await Click("Meniu principal"), _ctx + ": revenire meniu");
        await Frames(4);
        Check(VisibleButton("Joacă"), _ctx + ": meniul este functional");
    }

    private static Control FindControl(string name)
    {
        Control found = null;
        Walk(_main, n => { if (n is Control c && c.Name == name) found = c; });
        return found;
    }

    private static async Task CheckMarketScroll()
    {
        await Click("Piață");
        var session = _main.Session;
        string id = session.Scenario.Resources.Last().Id;
        var buy = FindControl("buy-resource-" + id) as Button;
        var scroll = FindControl("section-scroll") as ScrollContainer;
        scroll.EnsureControlVisible(buy);
        await Frames(3);
        int position = scroll.ScrollVertical, stock = session.State.GetResource(id);
        var bar = scroll.GetVScrollBar();
        Check(bar.MaxValue <= bar.Page || position > 0, _ctx + ": resursa este vizibila, cu derulare daca e necesara");
        Check(await ClickButton(buy), _ctx + ": cumparare dupa derulare");
        scroll = FindControl("section-scroll") as ScrollContainer;
        Check(session.State.GetResource(id) == stock + 1 && Math.Abs(scroll.ScrollVertical - position) <= 2,
            _ctx + ": cumpararea pastreaza pozitia in lista");
        await Click("Afacerea");
    }

    private static bool FullyInside(Control inner, Control outer) => inner != null && outer != null &&
        outer.GetGlobalRect().Encloses(inner.GetGlobalRect());

    private static async Task CheckSceneNavigation(string scenario)
    {
        var game = FindGame();
        var dock = FindControl("scene-navigation");
        Check(FullyInside(dock, game) && Math.Abs(dock.GetGlobalRect().GetCenter().X - game.GetGlobalRect().GetCenter().X) < 2,
            _ctx + ": bara cu meniuri este centrata in scena");
        string prefix = scenario == "agro" ? "farm-" : "it-";
        foreach (string section in new[] { "home", "market", "development", "team", "finance", "journal" })
        {
            var button = FindControl("nav-" + section) as Button;
            Check(FullyInside(button, dock) && button.Icon != null && button.Icon.ResourcePath.EndsWith(prefix + section + ".svg") &&
                !string.IsNullOrWhiteSpace(button.TooltipText), _ctx + ": icon si explicatie pentru " + section);
        }
        Check(await Click("Arată ghidul"), _ctx + ": ghidul este optional si accesibil");
        Check(FullyInside(FindControl("object-panel"), game), _ctx + ": ghidul ramane in scena");
        _shot("qa_" + scenario + "_guide");
        await Click("Afacerea");
        Check(await Click("Jurnal"), _ctx + ": jurnal din bara scenei");
        Check(FullyInside(FindControl("section-panel"), game), _ctx + ": jurnal centrat si limitat");
        Check(await Named("close-section") && game.CurrentSection == "home", _ctx + ": inchiderea revine la scena");
        await Click("Finanțe");
        _shot("qa_" + scenario + "_finance");
        _main.GetViewport().PushInput(new InputEventKey { Keycode = Key.Escape, Pressed = true }, true);
        _main.GetViewport().PushInput(new InputEventKey { Keycode = Key.Escape, Pressed = false }, true);
        await Frames(4);
        Check(game.CurrentSection == "home", _ctx + ": Esc inchide meniul inainte de pauza");
        await Click("Piață");
        _main.Notification((int)Main.NotificationWMGoBackRequest);
        await Frames(4);
        Check(game.CurrentSection == "home" && !VisibleButton("Continuă jocul"),
            _ctx + ": Inapoi Android inchide numai panoul activ");
        foreach (string section in new[] { "Piață", "Echipă", "Jurnal" })
        {
            Check(await Click(section), _ctx + ": deschide " + section);
            Check(FullyInside(FindControl("section-panel"), game), _ctx + ": panou vizibil pentru " + section);
            _shot("qa_" + scenario + "_menu_" + (section == "Piață" ? "market" : section == "Echipă" ? "team" : "journal"));
        }
        await Click("Afacerea");
        Check(await Named("action-rotate"), _ctx + ": rotirea camerei prin icon");
        await Named("action-zoom-in");
        Check(Enumerable.Range(0, _main.Session.Slots).All(i => FullyInside(FindControl("world-slot-" + i), game)),
            _ctx + ": marcaje vizibile dupa rotire si zoom");
        Check(FullyInside(FindControl("action-center"), game) &&
            !FindControl("action-center").GetGlobalRect().Intersects(dock.GetGlobalRect()), _ctx + ": camera nu acopera meniurile");
        await Named("action-center");
    }
}
