using System;
using System.Collections.Generic;
using System.Linq;
using Monstromatic.Data.AppSettingsProvider;
using Monstromatic.Utils;

namespace Monstromatic.Models;

public class FeaturesBundle
{
    private readonly List<MonsterFeature> _features;

    public FeaturesBundle(IEnumerable<MonsterFeature> features)
        : this(features, ServiceHub.Default.ServiceProvider.Get<IAppSettingsProvider>().Settings.SkillDefinitions)
    {
    }

    public FeaturesBundle(IEnumerable<MonsterFeature> features, IEnumerable<SkillDefinition> skillDefinitions)
    {
        _features = features.ToList();
        SkillDefinitions = skillDefinitions.ToArray();

        ValidateSkillDefinitions();
        ValidateFeatureModifiers();

        LevelModificator = _features.Sum(f => f.LevelModifier);
    }

    public int LevelModificator { get; }

    public IReadOnlyCollection<SkillDefinition> SkillDefinitions { get; }

    public IReadOnlyCollection<MonsterFeature> Features => _features;

    public IReadOnlyCollection<double> GetFeatureModifiers(string tag)
    {
        return _features
            .SelectMany(feature => feature.GetSkillModifiers())
            .Where(modifier => modifier.Tag == tag && modifier.Modifier != 0)
            .Select(modifier => modifier.Modifier)
            .ToArray();
    }

    private void ValidateSkillDefinitions()
    {
        var emptyTags = SkillDefinitions.Where(skill => string.IsNullOrWhiteSpace(skill.Tag)).ToArray();
        if (emptyTags.Length > 0)
        {
            throw new InvalidOperationException("Skill tag cannot be empty.");
        }

        var duplicateTag = SkillDefinitions
            .GroupBy(skill => skill.Tag)
            .FirstOrDefault(group => group.Count() > 1)
            ?.Key;

        if (duplicateTag is not null)
        {
            throw new InvalidOperationException($"Skill tag '{duplicateTag}' is duplicated in settings.");
        }
    }

    private void ValidateFeatureModifiers()
    {
        var skillTags = SkillDefinitions.Select(skill => skill.Tag).ToHashSet();
        var unknownModifier = _features
            .SelectMany(feature => feature.GetSkillModifiers(), (feature, modifier) => new { feature, modifier })
            .FirstOrDefault(pair => !skillTags.Contains(pair.modifier.Tag));

        if (unknownModifier is not null)
        {
            throw new InvalidOperationException(
                $"Feature '{unknownModifier.feature.Key}' uses unknown skill tag '{unknownModifier.modifier.Tag}'.");
        }
    }
}
