using System.Collections.Generic;

namespace Monstromatic.Models;

public class Encounter
{
    private readonly FeaturesBundle _featuresBundle;

    private char _lastMonsterIdificatorLetter = (char)65;
    private const string MonsterNamePattern = "{0} - {1}";
    public string Name { get; }

    public int Level { get; set; }

    public IReadOnlyCollection<MonsterFeature> Features => _featuresBundle.Features;

    public List<Monster> Monsters { get; private set; }

    public Encounter(string name, int baseLevel, IEnumerable<MonsterFeature> monsterFeatures) 
        : this(name, baseLevel, new FeaturesBundle(monsterFeatures)) {}
    
    public Encounter(string name, int baseLevel, FeaturesBundle featuresBundle)
    {
        Name = name;
        Level = baseLevel;
        _featuresBundle = featuresBundle;
        
        var initMonster = new Monster(Level, string.Format(MonsterNamePattern, Name, _lastMonsterIdificatorLetter++),_featuresBundle);
        Monsters = new List<Monster> { initMonster };
    }

    public Monster AddMonster()
    {
        var monster = new Monster(Level, Name, _featuresBundle);
        Monsters.Add(monster);
        return monster;
    }
}