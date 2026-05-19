using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Monstromatic.Models;

public class MonstromaticSettings
{
    public Dictionary<string, int> MonsterQualities { get; set; } = new();

    public IReadOnlyCollection<SkillDefinition> Skills { get; set; } = [];

    public DefaultModifiers? DefaultModifiers { get; set; }

    [JsonIgnore]
    public IReadOnlyCollection<SkillDefinition> SkillDefinitions =>
        Skills.Count > 0
            ? Skills
            : DefaultModifiers?.ToSkillDefinitions() ?? [];
}

public class DefaultModifiers
{
    public double AttackModifier { get; set; }
    public double DefenceModifier { get; set; }
    public double HealthModifier { get; set; }
    public double KnowledgeModifier { get; set; }
    public double TemperModifier { get; set; }
    public double BraveryModifier { get; set; }
    public double TrickeryModifier { get; set; }

    public IReadOnlyCollection<SkillDefinition> ToSkillDefinitions()
    {
        return new[]
        {
            CreateSkill("Атака", "Attack", AttackModifier),
            CreateSkill("Защита", "Defence", DefenceModifier),
            CreateSkill("Здоровье", "Health", HealthModifier),
            CreateSkill("Знания", "Knowledge", KnowledgeModifier),
            CreateSkill("Храбрость", "Temper", TemperModifier),
            CreateSkill("Храбрость", "Bravery", BraveryModifier),
            CreateSkill("Хитрость", "Trickery", TrickeryModifier)
        }.Where(skill => skill.BaseModifier != 0).ToArray();
    }

    private static SkillDefinition CreateSkill(string name, string tag, double baseModifier) =>
        new()
        {
            Name = name,
            Tag = tag,
            BaseModifier = baseModifier
        };
}
