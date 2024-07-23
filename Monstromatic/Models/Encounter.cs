using System;
using System.Collections.Generic;
using System.Linq;

namespace Monstromatic.Models;

public class Encounter
{
    private readonly FeaturesBundle _featuresBundle;
    private Dictionary<Guid, Monster> _monsters;

    private char _lastMonsterIdificatorLetter = (char)65;
    private const string MonsterNamePattern = "{0} - {1}";
    public string Name { get; }

    public int Level { get; set; }

    public IReadOnlyCollection<MonsterFeature> Features => _featuresBundle.Features;

    public List<Monster> Monsters => _monsters.Values.ToList();

    public Encounter(string name, int baseLevel, IEnumerable<MonsterFeature> monsterFeatures) 
        : this(name, baseLevel, new FeaturesBundle(monsterFeatures)) {}
    
    public Encounter(string name, int baseLevel, FeaturesBundle featuresBundle)
    {
        Name = name;
        Level = baseLevel;
        _featuresBundle = featuresBundle;
        _monsters = new Dictionary<Guid, Monster>();
        
        var initMonster = new Monster(Level, string.Format(MonsterNamePattern, Name, _lastMonsterIdificatorLetter++),_featuresBundle);
        _monsters.Add(initMonster.Id, initMonster);
    }

    public Monster AddMonster()
    {
        var monster = new Monster(Level, string.Format(MonsterNamePattern, Name, _lastMonsterIdificatorLetter++), _featuresBundle);
        _monsters.Add(monster.Id, monster);
        return monster;
    }

    public void RemoveMonster(Guid monsterId)
    {
        _monsters.Remove(monsterId);
    }
}