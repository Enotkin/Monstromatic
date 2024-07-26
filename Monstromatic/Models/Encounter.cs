using System;
using System.Collections.Generic;
using System.Linq;

namespace Monstromatic.Models;

public class Encounter
{
    private readonly FeaturesBundle _featuresBundle;
    private int _level;
    private readonly Dictionary<Guid, Monster> _monsters;

    private char _lastMonsterIdentifierLetter = (char)65; //Буква А
    private const string MonsterNamePattern = "{0} - {1}";
    public string Name { get; }

    public int Level
    {
        get => _level;
        set
        {
            _level = value;
            UpdateMonstersLevel();
        }
    }

    public IReadOnlyCollection<MonsterFeature> Features => _featuresBundle.Features;

    public List<Monster> Monsters => _monsters.Values.ToList();

    public Encounter(string name, int baseLevel, IEnumerable<MonsterFeature> monsterFeatures) 
        : this(name, baseLevel, new FeaturesBundle(monsterFeatures)) {}
    
    public Encounter(string name, int baseLevel, FeaturesBundle featuresBundle)
    {
        Name = name;
        _level = baseLevel;
        _featuresBundle = featuresBundle;
        _monsters = new Dictionary<Guid, Monster>();
        
        var initMonster = new Monster(_level, string.Format(MonsterNamePattern, Name, _lastMonsterIdentifierLetter++),_featuresBundle);
        _monsters.Add(initMonster.Id, initMonster);
    }

    public Monster AddMonster()
    {
        var monster = new Monster(_level, string.Format(MonsterNamePattern, Name, _lastMonsterIdentifierLetter++), _featuresBundle);
        _monsters.Add(monster.Id, monster);
        return monster;
    }

    public void RemoveMonster(Guid monsterId)
    {
        _monsters.Remove(monsterId);
    }

    private void UpdateMonstersLevel()
    {
        foreach (var monster in Monsters) 
            monster.EncounterLevel = _level;
    }
}