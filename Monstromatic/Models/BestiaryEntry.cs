using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Monstromatic.Models;

public class BestiaryEntry
{
    public string Name { get; init; } = string.Empty;

    public int Level { get; init; }

    public string QualityName { get; set; } = string.Empty;

    public List<MonsterFeature> Features { get; init; } = [];

    public List<SkillDefinition> SkillDefinitions { get; init; } = [];

    public List<BestiaryMonsterState> Monsters { get; init; } = [];

    [JsonIgnore]
    public string FeaturesSummary => Features.Count == 0
        ? "Без особенностей"
        : string.Join(", ", FeatureDisplayNames);

    [JsonIgnore]
    public IReadOnlyCollection<string> FeatureDisplayNames => Features
        .Select(feature => FeatureDisplayNameFormatter.Format(feature, SkillDefinitions))
        .ToArray();

    [JsonIgnore]
    public string QualityDisplayName => string.IsNullOrWhiteSpace(QualityName)
        ? "Не указан"
        : QualityName;

    [JsonIgnore]
    public bool HasFeatures => Features.Count > 0;

    public static BestiaryEntry FromEncounter(Encounter encounter)
    {
        return new BestiaryEntry
        {
            Name = encounter.Name,
            Level = encounter.Level,
            QualityName = encounter.QualityName,
            Features = encounter.Features.ToList(),
            SkillDefinitions = encounter.SkillDefinitions.ToList(),
            Monsters = encounter.Monsters.Select(BestiaryMonsterState.FromMonster).ToList()
        };
    }

    public Encounter CreateEncounter()
    {
        var featuresBundle = new FeaturesBundle(Features, SkillDefinitions);
        var encounter = new Encounter(
            Name,
            Level - featuresBundle.LevelModificator,
            featuresBundle,
            QualityName);
        var firstMonster = encounter.Monsters.First();

        if (Monsters.Count == 0)
        {
            encounter.RemoveMonster(firstMonster.Id);
            return encounter;
        }

        ApplyState(firstMonster, Monsters[0]);

        foreach (var monsterState in Monsters.Skip(1))
        {
            ApplyState(encounter.AddMonster(), monsterState);
        }

        return encounter;
    }

    private static void ApplyState(Monster monster, BestiaryMonsterState state)
    {
        monster.EncounterLevel = state.EncounterLevel;
        monster.PersonalLevel = state.PersonalLevel;

        foreach (var skill in monster.Skills)
        {
            if (state.SkillManualDeltas.TryGetValue(skill.Tag, out var manualDelta))
            {
                skill.SetManualDelta(manualDelta);
            }
        }
    }
}

public class BestiaryMonsterState
{
    public int EncounterLevel { get; init; }

    public int PersonalLevel { get; init; }

    public Dictionary<string, int> SkillManualDeltas { get; init; } = [];

    public static BestiaryMonsterState FromMonster(Monster monster)
    {
        return new BestiaryMonsterState
        {
            EncounterLevel = monster.EncounterLevel,
            PersonalLevel = monster.PersonalLevel,
            SkillManualDeltas = monster.Skills.ToDictionary(skill => skill.Tag, skill => skill.ManualDelta)
        };
    }
}
