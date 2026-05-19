using System;
using System.ComponentModel.DataAnnotations;
using Monstromatic.Models;

var tests = new (string Name, Action Run)[]
{
    ("standard x1.5 at levels 4, 6, 8", StandardModifierExamples),
    ("standard + feature + manual examples", CombinedModifierExamples),
    ("skill recalculates from new monster level", SkillRecalculatesFromNewLevel),
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

static void AssertEqual(int expected, int actual)
{
    if (expected != actual)
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
