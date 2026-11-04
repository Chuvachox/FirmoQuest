using Godot;
using System.Threading.Tasks;

namespace BusinessSim;

// Porneste aplicatia, incarca datele si gestioneaza ciclul de viata al jocului.

public partial class Main : Control
{
	private Content _content;
	private Control _host;

	private string _difficultyId = "medium";
	private Session _session;
	private bool _qaMode;
	private UpdateChecker _updates;
	internal string QaSavePath;

	private GameScreen _gameScreen;
	private int _galleryStep;
	private int _galleryFrame;
	private int _galleryTurns;
	private bool _galleryShot;
	private string _galleryScenario = "agro";

	public override void _Ready()
	{
		_content = Content.Load();
		GetWindow().Title = _content.Config.Title;
		FitDesktopWindow();

		_host = new Control { Name = "Screens" };
		Ui.Fill(_host);
		AddChild(_host);
		GetViewport().SizeChanged += ApplySafeArea;
		ApplySafeArea();
		GetTree().AutoAcceptQuit = false;
		GetTree().QuitOnGoBack = false;
		GetWindow().CloseRequested += QuitGame;

		foreach (var a in OS.GetCmdlineUserArgs())
		{
			if (a == "--balance-audit")
			{
				GetTree().Quit(BalanceAudit.Run(_content));
				return;
			}
			if (a == "--balance-audit-holdout")
			{
				GetTree().Quit(BalanceAudit.Run(_content, 40));
				return;
			}
			if (a == "--hard-audit")
			{
				GetTree().Quit(BalanceAudit.CompareHard(_content));
				return;
			}
			if (a == "--deadline-check")
			{
				GetTree().Quit(BalanceAudit.CheckDeadlines(_content));
				return;
			}
			if (a == "--update-check")
			{
				UpdateTest.Probe(this);
				return;
			}
			if (a == "--selftest")
			{
				GetTree().Quit(SelfTest.Run(_content));
				return;
			}
			if (a == "--gallery" || a == "--gallery-it")
			{
				_galleryScenario = a == "--gallery-it" ? "it" : "agro";
				StartGallery();
				return;
			}
			if (a == "--qatest")
			{
				_qaMode = true;
				_updates = new UpdateChecker { Name = "update-checker" };
				AddChild(_updates);
				ShowMenu();
				QaUiTest.Run(this, Shot);
				return;
			}
		}

		_updates = new UpdateChecker { Name = "update-checker" };
		AddChild(_updates);
		_updates.Check();
		ShowMenu();
	}

	public override void _Notification(int what)
	{
		if (_host == null || _shuttingDown)
			return;
		if (what == NotificationApplicationPaused)
			Persist();
		if (what == NotificationWMGoBackRequest)
		{
			if (_gameScreen != null && GodotObject.IsInstanceValid(_gameScreen) && _gameScreen.IsInsideTree())
				_gameScreen.BackToScene();
			else
				ShowMenu();
		}
	}

	private bool _shuttingDown;
	internal async Task Shutdown(int exitCode)
	{
		if (_shuttingDown)
			return;
		_shuttingDown = true;
		_gameScreen = null;
		_session = null;
		foreach (var child in _host.GetChildren())
		{
			_host.RemoveChild(child);
			child.QueueFree();
		}
		for (int i = 0; i < 3; i++)
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		Ui.ReleaseIcons();
		System.GC.Collect();
		System.GC.WaitForPendingFinalizers();
		GetTree().Quit(exitCode);
	}

}
