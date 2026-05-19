using System;
using System.Collections.Generic;
using System.Linq;

namespace Monstromatic.Models;

public class Monster
{
    public Guid Id { get; }

    private int _encounterLevel;
    private int _personalMonsterLevel;
    private readonly List<Skill> _skills;

    public Monster(int level, string name, FeaturesBundle featuresBundle)
    {
        Id = Guid.NewGuid();

        MonsterLevelRules.ValidateEvenLevel(level);
        _encounterLevel = level;
        _personalMonsterLevel = 0;
        Name = name;

        _skills = featuresBundle.SkillDefinitions
            .Select(skill => new Skill(
                skill.Name,
                skill.Tag,
                _encounterLevel,
                skill.BaseModifier,
                featuresBundle.GetFeatureModifiers(skill.Tag),
                featuresBundle.GetSkillComments(skill.Tag)))
            .ToList();
    }

    public IReadOnlyCollection<Skill> Skills => _skills;

    public int EncounterLevel
    {
        get => _encounterLevel;
        set
        {
            MonsterLevelRules.ValidateEvenLevel(value);
            _encounterLevel = value;
            UpdateSkills();
        }
    }

    public int Level => _encounterLevel + _personalMonsterLevel;

    public int PersonalLevel
    {
        get => _personalMonsterLevel;
        set
        {
            MonsterLevelRules.ValidateEvenLevel(Level - _personalMonsterLevel + value);
            _personalMonsterLevel = value;
            UpdateSkills();
        }
    }

    public string Name { get; }

    public void ResetModifications()
    {
        PersonalLevel = 0;
        foreach (var skill in _skills)
        {
            skill.Reset();
        }
    }

    private void UpdateSkills()
    {
        foreach (var skill in _skills)
        {
            skill.Level = Level;
        }
    }
}
