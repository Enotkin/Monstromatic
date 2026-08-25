using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Monstromatic.Data.Profiles;
using Monstromatic.Models;

var tests = new (string Name, Action Run)[]
{
    ("standard x1.5 at levels 4, 6, 8", StandardModifierExamples),
    ("standard + feature + manual examples", CombinedModifierExamples),
    ("skill recalculates from new monster level", SkillRecalculatesFromNewLevel),
    ("skill applies configured boost", SkillAppliesConfiguredBoost),
    ("settings deserialize dynamic skills", SettingsDeserializeDynamicSkills),
    ("feature deserialize skill modifiers", FeatureDeserializeSkillModifiers),
    ("feature exposes tag-based modifiers", FeatureSkillModifiers),
    ("skill stores feature comments", SkillStoresFeatureComments),
    ("legacy feature modifiers are converted to tags", LegacyFeatureModifiersFallback),
    ("odd monster level is validation error", OddMonsterLevelFails),
    ("fractional modifier delta is validation error", FractionalDeltaFails),
    ("modifier validity check matches skill calculation", ModifierValidityMatchesCalculation),
    ("renaming a skill tag keeps features working", RenamingTagUpdatesFeatures),
    ("renaming a skill tag converts legacy feature modifiers", RenamingTagConvertsLegacyModifiers),
    ("removing a skill tag drops dependent features", RemovingTagDropsDependentFeatures),
    ("untouched features survive skill edits unchanged", UntouchedFeaturesStayUnchanged),
    ("legacy data files move into the first profile", LegacyFilesMoveIntoProfile),
    ("a fresh install gets an empty first profile", FreshInstallCreatesProfile),
    ("second start keeps the existing profile", SecondStartKeepsProfile),
    ("remembered profile is forgotten when it disappears", MissingRememberedProfileIsForgotten)
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

static void SkillAppliesConfiguredBoost()
{
    var skill = new Skill("Attack", "Attack", 4, 1, boostValue: 3);

    AssertEqual(4, skill.Value);
    AssertEqual(true, skill.ApplyBoost());
    AssertEqual(7, skill.Value);
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
              "BaseModifier": 0.5,
              "BoostValue": 3
            }
          ]
        }
        """)!;

    var skill = settings.SkillDefinitions.Single();

    AssertEqual("Атака", skill.Name);
    AssertEqual("Attack", skill.Tag);
    AssertEqual(0.5, skill.BaseModifier);
    AssertEqual(3, skill.BoostValue);
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

static void SkillStoresFeatureComments()
{
    var skill = new Skill(
        "Attack",
        "Attack",
        4,
        1,
        comments: [new SkillComment("Feature", "Note")]);

    var comment = skill.Comments.Single();

    AssertEqual("Feature", comment.FeatureName);
    AssertEqual("Note", comment.Text);
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

static void ModifierValidityMatchesCalculation()
{
    // шаг 0,5 на чётном уровне всегда даёт целую прибавку
    AssertEqual(true, Skill.IsModifierValidForLevel(2, 1.5));
    AssertEqual(true, Skill.IsModifierValidForLevel(4, 1.5));
    // а вот более мелкий шаг на низком уровне — уже нет
    AssertEqual(false, Skill.IsModifierValidForLevel(2, 1.25));
    AssertEqual(true, Skill.IsModifierValidForLevel(4, 1.25));
    AssertEqual(false, Skill.IsModifierValidForLevel(4, 1.2));
    AssertEqual(true, Skill.IsModifierValidForLevel(4, 1));
}

static void RenamingTagUpdatesFeatures()
{
    var feature = new MonsterFeature
    {
        Key = "Armored",
        SkillModifiers = new[] { new SkillModifier { Tag = "Attack", Modifier = 1.5 } }
    };

    var updated = SkillTagMigration.Apply(
        new[] { feature },
        new Dictionary<string, string> { ["Attack"] = "Power" },
        Array.Empty<string>()).Single();

    var modifier = updated.GetSkillModifiers().Single();

    AssertEqual("Power", modifier.Tag);
    AssertEqual(1.5, modifier.Modifier);
    AssertEqual("Armored", updated.Key);
}

static void RenamingTagConvertsLegacyModifiers()
{
    var feature = new MonsterFeature
    {
        Key = "Legacy",
        AttackModifier = 1.5
    };

    var updated = SkillTagMigration.Apply(
        new[] { feature },
        new Dictionary<string, string> { ["Attack"] = "Power" },
        Array.Empty<string>()).Single();

    var modifier = updated.GetSkillModifiers().Single();

    AssertEqual("Power", modifier.Tag);
    AssertEqual(1.5, modifier.Modifier);
    // устаревшее поле больше не должно подменять собой явный список
    AssertEqual(0d, updated.AttackModifier);
}

static void RemovingTagDropsDependentFeatures()
{
    var dependent = new MonsterFeature
    {
        Key = "Dependent",
        SkillModifiers = new[] { new SkillModifier { Tag = "Attack", Modifier = 1.5 } }
    };
    var unrelated = new MonsterFeature
    {
        Key = "Unrelated",
        SkillModifiers = new[] { new SkillModifier { Tag = "Defence", Modifier = 1.5 } }
    };

    var dependentFeatures = SkillTagMigration.FindDependentFeatures(
        new[] { dependent, unrelated },
        new[] { "Attack" });

    AssertEqual("Dependent", dependentFeatures.Single().Key);

    var survivors = SkillTagMigration.Apply(
        new[] { dependent, unrelated },
        new Dictionary<string, string>(),
        new[] { "Attack" });

    AssertEqual("Unrelated", survivors.Single().Key);
}

static void UntouchedFeaturesStayUnchanged()
{
    var feature = new MonsterFeature
    {
        Key = "Tough",
        LevelModifier = 2,
        SkillModifiers = new[] { new SkillModifier { Tag = "Health", Modifier = 1.5 } }
    };

    var updated = SkillTagMigration.Apply(
        new[] { feature },
        new Dictionary<string, string> { ["Attack"] = "Power" },
        new[] { "Defence" }).Single();

    AssertEqual(true, ReferenceEquals(feature, updated));
}

static void LegacyFilesMoveIntoProfile()
{
    RunInTemporaryFolder((legacyDirectory, profilesDirectory) =>
    {
        File.WriteAllText(Path.Combine(legacyDirectory, "settings.json"), "{}");
        File.WriteAllText(Path.Combine(legacyDirectory, "features.json"), "[]");
        File.WriteAllText(Path.Combine(legacyDirectory, "bestiary.json"), "[]");

        var service = CreateProfileService(legacyDirectory, profilesDirectory);
        service.Initialize();

        AssertEqual(1, service.Profiles.Count);
        AssertEqual("Основной", service.Profiles[0].Name);

        foreach (var fileName in new[] { "settings.json", "features.json", "bestiary.json" })
        {
            AssertEqual(true, File.Exists(Path.Combine(service.CurrentProfileDirectory, fileName)));
            // оригиналы остаются страховочной копией
            AssertEqual(true, File.Exists(Path.Combine(legacyDirectory, fileName)));
        }
    });
}

static void FreshInstallCreatesProfile()
{
    RunInTemporaryFolder((legacyDirectory, profilesDirectory) =>
    {
        var service = CreateProfileService(legacyDirectory, profilesDirectory);
        service.Initialize();

        AssertEqual(1, service.Profiles.Count);
        AssertEqual(true, Directory.Exists(service.CurrentProfileDirectory));
        AssertEqual(false, File.Exists(Path.Combine(service.CurrentProfileDirectory, "settings.json")));
    });
}

static void SecondStartKeepsProfile()
{
    RunInTemporaryFolder((legacyDirectory, profilesDirectory) =>
    {
        File.WriteAllText(Path.Combine(legacyDirectory, "settings.json"), "{}");

        var first = CreateProfileService(legacyDirectory, profilesDirectory);
        first.Initialize();
        var created = first.Create("Второй");
        first.Remember(created);

        var second = CreateProfileService(legacyDirectory, profilesDirectory);
        second.Initialize();

        AssertEqual(2, second.Profiles.Count);
        AssertEqual(created.Id, second.RememberedProfileId);
    });
}

static void MissingRememberedProfileIsForgotten()
{
    RunInTemporaryFolder((legacyDirectory, profilesDirectory) =>
    {
        var first = CreateProfileService(legacyDirectory, profilesDirectory);
        first.Initialize();
        var created = first.Create("Второй");
        first.Remember(created);
        first.Delete(created);

        var second = CreateProfileService(legacyDirectory, profilesDirectory);
        second.Initialize();

        AssertEqual(1, second.Profiles.Count);
        AssertEqual(null, second.RememberedProfileId);
    });
}

static ProfileService CreateProfileService(string legacyDirectory, string profilesDirectory) =>
    new(profilesDirectory, Path.Combine(profilesDirectory, "profiles.json"), legacyDirectory);

static void RunInTemporaryFolder(Action<string, string> test)
{
    var root = Path.Combine(Path.GetTempPath(), "monstromatic-tests-" + Guid.NewGuid().ToString("N"));
    var legacyDirectory = Path.Combine(root, "app");
    var profilesDirectory = Path.Combine(root, "app", "Profiles");
    Directory.CreateDirectory(legacyDirectory);

    try
    {
        test(legacyDirectory, profilesDirectory);
    }
    finally
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }
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
