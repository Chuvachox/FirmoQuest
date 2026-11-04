using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BusinessSim;

// Verifica acoperirea traducerilor si parametrii textelor din date.

public static class LocalizationTest
{
    public static void Run(Content content, Action<bool, string> check)
    {
        var table = (IDictionary)typeof(Locale).GetField("Table", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
        var missing = new List<string>();
        void Covered(string value)
        {
            if (!string.IsNullOrEmpty(value) && !table.Contains(value))
                missing.Add(value);
        }
        Covered(content.Config.Subtitle);
        foreach (var difficulty in content.Config.Difficulties)
        {
            Covered(difficulty.Label);
            Covered(difficulty.Short);
            Covered(difficulty.Description);
        }
        foreach (var scenario in content.Scenarios.Values)
        {
            Covered(scenario.Name);
            Covered(scenario.Short);
            Covered(scenario.Description);
            Covered(scenario.Goal);
            Covered(scenario.UpkeepLabel);
            Covered(scenario.SlotsLabel);
            foreach (var resource in scenario.Resources)
            {
                Covered(resource.Name);
                Covered(resource.Desc);
                Covered(resource.Unit);
            }
            foreach (var product in scenario.Products)
            {
                Covered(product.Name);
                Covered(product.Desc);
            }
            foreach (var upgrade in scenario.Upgrades)
            {
                Covered(upgrade.Name);
                Covered(upgrade.Desc);
            }
            foreach (var staff in scenario.Staff)
            {
                Covered(staff.Name);
                Covered(staff.Desc);
            }
        }
        foreach (var ev in content.Events)
        {
            Covered(ev.Title);
            Covered(ev.Text);
            foreach (var choice in ev.Choices)
                Covered(choice.Label);
        }
        check(missing.Count == 0, "localizare: toate textele din date au RU/EN: " + string.Join("; ", missing));

        var malformed = new List<string>();
        HashSet<string> Slots(string value) => Regex.Matches(value, @"\{(\d+)(?:[^}]*)\}")
            .Select(m => m.Groups[1].Value).ToHashSet();
        foreach (DictionaryEntry entry in table)
        {
            var (ru, en) = ((string, string))entry.Value!;
            string ro = (string)entry.Key;
            foreach (string translation in new[] { ru, en })
                if (string.IsNullOrWhiteSpace(translation) || !Slots(ro).SetEquals(Slots(translation)))
                    malformed.Add(ro);
        }
        check(malformed.Count == 0, "localizare: traduceri complete cu aceiasi parametri: " + string.Join("; ", malformed));
    }
}
