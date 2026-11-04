using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BusinessSim;

// Deschide sectiunile si restaureaza derularea dupa actualizarea continutului.
public partial class GameScreen
{
    public override void _UnhandledKeyInput(InputEvent e)
    {
        if (e is not InputEventKey key || !key.Pressed || key.Echo)
            return;
        if (key.Keycode == Key.Escape)
        {
            BackToScene();
            GetViewport().SetInputAsHandled();
        }
        if (_activeOverlay == null && key.Keycode >= Key.Key1 && key.Keycode <= Key.Key6)
        {
            Navigate(_sectionTitles.Keys.ElementAt((int)(key.Keycode - Key.Key1)));
            GetViewport().SetInputAsHandled();
        }
    }

    public void Navigate(string section)
    {
        if (!_sectionTitles.ContainsKey(section) || HasEvent || _activeOverlay != null)
            return;
        _businessWorld.CloseContextMenu();
        _sectionScroll = null;
        _detailsScroll = null;
        _activeSection = section;
        _selectedSlot = -1;
        _guideOpen = false;
        BuildSection();
        foreach (var kv in _navigationButtons)
            Ui.ApplyButtonStyle(kv.Value, kv.Key == _activeSection ? (_session.Scenario.Id == "agro" ? ButtonKind.Success : ButtonKind.Primary) : ButtonKind.Normal);
    }
    private void BuildSection()
    {
        int previousScroll = _sectionScroll?.ScrollVertical ?? 0;
        _sectionScroll = null;
        _detailsScroll = null;
        _productionDetails = null;
        Clear(_sectionHost);
        if (_activeSection == "home")
        {
            if (_selectedSlot < 0 && !_guideOpen)
            {
                var tip = Ui.FloatingPanel(Accent, 16, 14);
                tip.Name = "scene-tip";
                tip.Position = new Vector2(24, 126);
                tip.Size = new Vector2(300, 0);
                _sectionHost.AddChild(tip);
                var col = Ui.Column(9);
                tip.AddChild(col);
                bool sell = _session.State.Goods.Values.Any(v => v > 0), busy = _session.State.Jobs.Count >= _session.Slots;
                var title = Ui.Row(9);
                title.AddChild(Ui.Icon(sell ? NavIcon("market") : busy ? "action-clock" : NavIcon("home"), 30));
                title.AddChild(Ui.Label(sell ? "Recolta este gata" : busy ? "Lucrări în curs" :
                    _session.Scenario.Id == "agro" ? "Alege o parcelă" : "Alege un post", 20));
                col.AddChild(title);
                col.AddChild(Ui.Caption(sell ? "Ai marfă în depozit. Vinde pe piață pentru a încasa venitul." : busy ?
                    "Încheie ziua pentru a avansa lucrările. Timpul așteaptă alegerea ta." :
                    _session.Scenario.Id == "agro" ? "Clic pe teren pentru a planta. Recolta se vinde pe piață." :
                    "Clic pe un birou pentru un contract. Plata vine la livrare.", 16));
                if (sell)
                    col.AddChild(Ui.Button("Vinde recolta", () => Navigate("market"), ButtonKind.Success, 16, NavIcon("market"), 22));
                col.AddChild(Ui.Button("Arată ghidul", () => { _guideOpen = true; BuildSection(); }, ButtonKind.Ghost, 15, "action-help", 20));
                return;
            }
            var side = Ui.FloatingPanel(Accent, 20, 18);
            side.Name = "object-panel";
            side.AnchorLeft = 1;
            side.AnchorRight = 1;
            side.AnchorBottom = 1;
            side.OffsetLeft = -420;
            side.OffsetRight = -24;
            side.OffsetTop = 126;
            side.OffsetBottom = -164;
            _sectionHost.AddChild(side);
            var scroll = Ui.Scroll();
            side.AddChild(scroll);
            _detailsScroll = scroll;
            _productionDetails = Ui.Column(12);
            _productionDetails.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            scroll.AddChild(_productionDetails);
            RefreshDetails();
            return;
        }
        var shade = new ColorRect { Name = "section-backdrop", Color = new Color(.015f, .025f, .04f, .38f) };
        Ui.Fill(shade);
        _sectionHost.AddChild(shade);
        shade.GuiInput += e => { if (e is InputEventMouseButton m && m.Pressed && m.ButtonIndex == MouseButton.Left) Navigate("home"); };
        var panel = Ui.FloatingPanel(Accent);
        panel.Name = "section-panel";
        panel.AnchorLeft = .14f;
        panel.AnchorRight = .86f;
        panel.AnchorTop = .15f;
        panel.AnchorBottom = 1;
        panel.OffsetBottom = -168;
        _sectionHost.AddChild(panel);
        var frame = Ui.Column(12);
        panel.AddChild(frame);
        var heading = Ui.Row(14);
        heading.AddChild(Ui.Icon(NavIcon(_activeSection), 44));
        var labels = Ui.Column(2);
        labels.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        labels.AddChild(Ui.Label(Locale.T(_session.Scenario.Id == "agro" ? "FERMĂ  /  ADMINISTRARE" : "STUDIO IT  /  ADMINISTRARE"), 12, Accent));
        labels.AddChild(Ui.Label(_sectionTitles[_activeSection], 28));
        heading.AddChild(labels);
        var close = Ui.Button("", () => Navigate("home"), ButtonKind.Ghost, 16, "action-close", 24);
        close.Name = "close-section";
        close.CustomMinimumSize = new Vector2(44, 44);
        close.TooltipText = Locale.T("Inchide si revino la scena (Esc).");
        heading.AddChild(close);
        frame.AddChild(heading);
        frame.AddChild(new HSeparator());
        var scrollBody = Ui.Scroll();
        scrollBody.Name = "section-scroll";
        _sectionScroll = scrollBody;
        scrollBody.SizeFlagsVertical = SizeFlags.ExpandFill;
        frame.AddChild(scrollBody);
        var body = Ui.Column(14);
        body.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        scrollBody.AddChild(body);
        switch (_activeSection)
        {
            case "market":
                BuildMarket(body);
                break;
            case "development":
                BuildDevelopment(body);
                break;
            case "team":
                BuildTeam(body);
                break;
            case "finance":
                BuildFinance(body);
                break;
            case "journal":
                BuildJournal(body);
                break;
        }
        RestoreScroll(scrollBody, previousScroll);
        frame.AddChild(Ui.Caption(OS.HasFeature("android") ? Ui.TouchScrollHint :
            "Derulează pentru detalii  ·  Esc: închide  ·  Bara de jos schimbă meniul", 13));
    }

    private async void RestoreScroll(ScrollContainer scroll, int position)
    {
        int epoch = ++_scrollRestoreVersion;
        var tree = GetTree();
        await ToSignal(tree, SceneTree.SignalName.ProcessFrame);
        await ToSignal(tree, SceneTree.SignalName.ProcessFrame);
        if (epoch == _scrollRestoreVersion && GodotObject.IsInstanceValid(this) && IsInsideTree() && GodotObject.IsInstanceValid(scroll) && scroll.IsInsideTree())
            scroll.ScrollVertical = position;
    }
}
