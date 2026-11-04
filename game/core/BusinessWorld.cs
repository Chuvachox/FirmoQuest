using System;
using System.Collections.Generic;
using Godot;

namespace BusinessSim;

// Construieste diorama 3D si transmite selectiile obiectelor catre interfata.

public partial class BusinessWorld : Control
{
    private readonly Session _session;
    private readonly Action<int> _select;
    private readonly Action<string> _navigate;
    private SubViewport _viewport;
    private Camera3D _camera;
    private Node3D _dynamic;
    private Control _contextMenu;
    private readonly List<(Button button, Vector3 position)> _markers = new();
    private readonly Vector3[] _places = {
        new(-2.5f, 0, -1.5f), new(1f, 0, -1.5f), new(-2.5f, 0, 2f), new(1f, 0, 2f)
    };
    private float _angle = 0.76f;
    private float _zoom = 15;
    private bool Agro => _session.Scenario.Id == "agro";

    public BusinessWorld(Session session, Action<int> select = null, Action<string> navigate = null)
    {
        _session = session;
        _select = select;
        _navigate = navigate;
    }

    public override void _Ready()
    {
        if (_select == null)
            MouseFilter = MouseFilterEnum.Ignore;
        ClipContents = true;
        var container = new SubViewportContainer { Stretch = true, MouseFilter = MouseFilterEnum.Ignore };
        Ui.Fill(container);
        AddChild(container);
        _viewport = new SubViewport
        {
            Size = new Vector2I(1100, 650),
            OwnWorld3D = true,
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
            Msaa3D = OS.HasFeature("android") ? Viewport.Msaa.Disabled : Viewport.Msaa.Msaa4X,
        };
        container.AddChild(_viewport);
        var scene = new Node3D();
        _viewport.AddChild(scene);
        var environment = new Godot.Environment
        {
            BackgroundMode = Godot.Environment.BGMode.Color,
            BackgroundColor = Agro ? new Color("233f37") : new Color("25354c"),
            AmbientLightSource = Godot.Environment.AmbientSource.Color,
            AmbientLightColor = new Color("dce9ed"),
            AmbientLightEnergy = 0.38f,
            ReflectedLightSource = Godot.Environment.ReflectionSource.Disabled,
            TonemapMode = Godot.Environment.ToneMapper.Linear,
        };
        scene.AddChild(new WorldEnvironment { Environment = environment });
        scene.AddChild(new DirectionalLight3D
        {
            RotationDegrees = new Vector3(-50, -28, 0),
            LightColor = new Color("fff0d3"),
            LightEnergy = 0.85f,
            ShadowEnabled = !OS.HasFeature("android"),
        });
        _camera = new Camera3D { Projection = Camera3D.ProjectionType.Orthogonal, Size = _zoom, Current = true };
        scene.AddChild(_camera);
        scene.AddChild(Box(new Vector3(11.8f, 0.6f, 10.4f), new Vector3(-0.4f, -0.42f, 0), Agro ? "526e40" : "8b9da5"));
        scene.AddChild(Box(new Vector3(11.4f, 0.1f, 10f), new Vector3(-0.4f, -0.08f, 0), Agro ? "7b9854" : "d4d7c9"));
        if (Agro)
            BuildFarm(scene);
        else
            BuildOffice(scene);
        _dynamic = new Node3D();
        scene.AddChild(_dynamic);

        if (_select != null)
        {
            for (int i = 0; i < 4; i++)
            {
                int index = i;
                AddMarker("", _places[i] + new Vector3(0, 0.15f, 1.15f), () => _select(index), $"world-slot-{i}");
            }
            AddMarker(Agro ? "Piață" : "Contracte", new Vector3(3.65f, 1, -2.7f),
                () => _navigate("market"), "world-market");
            var tools = Ui.Row(5);
            tools.AnchorTop = 1;
            tools.AnchorBottom = 1;
            tools.OffsetLeft = 24;
            tools.OffsetTop = OS.HasFeature("android") ? -220 : -200;
            tools.OffsetBottom = -152;
            if (OS.HasFeature("android"))
            {
                tools.AnchorLeft = tools.AnchorRight = 1;
                tools.AnchorTop = tools.AnchorBottom = 0;
                tools.OffsetLeft = -300;
                tools.OffsetRight = -24;
                tools.OffsetTop = 112;
                tools.OffsetBottom = 176;
            }
            foreach (var (label, icon, action) in new (string, string, Action)[] {
                ("Micșorează", "action-zoom-out", () => { _zoom = Mathf.Min(19, _zoom + 1); }),
                ("Mărește", "action-zoom-in", () => { _zoom = Mathf.Max(12, _zoom - 1); }),
                ("Rotire", "action-rotate", () => { _angle += Mathf.Pi / 2; }),
                ("Centrare", "action-center", () => { _zoom = 15; _angle = 0.76f; }) })
            {
                var b = Ui.Button("", action, ButtonKind.Normal, 16, icon, 24);
                b.Name = icon;
                b.TooltipText = label + " camera";
                b.CustomMinimumSize = OS.HasFeature("android") ? new Vector2(64, 64) : new Vector2(44, 38);
                tools.AddChild(b);
            }
            AddChild(tools);
        }
        GuiInput += e =>
        {
            if (e is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
            {
                Vector2 screen = mb.Position * (Vector2)_viewport.Size / Size;
                Vector3 origin = _camera.ProjectRayOrigin(screen), ray = _camera.ProjectRayNormal(screen);
                Vector3 hit = origin + ray * (-origin.Y / ray.Y);
                for (int i = 0; i < Math.Min(_places.Length, _session.Scenario.Slots + 1); i++)
                    if (Mathf.Abs(hit.X - _places[i].X) < 1.45f && Mathf.Abs(hit.Z - _places[i].Z) < 1.35f)
                    {
                        _select?.Invoke(i);
                        return;
                    }
                _select?.Invoke(-1);
            }
        };
        Refresh();
    }

    private void AddMarker(string text, Vector3 position, Action action, string name)
    {
        var b = Ui.Button(text, action, ButtonKind.Normal, 15,
            (Agro ? "farm-" : "it-") + (name == "world-market" ? "market" : "home"), 24);
        b.Name = name;
        b.CustomMinimumSize = new Vector2(150, OS.HasFeature("android") ? 64 : 40);
        b.TooltipText = "Clic pentru acțiuni. Clic dreapta pentru meniul contextual.";
        b.GuiInput += e =>
        {
            if (e is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Right)
            {
                ContextMenu(action, b.Position + mb.Position);
                AcceptEvent();
            }
        };
        AddChild(b);
        _markers.Add((b, position));
    }

    public bool CloseContextMenu()
    {
        if (_contextMenu == null)
            return false;
        var old = _contextMenu;
        _contextMenu = null;
        old.GetParent().RemoveChild(old);
        old.QueueFree();
        return true;
    }

    private void ContextMenu(Action action, Vector2 at)
    {
        CloseContextMenu();
        var blocker = new Control();
        Ui.Fill(blocker);
        GetParent().AddChild(blocker);
        _contextMenu = blocker;
        blocker.GuiInput += e => { if (e is InputEventMouseButton m && m.Pressed) CloseContextMenu(); };
        var panel = Ui.FloatingPanel(Agro ? ThemeColors.Green : ThemeColors.Accent, 10, 10);
        panel.Name = "world-context";
        panel.CustomMinimumSize = new Vector2(290, 0);
        blocker.AddChild(panel);
        void Place()
        {
            panel.Size = panel.GetCombinedMinimumSize();
            panel.Position = new Vector2(Mathf.Clamp(at.X, 12, Mathf.Max(12, Size.X - panel.Size.X - 12)),
                Mathf.Clamp(at.Y, 118, Mathf.Max(118, Size.Y - panel.Size.Y - 160)));
        }
        panel.Resized += Place;
        blocker.Resized += Place;
        Callable.From(Place).CallDeferred();
        var body = Ui.Column(7);
        panel.AddChild(body);
        body.AddChild(Ui.Label(Agro ? "FERMĂ / ACȚIUNI" : "STUDIO IT / ACȚIUNI", 12, ThemeColors.TextDim));
        string prefix = Agro ? "farm-" : "it-";
        body.AddChild(Ui.Button("Gestionează obiectul", () => { CloseContextMenu(); action(); }, ButtonKind.Primary, 17, prefix + "home", 24));
        body.AddChild(Ui.Button("Harta dezvoltării", () => { CloseContextMenu(); _navigate("development"); }, ButtonKind.Normal, 17, prefix + "development", 24));
        body.AddChild(Ui.Button("Piață și resurse", () => { CloseContextMenu(); _navigate("market"); }, ButtonKind.Normal, 17, prefix + "market", 24));
        body.AddChild(Ui.Label("Esc sau clic în afară: închide", 13, ThemeColors.TextDim));
    }

    public override void _Process(double delta)
    {
        if (_camera == null)
            return;
        _camera.Position = new Vector3(Mathf.Sin(_angle) * 15, 15, Mathf.Cos(_angle) * 15);
        _camera.LookAt(new Vector3(-0.3f, 0, 0));
        _camera.Size = _zoom;
        foreach (var marker in _markers)
        {
            var pos = _camera.UnprojectPosition(marker.position);
            Vector2 scale = Size / (Vector2)_viewport.Size;
            var at = pos * scale - marker.button.Size / 2;
            marker.button.Position = new Vector2(Mathf.Clamp(at.X, 18, Mathf.Max(18, Size.X - marker.button.Size.X - 18)),
                Mathf.Clamp(at.Y, 122, Mathf.Max(122, Size.Y - marker.button.Size.Y - 230)));
        }
    }

    public void Refresh()
    {
        if (_dynamic == null)
            return;
        foreach (var node in _dynamic.GetChildren())
        {
            _dynamic.RemoveChild(node);
            node.QueueFree();
        }
        var st = _session.State;
        for (int i = 0; i < 4; i++)
        {
            if (i >= _session.Scenario.Slots + 1)
            {
                if (i < _markers.Count)
                    _markers[i].button.Visible = false;
                continue;
            }
            bool unlocked = i < _session.Slots;
            Job job = st.Jobs.Find(j => j.Slot == i);
            var p = _places[i];
            _dynamic.AddChild(Box(new Vector3(2.85f, 0.08f, 2.6f), p,
                !unlocked ? "536265" : Agro ? "79604b" : "a5b4b4"));
            if (Agro && unlocked)
            {
                for (int x = 0; x < 4; x++)
                    for (int z = 0; z < 3; z++)
                    {
                        var at = p + new Vector3(-1 + x * 0.64f, 0.08f, -0.8f + z * 0.65f);
                        if (job == null)
                            _dynamic.AddChild(Box(new Vector3(0.35f, 0.04f, 0.5f), at, "69503d"));
                        else
                        {
                            bool grown = job.DaysLeft <= Mathf.CeilToInt(job.TotalDays / 2f);
                            string model = job.ProductId switch
                            {
                                "wheat" => grown ? "crops_wheatStageB" : "crops_wheatStageA",
                                "corn" => grown ? "crops_cornStageD" : "crops_cornStageA",
                                _ => grown ? "crops_leafsStageB" : "crops_leafsStageA"
                            };
                            Model(_dynamic, model, at, 0.65f);
                        }
                    }
            }
            else if (!Agro && unlocked)
            {
                Model(_dynamic, "desk", p + new Vector3(0, 0.08f, 0), 1.5f);
                Model(_dynamic, "chairDesk", p + new Vector3(0, 0.08f, 0.9f), 1.3f, Mathf.Pi);
                Model(_dynamic, st.Upgrades.Contains("workstations") ? "computerScreen" : "laptop",
                    p + new Vector3(0, 1.25f, -0.15f), 1.3f);
                if (job != null)
                {
                    _dynamic.AddChild(Box(new Vector3(0.85f, 0.06f, 0.12f), p + new Vector3(0, 0.14f, 1.1f), "4dd6b1"));
                    Person(_dynamic, p + new Vector3(0, 0.15f, 0.8f), "518baa");
                }
            }
            if (i < _markers.Count)
            {
                var button = _markers[i].button;
                button.Text = !unlocked ? "+ Extindere" : job == null ? $"{(Agro ? "Parcelă" : "Post")} {i + 1}  +" :
                    $"{_session.Scenario.FindProduct(job.ProductId)?.Name} · {job.DaysLeft}z";
                Ui.ApplyButtonStyle(button, job != null ? ButtonKind.Success : ButtonKind.Normal);
            }
        }
        if (st.Upgrades.Contains("irrigation"))
            for (int i = 0; i < 3; i++)
                _dynamic.AddChild(Box(new Vector3(2.6f, 0.08f, 0.08f), _places[i] + new Vector3(0, .17f, -1.1f), "7199c2"));
        if (st.Upgrades.Contains("solar"))
            for (int i = 0; i < 3; i++)
                _dynamic.AddChild(Box(new Vector3(.8f, .09f, 1.2f), new Vector3(3.6f, .4f, i - 0.1f), "315473"));
        if (st.Upgrades.Contains("tractor"))
        {
            _dynamic.AddChild(Box(new Vector3(.9f, .65f, 1.5f), new Vector3(3.6f, .6f, 1.5f), "cf6350"));
            for (int i = 0; i < 4; i++)
                _dynamic.AddChild(Box(new Vector3(.22f, .45f, .45f), new Vector3(3.1f + (i % 2) * 1f, .3f, 1f + (i / 2) * .9f), "303b37"));
        }
        int staff = 0;
        foreach (var value in st.Staff.Values)
            staff += value;
        for (int i = 0; i < Math.Min(staff, 5); i++)
            Person(_dynamic, new Vector3(-4.7f + i * .65f, .05f, 3.7f), "d9a75c");
    }

    private void BuildFarm(Node3D scene)
    {
        scene.AddChild(Box(new Vector3(1.2f, .04f, 9.7f), new Vector3(2.8f, .02f, 0), "c8b48c"));
        scene.AddChild(Box(new Vector3(2, 1.65f, 2), new Vector3(3.6f, .8f, -3.3f), "b15b46"));
        var roof = Box(new Vector3(2.3f, .3f, 2.35f), new Vector3(3.6f, 1.8f, -3.3f), "433f46");
        scene.AddChild(roof);
        scene.AddChild(Box(new Vector3(.65f, .95f, .05f), new Vector3(3.6f, .45f, -2.27f), "e5c790"));
        for (int i = 0; i < 7; i++)
        {
            Model(scene, "fence_simple", new Vector3(-5.5f + i * 1.65f, .03f, -4.8f), .8f);
            if (i < 5)
                Model(scene, i % 2 == 0 ? "tree_oak" : "tree_small", new Vector3(-5.25f, 0, -4 + i * 1.8f), .85f);
        }
        for (int i = 0; i < 5; i++)
            Model(scene, "plant_bush", new Vector3(-3.8f + i * 1.65f, .02f, 4.4f), .7f);
        Model(scene, "bench", new Vector3(4.2f, .05f, 3.2f), 1.2f);
        Model(scene, "cardboardBoxClosed", new Vector3(4.2f, .05f, -1.4f), 1.4f);
    }

    private void BuildOffice(Node3D scene)
    {
        scene.AddChild(Box(new Vector3(11.4f, 2.7f, .18f), new Vector3(-.4f, 1.3f, -4.9f), "96a7a6"));
        scene.AddChild(Box(new Vector3(.18f, 2.7f, 10f), new Vector3(-6, 1.3f, 0), "b8c3b6"));
        for (int i = 0; i < 4; i++)
        {
            scene.AddChild(Box(new Vector3(1.6f, 1.2f, .06f), new Vector3(-4.5f + i * 2.4f, 1.65f, -4.77f), "9ec9d2"));
            scene.AddChild(Box(new Vector3(.06f, 1.2f, .07f), new Vector3(-4.5f + i * 2.4f, 1.65f, -4.72f), "edf2dc"));
        }
        Model(scene, "bookcaseOpen", new Vector3(3.9f, .05f, -3.7f), 1.8f);
        Model(scene, "table", new Vector3(3.9f, .05f, .1f), 1.35f);
        Model(scene, "kitchenCoffeeMachine", new Vector3(3.9f, 1.2f, .1f), 1);
        Model(scene, "pottedPlant", new Vector3(-4.8f, .05f, -3.7f), 1.5f);
        Model(scene, "pottedPlant", new Vector3(4.5f, .05f, 3.8f), 1.5f);
        Model(scene, "bench", new Vector3(3.9f, .05f, 3f), 1.2f);
        Model(scene, "table", new Vector3(1f, .05f, 2f), 1.5f);
        Model(scene, "chairDesk", new Vector3(1f, .05f, 3f), 1.3f, Mathf.Pi);
    }

    private static void Person(Node3D parent, Vector3 at, string color)
    {
        parent.AddChild(Box(new Vector3(.35f, .55f, .3f), at + new Vector3(0, .5f, 0), color));
        parent.AddChild(new MeshInstance3D
        {
            Mesh = new SphereMesh { Radius = .18f, Height = .36f },
            Position = at + new Vector3(0, .95f, 0),
            MaterialOverride = new StandardMaterial3D { AlbedoColor = new Color("e6b687"), Roughness = 1 }
        });
    }

    private static MeshInstance3D Box(Vector3 size, Vector3 at, string hex) => new()
    {
        Mesh = new BoxMesh { Size = size },
        Position = at,
        MaterialOverride = new StandardMaterial3D { AlbedoColor = new Color(hex), Roughness = .85f },
    };

    private static void Model(Node3D parent, string name, Vector3 at, float scale, float angle = 0)
    {
        var packed = GD.Load<PackedScene>($"res://assets/ui/icons/world/{name}.glb");
        if (packed == null)
        {
            GD.PushError("Model lipsă: " + name);
            return;
        }
        var model = packed.Instantiate<Node3D>();
        parent.AddChild(model);
        model.Position = at;
        model.Scale = Vector3.One * scale;
        model.Rotation = new Vector3(0, angle, 0);
    }
}
