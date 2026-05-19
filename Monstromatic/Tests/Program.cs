using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Monstromatic.Models;

var tests = new (string Name, Action Run)[]
{
    ("standard x1.5 at levels 4, 6, 8", StandardModifierExamples),
    ("standard + feature + manual examples", CombinedModifierExamples),
    ("skill recalculates from new monster level", SkillRecalculatesFromNewLevel),
    ("settings deserialize dynamic skills", SettingsDeserializeDynamicSkills),
    ("feature deserialize skill modifiers", FeatureDeserializeSkillModifiers),
    ("feature exposes tag-based modifiers", FeatureSkillModifiers),
    ("legacy feature modifiers are converted to tags", LegacyFeatureModifiersFallback),
    ("odd monster level is validation error", OddMonsterLevelFails),
    ("fractional modifier delta is validation error", FractionalDeltaFails)
};

foreach (var test in tests)
{
    test.Run();
    Console.WriteLine($"PASS {test.Name}");
}

static void StandardModifierExamples()
{
    AssertEqual(6, new Skill("Attack", 4, 1.5).Value);
    AssertEqual(9, new Skill("Attack", 6, 1.5).Value);
    AssertEqual(12, new Skill("Attack", 8, 1.5).Value);
}

static void CombinedModifierExamples()
{
    AssertEqual(9, CreateCombinedSkill(4).Value);
    AssertEqual(13, CreateCombinedSkill(6).Value);
    AssertEqual(17, CreateCombinedSkill(8).Value);
}

static void SkillRecalculatesFromNewLevel()
{
    var skill = CreateCombinedSkill(4);
    AssertEqual(9, skill.Value);

    skill.Level = 6;

    AssertEqual(13, skill.Value);
}

static void SettingsDeserializeDynamicSkills()
{
    var settings = JsonSerializer.Deserialize<MonstromaticSettings>(
        """
        {
          "MonsterQualities": {},
          "Skills": [
            {
              "Name": "Атака",
              "Tag": "Attack",
              "BaseModifier": 0.5
            }
          ]
        }
        """)!;

    var skill = settings.SkillDefinitions.Single();

    AssertEqual("Атака", skill.Name);
    AssertEqual("Attack", skill.Tag);
    AssertEqual(0.5, skill.BaseModifier);
}

static void FeatureDeserializeSkillModifiers()
{
    var feature = JsonSerializer.Deserialize<MonsterFeature>(
        """
        {
          "Key": "Big",
          "DisplayName": "Большой",
          "SkillModifiers": [
            {
              "Tag": "Attack",
              "Modifier": 1.5
            }
          ]
        }
        """)!;

    var modifier = feature.GetSkillModifiers().Single();

    AssertEqual("Attack", modifier.Tag);
    AssertEqual(1.5, modifier.Modifier);
}

static void FeatureSkillModifiers()
{
    var feature = new MonsterFeature
    {
        SkillModifiers =
        [
            new SkillModifier { Tag = "Attack", Modifier = 1.5 },
            new SkillModifier { Tag = "Defence", Modifier = 0.5 }
        ]
    };

    AssertEqual(true, feature.HasSkillModifier("Attack"));
    AssertEqual(false, feature.HasSkillModifier("Health"));
    AssertEqual(1.5, feature.GetSkillModifiers().Single(modifier => modifier.Tag == "Attack").Modifier);
}

static void LegacyFeatureModifiersFallback()
{
    var feature = new MonsterFeature
    {
        AttackModifier = 1.5
    };

    var modifier = feature.GetSkillModifiers().Single();

    AssertEqual("Attack", modifier.Tag);
    AssertEqual(1.5, modifier.Modifier);
}

static void OddMonsterLevelFails()
{
    AssertThrows<ValidationException>(() => new Skill("Attack", 5, 1));
}

static void FractionalDeltaFails()
{
    var skill = new Skill("Attack", 4, 1.2);

    AssertThrows<ValidationException>(() => _ = skill.Value);
}

static Skill CreateCombinedSkill(int level)
{
    var skill = new Skill("Attack", level, 1.5, new[] { 1.5 });
    skill.Increment();
    return skill;
}

static void AssertEqual<T>(T expected, T actual)
{
    if (!Equals(expected, actual))
        throw new InvalidOperationException($"Expected {expected}, got {actual}.");
}

static void AssertThrows<TException>(Action action)
    where TException : Exception
{
    try
    {
        action();
    }
    catch (TException)
    {
        return;
    }

    throw new InvalidOperationException($"Expected exception {typeof(TException).Name}.");
}
