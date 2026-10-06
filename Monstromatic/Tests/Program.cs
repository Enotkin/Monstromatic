using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Monstromatic.Models;
using Monstromatic.Views;

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
    ("quick skill sweep never reveals controls", SkillLensQuickSweep),
    ("skill controls appear only after full dwell", SkillLensRequiresFullDwell),
    ("movement within a skill preserves dwell", SkillLensMovementPreservesDwell),
    ("switching skills requires a new full dwell", SkillLensSwitchStartsNewDwell),
    ("leaving and reentering active skill preserves selection", SkillLensActiveSelectionPersists),
    ("leaving cancels dwell before reentry", SkillLensReentryRestartsDwell),
    ("reset clears active selection and pending dwell", SkillLensResetClearsState)
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

static void SkillLensQuickSweep()
{
    var lens = new SkillLensState<object>();
    var skills = new[] { new object(), new object(), new object() };

    for (var index = 0; index < skills.Length; index++)
    {
        lens.MoveTo(skills[index], TimeSpan.FromMilliseconds(index * 100));
        AssertEqual(false, lens.TryActivate(TimeSpan.FromMilliseconds(index * 100 + 99)));
        AssertEqual<object?>(null, lens.ActiveSkill);
    }

    lens.MoveTo(null, TimeSpan.FromMilliseconds(300));
    AssertEqual(false, lens.TryActivate(TimeSpan.FromSeconds(2)));
    AssertEqual<object?>(null, lens.HoveredSkill);
    AssertEqual<object?>(null, lens.ActiveSkill);
}

static void SkillLensRequiresFullDwell()
{
    var lens = new SkillLensState<object>();
    var skill = new object();
    var enteredAt = TimeSpan.FromSeconds(1);
    lens.MoveTo(skill, enteredAt);

    AssertEqual(false, lens.TryActivate(enteredAt + SkillLensState<object>.ActivationDelay - TimeSpan.FromMilliseconds(1)));
    AssertEqual<object?>(null, lens.ActiveSkill);
    AssertEqual(true, lens.TryActivate(enteredAt + SkillLensState<object>.ActivationDelay));
    AssertEqual(skill, lens.ActiveSkill);
    AssertEqual(false, lens.TryActivate(enteredAt + TimeSpan.FromSeconds(1)));
}

static void SkillLensMovementPreservesDwell()
{
    var lens = new SkillLensState<object>();
    var skill = new object();
    lens.MoveTo(skill, TimeSpan.Zero);
    lens.MoveTo(skill, TimeSpan.FromMilliseconds(100));
    lens.MoveTo(skill, TimeSpan.FromMilliseconds(300));

    AssertEqual(true, lens.TryActivate(SkillLensState<object>.ActivationDelay));
    AssertEqual(skill, lens.ActiveSkill);
}

static void SkillLensSwitchStartsNewDwell()
{
    var lens = new SkillLensState<object>();
    var first = new object();
    var second = new object();
    lens.MoveTo(first, TimeSpan.Zero);
    var interruptedAt = SkillLensState<object>.ActivationDelay - TimeSpan.FromMilliseconds(1);
    lens.MoveTo(second, interruptedAt);
    AssertEqual(false, lens.TryActivate(SkillLensState<object>.ActivationDelay));
    AssertEqual(false, lens.TryActivate(interruptedAt + SkillLensState<object>.ActivationDelay - TimeSpan.FromMilliseconds(1)));
    AssertEqual(true, lens.TryActivate(interruptedAt + SkillLensState<object>.ActivationDelay));
    AssertEqual(second, lens.ActiveSkill);

    var switchedAt = TimeSpan.FromSeconds(1);
    lens.MoveTo(first, switchedAt);
    AssertEqual(first, lens.HoveredSkill);
    AssertEqual<object?>(null, lens.ActiveSkill);
    AssertEqual(false, lens.TryActivate(switchedAt + SkillLensState<object>.ActivationDelay - TimeSpan.FromMilliseconds(1)));
    AssertEqual(true, lens.TryActivate(switchedAt + SkillLensState<object>.ActivationDelay));
    AssertEqual(first, lens.ActiveSkill);
}

static void SkillLensActiveSelectionPersists()
{
    var lens = new SkillLensState<object>();
    var skill = new object();
    lens.MoveTo(skill, TimeSpan.Zero);
    AssertEqual(true, lens.TryActivate(SkillLensState<object>.ActivationDelay));

    lens.MoveTo(null, TimeSpan.FromSeconds(1));
    AssertEqual<object?>(null, lens.HoveredSkill);
    AssertEqual(skill, lens.ActiveSkill);
    AssertEqual(false, lens.TryActivate(TimeSpan.FromSeconds(2)));

    lens.MoveTo(skill, TimeSpan.FromSeconds(3));
    AssertEqual(skill, lens.ActiveSkill);
    AssertEqual(false, lens.TryActivate(TimeSpan.FromSeconds(4)));

    lens.MoveTo(null, TimeSpan.FromSeconds(5));
    var anotherSkill = new object();
    lens.MoveTo(anotherSkill, TimeSpan.FromSeconds(6));
    AssertEqual<object?>(null, lens.ActiveSkill);
    AssertEqual(anotherSkill, lens.HoveredSkill);
}

static void SkillLensReentryRestartsDwell()
{
    var lens = new SkillLensState<object>();
    var skill = new object();
    lens.MoveTo(skill, TimeSpan.Zero);
    lens.MoveTo(null, TimeSpan.FromMilliseconds(300));
    AssertEqual(false, lens.TryActivate(TimeSpan.FromSeconds(1)));

    var reenteredAt = TimeSpan.FromSeconds(2);
    lens.MoveTo(skill, reenteredAt);
    AssertEqual(false, lens.TryActivate(reenteredAt + SkillLensState<object>.ActivationDelay - TimeSpan.FromMilliseconds(1)));
    AssertEqual(true, lens.TryActivate(reenteredAt + SkillLensState<object>.ActivationDelay));
    AssertEqual(skill, lens.ActiveSkill);
}

static void SkillLensResetClearsState()
{
    var lens = new SkillLensState<object>();
    var skill = new object();
    lens.MoveTo(skill, TimeSpan.Zero);
    AssertEqual(true, lens.TryActivate(SkillLensState<object>.ActivationDelay));

    lens.Reset();
    AssertEqual<object?>(null, lens.HoveredSkill);
    AssertEqual<object?>(null, lens.ActiveSkill);
    AssertEqual(false, lens.TryActivate(TimeSpan.FromSeconds(1)));

    lens.MoveTo(skill, TimeSpan.FromSeconds(2));
    lens.Reset();
    AssertEqual(false, lens.TryActivate(TimeSpan.FromSeconds(3)));
    AssertEqual<object?>(null, lens.HoveredSkill);
    AssertEqual<object?>(null, lens.ActiveSkill);

    lens.MoveTo(skill, TimeSpan.FromSeconds(4));
    AssertEqual(true, lens.TryActivate(TimeSpan.FromSeconds(4) + SkillLensState<object>.ActivationDelay));
    AssertEqual(skill, lens.ActiveSkill);
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
