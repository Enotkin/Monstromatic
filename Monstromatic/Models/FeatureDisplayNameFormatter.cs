using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Monstromatic.Models;

public static class FeatureDisplayNameFormatter
{
    public static string Format(
        MonsterFeature feature,
        IEnumerable<SkillDefinition> skillDefinitions)
    {
        if (!feature.Key.StartsWith("User_", StringComparison.Ordinal))
        {
            return feature.DisplayName;
        }

        var skillNames = skillDefinitions.ToDictionary(skill => skill.Tag, skill => skill.Name);
        var name = string.IsNullOrWhiteSpace(feature.DetailsDisplayName)
            ? feature.DisplayName
            : feature.DetailsDisplayName;
        var modifiers = new List<string>();

        if (feature.LevelModifier != 0)
        {
            modifiers.Add($"Ур {feature.LevelModifier:+0;-0}");
        }

        modifiers.AddRange(feature.GetSkillModifiers().Select(modifier =>
        {
            var skillName = skillNames.GetValueOrDefault(modifier.Tag, modifier.Tag);
            var value = modifier.Modifier.ToString("0.##", CultureInfo.CurrentCulture);
            return $"{skillName} ×{value}";
        }));

        return modifiers.Count == 0
            ? name
            : $"{name} {string.Join(" + ", modifiers)}";
    }
}
