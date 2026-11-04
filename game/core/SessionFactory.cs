using System;
using System.Collections.Generic;

namespace BusinessSim;

// Creeaza sesiunea si initializeaza starea dupa scenariu si dificultate.

public partial class Session
{
    public Content Content;
    public ScenarioDef Scenario;
    public DifficultyDef Difficulty;
    public GameState State;

    private readonly Random _rng;

    public Session() : this(null) { }
    private Session(int? randomSeed) => _rng = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();

    public event Action Changed;

    public string Notice { get; private set; } = "";

    public GameConfig Config => Content.Config;
    public int DayLimit => State.DayLimit;
    public string Currency => Config.Currency;

    public static Session NewGame(Content content, string scenarioId, string difficultyId, int? randomSeed = null)
    {
        var session = new Session(randomSeed)
        {
            Content = content,
            Scenario = content.GetScenario(scenarioId),
            Difficulty = content.Config.FindDifficulty(difficultyId),
        };

        var difficulty = session.Difficulty;
        session.State = new GameState
        {
            ScenarioId = scenarioId,
            DifficultyId = difficultyId,
            DayLimit = content.Config.DaysFor(difficultyId),
            StartBalance = difficulty.StartBalance,
            Grant = difficulty.Grant,
            Balance = difficulty.StartBalance + difficulty.Grant + difficulty.Loan,
            LoanLeft = difficulty.Loan,
            LoanInstallment = difficulty.LoanInstallment,
        };

        session.Log($"Ai început afacerea „{session.Scenario.Name}” pe dificultatea {difficulty.Label}.");
        if (difficulty.Grant > 0)
            session.Log($"Ai primit un grant de {difficulty.Grant} {session.Currency}.");
        if (difficulty.Loan > 0)
            session.Log($"Ai luat un credit de {difficulty.Loan} {session.Currency}, cu rambursare de {difficulty.LoanInstallment} {session.Currency}/zi.");
        session.Log($"Ținta: {content.Config.TargetProfit} {session.Currency} profit în {session.DayLimit} zile.");
        session.Changed?.Invoke();
        return session;
    }

}
