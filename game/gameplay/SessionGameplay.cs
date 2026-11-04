using System;
using System.Collections.Generic;

namespace BusinessSim;

// Avanseaza zilele, finalizeaza lucrarile si decide rezultatul campaniei.

public partial class Session
{
    public EventDef EndDay()
    {
        if (State.Finished)
            return null;
        if (PendingEvent != null)
            return PendingEvent;

        var finished = new List<Job>();
        foreach (var j in State.Jobs)
        {
            j.DaysLeft--;
            if (j.DaysLeft <= 0)
                finished.Add(j);
        }
        foreach (var j in finished)
        {
            State.Jobs.Remove(j);
            Complete(j);
        }

        int upkeep = DailyUpkeep();
        if (upkeep > 0)
            Spend(upkeep, Scenario.UpkeepLabel);

        if (State.LoanLeft > 0 && State.LoanInstallment > 0)
        {
            RepayLoan();
        }

        foreach (var m in State.Mods)
            m.DaysLeft--;
        State.Mods.RemoveAll(m => m.DaysLeft <= 0);

        State.ProfitHistory.Add(State.Profit);

        State.Day++;
        Notice = "";

        if (CheckEnd())
        {
            Changed?.Invoke();
            return null;
        }

        var ev = RollEvent();
        State.PendingEventId = ev?.Id ?? "";
        Changed?.Invoke();
        return ev;
    }

    private bool CheckEnd()
    {
        if (State.Balance < 0)
        {
            End(false, "Faliment", "Soldul a devenit negativ. Bancherii ți-au luat cheile de la afacere.");
            return true;
        }

        if (State.Profit >= Config.TargetProfit)
        {
            End(true, "Ținta atinsă!",
                $"Ai obținut {State.Profit} {Currency} profit în {State.Day - 1} zile. Felicitări!");
            return true;
        }

        if (State.Day > DayLimit)
        {
            End(false, "Timpul a expirat",
                $"Ai încheiat cu {State.Profit} {Currency} profit, iar ținta era de {Config.TargetProfit} {Currency}.");
            return true;
        }

        return false;
    }

    private void End(bool won, string title, string text)
    {
        State.Finished = true;
        State.Won = won;
        State.EndTitle = title;
        State.EndText = text;
        Log(title);
    }

}
