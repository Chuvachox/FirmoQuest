using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BusinessSim;

// Schimba ecranele si leaga salvarea de actiunile utilizatorului.
public partial class Main
{
    private void Show(Control screen)
    {
        for (int i = _host.GetChildCount() - 1; i >= 0; i--)
        {
            var child = _host.GetChild(i);
            _host.RemoveChild(child);
            child.QueueFree();
        }
        _host.AddChild(screen);

        if (_galleryStep > 0)
        {
            GD.Print($"  [{screen.GetType().Name}] host={_host.Size} ecran={screen.Size} " +
                     $"ancore={screen.AnchorLeft},{screen.AnchorRight} offset={screen.OffsetLeft},{screen.OffsetRight}");
        }
    }

    private void ShowMenu()
    {
        var saved = (_qaMode && QaSavePath == null) || _galleryStep > 0 ? null : SaveGame.Read(_content, out _, QaSavePath);
        Show(new MainMenu(_content, ShowDifficulty, ShowHelp, QuitGame,
            saved != null && !saved.State.Finished ? () => Resume(saved) : null, _updates));
    }

    private bool Persist()
    {
        if ((_qaMode && QaSavePath == null) || _galleryStep > 0 || _session == null)
            return true;
        if (SaveGame.Write(_session, out var error, QaSavePath))
            return true;
        GD.PushWarning(error);
        if (_gameScreen != null && IsInstanceValid(_gameScreen))
            _gameScreen.ReportSaveFailure();
        return false;
    }

    private void Resume(Session session)
    {
        _session = session;
        _difficultyId = session.State.DifficultyId;
        var screen = new GameScreen(_session, ShowMenu, ShowEnd, Persist);
        _gameScreen = screen;
        Show(screen);
    }

    private void ShowHelp() =>
        Show(new HelpScreen(_content, ShowMenu));

    private void ShowDifficulty() =>
        Show(new DifficultyScreen(_content, id =>
        {
            _difficultyId = id;
            ShowScenario();
        }, ShowMenu));

    private void ShowScenario() =>
        Show(new ScenarioScreen(_content, _difficultyId, sc => StartGame(sc.Id),
            ShowDifficulty));

    public Session Session => _session;

    private void StartGame(string scenarioId)
    {
        _session = Session.NewGame(_content, scenarioId, _difficultyId, _qaMode ? 20261028 : null);

        var screen = new GameScreen(_session, ShowMenu, ShowEnd, Persist);
        _gameScreen = screen;
        Show(screen);
    }

    private void ShowEnd()
    {
        var session = _session;
        Show(new EndScreen(
            session,
            onRetry: () => StartGame(session.State.ScenarioId),
            onMenu: ShowMenu));
    }

    private void QuitGame()
    {
        if (Persist())
            _ = Shutdown(0);
    }
}
