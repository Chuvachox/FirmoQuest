using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;

namespace BusinessSim;

// Pastreaza limba aleasa si traduce textele simple, mesajele si jurnalul.

public static partial class Locale
{
    public const string Ro = "ro";
    public const string Ru = "ru";
    public const string En = "en";

    public static readonly string[] Codes = { Ro, Ru, En };

    public static string Lang { get; private set; } = Ro;

    public static event Action Changed;

    private static bool _loaded;
    private static readonly Dictionary<string, (string Ru, string En)> Table = new();

    private static string LangPath => "user://language.txt";

    private static void EnsureLoaded()
    {
        if (_loaded)
            return;
        _loaded = true;
        try
        {
            var f = FileAccess.Open(LangPath, FileAccess.ModeFlags.Read);
            if (f == null)
                return; // prima rulare: rămâne româna
            var code = f.GetAsText().Trim();
            f.Dispose();
            if (Array.IndexOf(Codes, code) >= 0)
                Lang = code;
        }
        catch (Exception)
        {
            Lang = Ro; // fișier lipsă sau ilezibil: româna
        }
    }

    public static string T(string ro)
    {
        if (string.IsNullOrEmpty(ro))
            return ro;
        EnsureLoaded();
        if (Lang == Ro)
            return ro;
        if (!Table.TryGetValue(ro, out var pair))
            return ro;
        return Lang == Ru ? pair.Ru : pair.En;
    }

    public static string F(string ro, params object[] args) => string.Format(T(ro), args);

    public static void Set(string code)
    {
        EnsureLoaded();
        if (code == Lang || Array.IndexOf(Codes, code) < 0)
            return;
        Lang = code;
        try
        {
            var f = FileAccess.Open(LangPath, FileAccess.ModeFlags.Write);
            if (f != null)
            {
                f.StoreString(code);
                f.Dispose();
            }
        }
        catch (Exception)
        {
        }
        Changed?.Invoke();
    }

    public static string Name(string code) => code switch
    {
        Ru => "Русский",
        En => "English",
        _ => "Română",
    };

    private static void Add(string ro, string ru, string en) => Table[ro] = (ru, en);

    public static string Notice(string msg)
    {
        if (string.IsNullOrEmpty(msg))
            return msg;
        EnsureLoaded();
        if (Lang == Ro)
            return msg;
        string direct = T(msg);
        if (direct != msg)
            return direct;
        var m = Regex.Match(msg, @"^Maximum (\d+) loturi pe loc\.$");
        if (m.Success)
            return F("Maximum {0} loturi pe loc.", m.Groups[1].Value);
        m = Regex.Match(msg, @"^Nu mai ai (.+) libere \((\d+)\/(\d+)\)\.$");
        if (m.Success)
            return F("Nu mai ai {0} libere ({1}/{2}).",
            T(m.Groups[1].Value), m.Groups[2].Value, m.Groups[3].Value);
        m = Regex.Match(msg, @"^Nu ai suficiente resurse pentru (.+)\.$");
        if (m.Success)
            return F("Nu ai suficiente resurse pentru {0}.", T(m.Groups[1].Value));
        return msg;
    }

    public static string Log(string entry)
    {
        if (string.IsNullOrEmpty(entry))
            return entry;
        EnsureLoaded();
        if (Lang == Ro)
            return entry;
        var day = Regex.Match(entry, @"^\[Ziua (\d+)\] (.*)$");
        string body = LogBody(day.Success ? day.Groups[2].Value : entry);
        return day.Success ? F("[Ziua {0}]", day.Groups[1].Value) + " " + body : body;
    }

