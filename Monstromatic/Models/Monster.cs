using System;
using System.Collections.Generic;

namespace Monstromatic.Models;

public class Monster
{
    public Guid Id { get; }
    
    private int _encounterLevel;
    private int _personalMonsterLevel;

    private List<Skill> Skills { get;  }
    
    public Skill Attack { get; set; }
    
    public Skill Defence { get; set; }
    
    public Skill Health { get; set; }
    
    public Skill Perception { get; set; }
    
    public Skill Speed { get; set; }
    
    public Skill Trickery { get; set; }
    
    public Monster(int level, string name, FeaturesBundle featuresBundle)
    {
        Id = Guid.NewGuid();
        
        _encounterLevel = (int)(level + featuresBundle.LevelModificator);
        _personalMonsterLevel = 0;
        Name = name;
        
        Attack = new Skill("Атака", _encounterLevel, featuresBundle.AttackModificator) ;
        Defence = new Skill("Защита", _encounterLevel, featuresBundle.DefenceModificator) ;
        Health = new Skill("Здоровье", _encounterLevel, featuresBundle.HealthModificator) ;
        Perception = new Skill("Восприятие", _encounterLevel, featuresBundle.PerceptionModificator) ;
        Speed = new Skill("Воля", _encounterLevel, featuresBundle.WillModificator) ;
        Trickery = new Skill("Хитрость", _encounterLevel, featuresBundle.TrickeryModificator);

        Skills = [Attack, Defence, Health, Perception, Speed, Trickery];
    }

    public int EncounterLevel
    {
        get => _encounterLevel;
        set
        {
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
            _personalMonsterLevel = value;
            UpdateSkills();
        }
    }

    private void UpdateSkills()
    {
        foreach (var skill in Skills)
        {
            skill.Level = Level;
        }
    }
    
    public string Name { get; }
    
}