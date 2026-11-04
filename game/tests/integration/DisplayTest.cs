using System;
using Godot;

namespace BusinessSim;

// Verifica dimensiunile ferestrei si zona sigura a continutului.

public static class DisplayTest
{
    public static void Run(Action<bool, string> check)
    {
        var visible = new Rect2(0, 0, 1600, 720);
        var wide = new Transform2D(new Vector2(1.5f, 0), new Vector2(0, 1.5f), Vector2.Zero);
        var safe = Main.SafeContentRect(visible, wide, new Rect2I(90, 0, 2310, 1056));
        check(safe.IsEqualApprox(new Rect2(60, 0, 1540, 704)), "ecran 2400x1080: decupajul si bara sistemului sunt excluse");
        var letterboxed = new Transform2D(new Vector2(1.5f, 0), new Vector2(0, 1.5f), new Vector2(240, 0));
        safe = Main.SafeContentRect(new Rect2(0, 0, 1280, 720), letterboxed, new Rect2I(90, 0, 2310, 1056));
        check(safe.IsEqualApprox(new Rect2(0, 0, 1280, 704)), "benzile laterale nu devin margini UI in plus");
        check(Main.SafeContentRect(visible, wide, new Rect2I()).IsEqualApprox(visible), "arie sigura necunoscuta: foloseste ecranul vizibil");
    }
}
