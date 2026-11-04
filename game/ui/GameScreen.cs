using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BusinessSim;

// Coordoneaza sesiunea activa, actualizarea ecranului si salvarea progresului.
public partial class GameScreen : Control
{
    private readonly Session _session;
    private readonly Action _returnToMenu, _showResults;
    private readonly Func<bool> _saveProgress;
    private Control _sectionHost, _activeOverlay;
    private VBoxContainer _productionDetails;
    private ScrollContainer _sectionScroll, _detailsScroll;
    private int _scrollRestoreVersion;
    private Label _dayLabel, _balanceLabel, _profitLabel, _statusLabel;
    private ProgressBar _profitProgress;
    private BusinessWorld _businessWorld;
    private string _activeSection = "home";
    private int _selectedSlot = -1, _batchCount = 1;
    private bool _refreshQueued, _finishReported, _guideOpen;
    private Color Accent => _session.Scenario.Id == "agro" ? new Color("9ec76c") : new Color("65bdf2");
    private string NavIcon(string key) => (_session.Scenario.Id == "agro" ? "farm-" : "it-") + key;
    private static string SectionHint(string key) => Locale.T(key switch
    {
        "home" => "Alege un obiect din scena pentru productie.",
        "market" => "Vinde recolta sau verifica ofertele si resursele.",
        "development" => "Urmareste ramurile si deblocheaza imbunatatiri.",
        "team" => "Angajeaza oameni si compara bonusul cu salariul.",
        "finance" => "Vezi banii, costurile, datoria si profitul.",
        _ => "Urmareste obiectivele si ultimele actiuni."
    });
    private readonly Dictionary<string, Button> _navigationButtons = new();
    private readonly Dictionary<string, string> _sectionTitles = new()
    {
        ["home"] = "Afacerea",
        ["market"] = "Piață",
        ["development"] = "Dezvoltare",
        ["team"] = "Echipă",
        ["finance"] = "Finanțe",
        ["journal"] = "Jurnal"
    };
    public string CurrentSection => _activeSection;
    public bool HasEvent => _session.PendingEvent != null;
    public void BackToScene()
    {
        if (HasEvent)
            return;
        if (_activeOverlay != null)
            CloseOverlay();
        else if (_businessWorld.CloseContextMenu())
        {
        }
        else if (_activeSection != "home")
            Navigate("home");
        else if (_selectedSlot >= 0 || _guideOpen)
        {
            _guideOpen = false;
            SelectSlot(-1);
        }
        else
            PauseMenu();
    }

    public GameScreen(Session session, Action onMenu, Action onFinish, Func<bool> save = null)
    {
        _session = session;
        _returnToMenu = onMenu;
        _showResults = onFinish;
        _saveProgress = save;
    }

    public override void _ExitTree()
    {
        _session.Changed -= RequestRefresh;
        Locale.Changed -= OnLocaleChanged;
    }

    private void OnLocaleChanged() => Callable.From(Rebuild).CallDeferred();

    private void Rebuild()
    {
        if (!GodotObject.IsInstanceValid(this) || !IsInsideTree())
            return;
        int sectionPosition = _sectionScroll?.ScrollVertical ?? 0;
        int detailsPosition = _detailsScroll?.ScrollVertical ?? 0;
        _sectionScroll = null;
        _detailsScroll = null;
        _scrollRestoreVersion++;
        bool resumePause = _activeOverlay?.Name == "pause-overlay";
        _session.Changed -= RequestRefresh; // _Ready abonează din nou
        Locale.Changed -= OnLocaleChanged;
        foreach (var ch in GetChildren())
        {
            RemoveChild(ch);
            ch.QueueFree();
        }
        _activeOverlay = null;
        _productionDetails = null;
        _businessWorld = null;
        _refreshQueued = false;
        _Ready();
        if (_sectionScroll != null)
            RestoreScroll(_sectionScroll, sectionPosition);
        if (_detailsScroll != null)
            RestoreScroll(_detailsScroll, detailsPosition);
        if (resumePause)
            Callable.From(PauseMenu).CallDeferred();
    }

    private void RequestRefresh()
    {
        if (_refreshQueued)
            return;
        _refreshQueued = true;
        Callable.From(() => { _refreshQueued = false; if (IsInsideTree()) Refresh(); }).CallDeferred();
    }
    private static void Clear(Node node)
    {
        foreach (var ch in node.GetChildren())
        {
            node.RemoveChild(ch);
            ch.QueueFree();
        }
    }
    private void Refresh()
    {
        if (!IsInsideTree())
            return;
        var st = _session.State;
        _dayLabel.Text = Locale.F("Ziua {0} / {1} · {2}",
            Math.Min(st.Day, _session.DayLimit), _session.DayLimit, Locale.T(_session.Difficulty.Label));
        _balanceLabel.Text = Ui.Money(st.Balance, _session.Currency);
        _balanceLabel.AddThemeColorOverride("font_color", st.Balance < _session.DailyUpkeep() * 2 ? ThemeColors.Gold : ThemeColors.Text);
        _balanceLabel.TooltipText = Locale.F("Cost zilnic {0:N0} + rată credit {1:N0} {2}",
            _session.DailyUpkeep(), Math.Min(st.LoanLeft, st.LoanInstallment), _session.Currency);
        _profitLabel.Text = Locale.F("Profit {0:N0} / {1:N0} {2}",
            st.Profit, _session.Config.TargetProfit, _session.Currency);
        _profitProgress.MaxValue = _session.Config.TargetProfit;
        _profitProgress.Value = Math.Max(0, st.Profit);
        foreach (var kv in _navigationButtons)
            Ui.ApplyButtonStyle(kv.Value, kv.Key == _activeSection ? (_session.Scenario.Id == "agro" ? ButtonKind.Success : ButtonKind.Primary) : ButtonKind.Normal);
        _businessWorld?.Refresh();
        TranslateWorldTexts(_businessWorld);
        if (_activeSection == "home" && _productionDetails != null)
            RefreshDetails();
        else
            BuildSection();
        if (!string.IsNullOrEmpty(_session.Notice))
            Message(_session.Notice, true);
        SaveProgress();
        if (st.Finished && !_finishReported)
        {
            _finishReported = true;
            Callable.From(_showResults).CallDeferred();
        }
    }
    private static void TranslateWorldTexts(Node node)
    {
        if (node == null)
            return;
        foreach (var child in node.GetChildren())
        {
            if (child is Button b)
            {
                b.Text = Locale.SceneText(b.Text);
                b.TooltipText = Locale.T(b.TooltipText);
            }
            TranslateWorldTexts(child);
        }
    }

    private void Message(string text, bool warning = false)
    {
        text = Locale.Notice(text);
        _statusLabel.Text = text;
        _statusLabel.TooltipText = text;
        _statusLabel.AddThemeColorOverride("font_color", warning ? ThemeColors.Gold : ThemeColors.TextDim);
    }
    private bool SaveProgress()
    {
        if (_saveProgress == null || _saveProgress())
            return true;
        Message("Salvarea nu a reușit. Eliberează spațiu și încearcă din nou înainte de a ieși.", true);
        return false;
    }
    public void ReportSaveFailure() => Message("Salvarea nu a reușit. Jocul rămâne deschis pentru a putea încerca din nou.", true);
}