    private static string LogBody(string s)
    {
        string direct = T(s);
        if (direct != s)
            return direct; // titluri de final și alte texte statice
        var m = Regex.Match(s, @"^([+-])(\d+) (\S+) - (.+)$");
        if (m.Success)
            return m.Groups[1].Value + m.Groups[2].Value + " " + m.Groups[3].Value + " - " +
                Reason(m.Groups[4].Value);
        m = Regex.Match(s, @"^Ai început afacerea „(.+)” pe dificultatea (.+)\.$");
        if (m.Success)
            return F("Ai început afacerea „{0}” pe dificultatea {1}.",
            T(m.Groups[1].Value), T(m.Groups[2].Value));
        m = Regex.Match(s, @"^Ai primit un grant de (\d+) (\S+)\.$");
        if (m.Success)
            return F("Ai primit un grant de {0} {1}.",
            m.Groups[1].Value, m.Groups[2].Value);
        m = Regex.Match(s, @"^Ai luat un credit de (\d+) (\S+), cu rambursare de (\d+) (\S+)/zi\.$");
        if (m.Success)
            return F("Ai luat un credit de {0} {1}, cu rambursare de {2} {3}/zi.",
            m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value, m.Groups[4].Value);
        m = Regex.Match(s, @"^Ținta: (\d+) (\S+) profit în (\d+) zile\.$");
        if (m.Success)
            return F("Ținta: {0} {1} profit în {2} zile.",
            m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value);
        m = Regex.Match(s, @"^Ai început: (.+) × (\d+) \((\d+)\s+(zi|zile)\s*\)$");
        if (m.Success)
            return F("Ai început: {0} × {1} ({2} {3})",
            T(m.Groups[1].Value), m.Groups[2].Value, m.Groups[3].Value, T(m.Groups[4].Value));
        m = Regex.Match(s, @"^Ai cumpărat: (.+)\.$");
        if (m.Success)
            return F("Ai cumpărat: {0}.", T(m.Groups[1].Value));
        m = Regex.Match(s, @"^Ai angajat: (.+) \((\d+) bucăți, (\d+) (\S+)/zi\)\.$");
        if (m.Success)
            return F("Ai angajat: {0} ({1} bucăți, {2} {3}/zi).",
            T(m.Groups[1].Value), m.Groups[2].Value, m.Groups[3].Value, m.Groups[4].Value);
        m = Regex.Match(s, @"^Ai concediat: (.+)\.$");
        if (m.Success)
            return F("Ai concediat: {0}.", T(m.Groups[1].Value));
        m = Regex.Match(s, @"^Recoltă gata: \+(\d+) (.+) pe stoc\.$");
        if (m.Success)
            return F("Recoltă gata: +{0} {1} pe stoc.",
            m.Groups[1].Value, T(m.Groups[2].Value));
        m = Regex.Match(s, @"^Ai obținut (\d+) (\S+) profit în (\d+) zile\. Felicitări!$");
        if (m.Success)
            return F("Ai obținut {0} {1} profit în {2} zile. Felicitări!",
            m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value);
        m = Regex.Match(s, @"^Ai încheiat cu (-?\d+) (\S+) profit, iar ținta era de (\d+) (\S+)\.$");
        if (m.Success)
            return F("Ai încheiat cu {0} {1} profit, iar ținta era de {2} {1}.",
            m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value);
        return T(s);
    }

    private static string Reason(string r)
    {
        var m = Regex.Match(r, @"^Cumpărat (\d+) × (.+)$");
        if (m.Success)
            return F("Cumpărat {0} × {1}", m.Groups[1].Value, T(m.Groups[2].Value));
        m = Regex.Match(r, @"^Vândut (\d+) × (.+) la (\d+) (\S+)/buc$");
        if (m.Success)
            return F("Vândut {0} × {1} la {2} {3}/buc",
            m.Groups[1].Value, T(m.Groups[2].Value), m.Groups[3].Value, m.Groups[4].Value);
        m = Regex.Match(r, @"^Îmbunătățire: (.+)$");
        if (m.Success)
            return F("Îmbunătățire: {0}", T(m.Groups[1].Value));
        m = Regex.Match(r, @"^Angajat: (.+) \(avans\)$");
        if (m.Success)
            return F("Angajat: {0} (avans)", T(m.Groups[1].Value));
        m = Regex.Match(r, @"^Contract livrat: (.+) × (\d+)$");
        if (m.Success)
            return F("Contract livrat: {0} × {1}", T(m.Groups[1].Value), m.Groups[2].Value);
        m = Regex.Match(r, @"^rambursare credit \(mai ai (\d+) (\S+)\)$");
        if (m.Success)
            return F("rambursare credit (mai ai {0} {1})",
            m.Groups[1].Value, m.Groups[2].Value);
        return T(r); // „Eveniment”, eticheta de întreținere din date sau text necunoscut
    }

    public static string SceneText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        EnsureLoaded();
        if (Lang == Ro)
            return text;
        string direct = T(text);
        if (direct != text)
            return direct;
        var m = Regex.Match(text, @"^(Parcelă|Post) (\d+)  \+$");
        if (m.Success)
            return F(m.Groups[1].Value == "Parcelă" ? "Parcelă {0}  +" : "Post {0}  +",
            m.Groups[2].Value);
        m = Regex.Match(text, @"^(.+) · (\d+)z$");
        if (m.Success)
            return F("{0} · {1}z", T(m.Groups[1].Value), m.Groups[2].Value);
        return text;
    }
}
