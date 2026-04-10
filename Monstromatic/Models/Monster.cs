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
    
    public Skill Bravery { get; set; }
    
    public Skill Trickery { get; set; }
    
    public Monster(int level, string name, FeaturesBundle featuresBundle)
    {
        Id = Guid.NewGuid();
        
        _encounterLevel = (int)(level + featuresBundle.LevelModificator);
        _personalMonsterLevel = 0;
        Name = name;
        
        Attack = new Skill("Атака", _encounterLevel, featuresBundle.AttackModificator) ;
        Defence = new Skill("Защита", _encounterLevel, featuresBundle.DefenceModificator) ;
        Bravery = new Skill("Храбрость", _encounterLevel, featuresBundle.BraveryModificator) ;
        Trickery = new Skill("Хитрость", _encounterLevel, featuresBundle.TrickeryModificator);

        Skills = [Attack, Defence, Bravery, Trickery];
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

    public void ResetModifications()
    {
        PersonalLevel = 0;
        foreach (var skill in Skills)
        {
            skill.Reset();
        }
    }
    
    public string Name { get; }
    
}