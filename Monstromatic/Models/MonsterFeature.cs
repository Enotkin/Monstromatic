using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Monstromatic.Models;

public class MonsterFeature
{
    public string Key { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string DetailsDisplayName { get; init; } = string.Empty;

    public int LevelModifier { get; set; }

    public IReadOnlyCollection<SkillModifier> SkillModifiers { get; init; } = [];

    public string Description { get; init; } = string.Empty;

    public bool IsHidden { get; init; }

    [JsonPropertyName("IncompatibleFeatures")]
    public IReadOnlyCollection<string> IncompatibleFeaturesKeys { get; init; } = [];

    [JsonPropertyName("IncludedFeatures")]
    public IReadOnlyCollection<string> IncludedFeaturesKeys { get; init; } = [];

    [JsonPropertyName("ExcludedFeatures")]
    public IReadOnlyCollection<string> ExcludedFeaturesKeys { get; init; } = [];

    [JsonIgnore]
    public IReadOnlyCollection<MonsterFeature> IncompatibleFeatures { get; set; } = [];

    [JsonIgnore]
    public IReadOnlyCollection<MonsterFeature> IncludedFeatures { get; set; } = [];

    [JsonIgnore]
    public IReadOnlyCollection<MonsterFeature> ExcludedFeatures { get; set; } = [];

    public double AttackModifier { get; set; }
    public double DefenceModifier { get; set; }
    public double HealthModifier { get; set; }
    public double KnowledgeModifier { get; set; }
    public double TemperModifier { get; set; }
    public double BraveryModifier { get; set; }
    public double TrickeryModifier { get; set; }

    public IReadOnlyCollection<SkillModifier> GetSkillModifiers()
    {
        if (SkillModifiers.Count > 0)
        {
            return SkillModifiers;
        }

        return new[]
        {
            CreateModifier("Attack", AttackModifier),
            CreateModifier("Defence", DefenceModifier),
            CreateModifier("Health", HealthModifier),
            CreateModifier("Knowledge", KnowledgeModifier),
            CreateModifier("Temper", TemperModifier),
            CreateModifier("Bravery", BraveryModifier),
            CreateModifier("Trickery", TrickeryModifier)
        }.Where(modifier => modifier.Modifier != 0).ToArray();
    }

    public bool HasSkillModifier(string tag) =>
        GetSkillModifiers().Any(modifier => modifier.Tag == tag && modifier.Modifier != 0);

    public override bool Equals(object? obj)
    {
        return obj is MonsterFeature feature && Equals(feature);
    }

    protected bool Equals(MonsterFeature other)
    {
        return other.Key == Key;
    }

    public override int GetHashCode() => Key.GetHashCode();

    private static SkillModifier CreateModifier(string tag, double modifier) =>
        new()
        {
            Tag = tag,
            Modifier = modifier
        };
}
