using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;

namespace Monstromatic.Models;

public partial class Encounter : ReactiveObject
{
    private readonly FeaturesBundle _featuresBundle;
    private readonly int _baseLevel;
    
    private int _level;
    
    private readonly Dictionary<Guid, Monster> _monsters;

    private char _lastMonsterIdentifierLetter = (char)65; //Буква А
    private const string MonsterNamePattern = "{0} - {1}";
    public string Name { get; }

    public IReadOnlyCollection<MonsterFeature> Features => _featuresBundle.Features;

    public List<Monster> Monsters => _monsters.Values.ToList();

    public int Level
    {
        get => _level;
        set
        {
            MonsterLevelRules.ValidateEvenLevel(value);
            this.RaiseAndSetIfChanged(ref _level, value);
        }
    }

    public Encounter(string name, int baseLevel, IEnumerable<MonsterFeature> monsterFeatures) 
        : this(name, baseLevel, new FeaturesBundle(monsterFeatures)) {}
    
    public Encounter(string name, int baseLevel, FeaturesBundle featuresBundle)
    {
        Name = name;
        Level = baseLevel + featuresBundle.LevelModificator;
        MonsterLevelRules.ValidateEvenLevel(Level);
        _baseLevel = baseLevel;
        _featuresBundle = featuresBundle;
        _monsters = new Dictionary<Guid, Monster>();
        
        var initMonster = new Monster(Level, string.Format(MonsterNamePattern, Name, _lastMonsterIdentifierLetter++), _featuresBundle);
        _monsters.Add(initMonster.Id, initMonster);
        
    }

    public void ApplyLevelToMonsters()
    {
        foreach (var monster in Monsters)
        {
            monster.EncounterLevel = Level;
        }
    }

    public Monster AddMonster()
    {
        var monster = new Monster(Level, string.Format(MonsterNamePattern, Name, _lastMonsterIdentifierLetter++), _featuresBundle);
        _monsters.Add(monster.Id, monster);
        return monster;
    }

    public void RemoveMonster(Guid monsterId)
    {
        _monsters.Remove(monsterId);
    }
}
