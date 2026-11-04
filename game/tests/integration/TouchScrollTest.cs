using System;
using System.Threading.Tasks;
using Godot;

namespace BusinessSim;

// Verifica glisarea listelor si prevenirea apasarilor accidentale.

public static class TouchScrollTest
{
    public static async Task Run(Main main, Action<bool, string> check)
    {
        bool emulation = Input.EmulateTouchFromMouse;
        Input.EmulateTouchFromMouse = true;
        var surface = new Control { Name = "touch-regression" };
        Ui.Fill(surface);
        main.AddChild(surface);
        var scroll = Ui.Scroll();
        scroll.Position = new Vector2(80, 80);
        scroll.Size = new Vector2(420, 220);
        surface.AddChild(scroll);
        var content = Ui.Column(12);
        content.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        scroll.AddChild(content);
        var session = Session.NewGame(Content.Load(), "agro", "easy", 71);
        var card = Ui.Card("Resurse", ThemeColors.Green);
        content.AddChild(card.Item1);
        string resource = session.Scenario.Resources[0].Id;
        var buy = Ui.Button("Cumpără", () => session.BuyResource(resource, 1));
        buy.CustomMinimumSize = new Vector2(0, 84);
        card.Item2.AddChild(buy);
        for (int i = 0; i < 8; i++)
            content.AddChild(Ui.Card("Card " + i, ThemeColors.Accent).Item1);

        async Task Frames(int count)
        {
            for (int i = 0; i < count; i++)
                await main.ToSignal(main.GetTree(), SceneTree.SignalName.ProcessFrame);
        }
        void Move(Vector2 p, Vector2 relative, bool held) => main.GetViewport().PushInput(new InputEventMouseMotion
        {
            Position = p,
            GlobalPosition = p,
            Relative = relative,
            ButtonMask = held ? MouseButtonMask.Left : 0
        }, true);
        void Press(Vector2 p, bool pressed) => main.GetViewport().PushInput(new InputEventMouseButton
        {
            Position = p,
            GlobalPosition = p,
            ButtonIndex = MouseButton.Left,
            Pressed = pressed
        }, true);
        async Task Swipe(Vector2 start, float distance)
        {
            Move(start, Vector2.Zero, false);
            Press(start, true);
            var position = start;
            for (int i = 0; i < 8; i++)
            {
                var next = start + new Vector2(0, distance * (i + 1) / 8);
                Move(next, next - position, true);
                position = next;
                await Frames(1);
            }
            Press(position, false);
            await Frames(2);
            var edge = scroll.GetGlobalRect().Position + new Vector2(2, 2);
            Move(edge, Vector2.Zero, false);
            Press(edge, true);
            Press(edge, false);
            await Frames(2);
        }

        try
        {
            await Frames(5);
            int balance = session.State.Balance;
            await Swipe(buy.GetGlobalRect().GetCenter(), -28);
            check(scroll.ScrollVertical > 0 && session.State.Balance == balance,
                "touch: glisarea pornita pe cumparare deruleaza fara tranzactie");
            scroll.ScrollVertical = 0;
            await Frames(3);
            var tap = buy.GetGlobalRect().GetCenter();
            Move(tap, Vector2.Zero, false);
            Press(tap, true);
            Press(tap, false);
            await Frames(3);
            check(session.State.Balance == balance - session.ResourceCost(resource) && session.State.GetResource(resource) == 1,
                "touch: atingerea stationara cumpara o singura data");
            int before = scroll.ScrollVertical;
            await Swipe(scroll.GetGlobalRect().Position + new Vector2(100, 190), -120);
            int down = scroll.ScrollVertical;
            check(down > before + 40, "touch: glisare in sus peste carduri");
            await Swipe(scroll.GetGlobalRect().Position + new Vector2(100, 50), 90);
            check(scroll.ScrollVertical < down, "touch: glisare in jos peste carduri");
            var point = scroll.GetGlobalRect().GetCenter();
            Move(point, Vector2.Zero, false);
            main.GetViewport().PushInput(new InputEventMouseButton
            {
                Position = point,
                GlobalPosition = point,
                ButtonIndex = MouseButton.WheelDown,
                Pressed = true
            }, true);
            await Frames(2);
            check(scroll.ScrollVertical > 0, "mouse: rotita ramane functionala");
        }
        finally
        {
            Input.EmulateTouchFromMouse = emulation;
            main.RemoveChild(surface);
            surface.QueueFree();
            await Frames(2);
        }
    }
}
