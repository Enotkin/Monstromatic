using System;
using System.Collections.Generic;

namespace Monstromatic.Models;

public class Monster
{
    public Guid Id { get; }
    
    private int _level;

    private List<Skill> Skills { get;  }
    
    public Skill Attack { get; set; }
    
    public Skill Defence { get; set; }
    
    public Skill Health { get; set; }
    
    public Skill Perception { get; set; }
    
    public Skill Speed { get; set; }
    
    public Skill Trickery { get; set; }
    
    public Monster(int level, string name, FeaturesBundle featuresBundle)
    {
        Id = new Guid();
        _level = (int)(level + featuresBundle.LevelModificator);
        Name = name;
        
        Attack = new Skill("Атака", _level, featuresBundle.AttackModificator) ;
        Defence = new Skill("Защита", _level, featuresBundle.DefenceModificator) ;
        Health = new Skill("Здоровье", _level, featuresBundle.HealthModificator) ;
        Perception = new Skill("Восприятие", _level, featuresBundle.PerceptionModificator) ;
        Speed = new Skill("Воля", _level, featuresBundle.WillModificator) ;
        Trickery = new Skill("Хитрость", _level, featuresBundle.TrickeryModificator);

        Skills = new List<Skill> { Attack, Defence, Health, Perception, Speed, Trickery};
    }

    public int Level
    {
        get => _level;
        set
        {
            _level = value;
            UpdateSkills(_level);
        }
    }

    private void UpdateSkills(int newLevel)
    {
        foreach (var skill in Skills)
        {
            skill.Level = newLevel;
        }
    }
    
    public string Name { get; }
    
}