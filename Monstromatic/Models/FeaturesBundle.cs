using System;
using System.Collections.Generic;
using System.Linq;
using DynamicData.Kernel;

namespace Monstromatic.Models;

public class FeaturesBundle
{
    private readonly List<MonsterFeature> _features;
    private const double BaseModificator = 1.0;
    
    public FeaturesBundle(IEnumerable<MonsterFeature> features)
    {
        _features = features.AsList();
        
        LevelModificator = _features.Sum(f => f.LevelModifier);

        if (_features.Any(f => f.AttackModifier != 0))
        {
            AttackModificator = BaseModificator + _features.Where(f => f.AttackModifier != 0)
                .Select(f => f.AttackModifier - BaseModificator).Sum();
        }

        if (_features.Any(f => f.DefenceModifier != 0))
        {
            DefenceModificator = BaseModificator + _features.Where(f => f.DefenceModifier != 0).Select(f => f.DefenceModifier - BaseModificator).Sum();
        }
        
        if (_features.Any(f => f.HealthModifier != 0))
        {
            HealthModificator = BaseModificator + _features.Where(f => f.HealthModifier != 0).Select(f => f.HealthModifier - BaseModificator).Sum();
        }
        
        if (_features.Any(f => f.PerceptionModifier != 0))
        {
            PerceptionModificator = BaseModificator + _features.Where(f => f.PerceptionModifier != 0).Select(f => f.PerceptionModifier - BaseModificator).Sum();
        }
        
        if (_features.Any(f => f.WillModifier != 0))
        {
            WillModificator = BaseModificator + _features.Where(f => f.WillModifier != 0).Select(f => f.WillModifier - BaseModificator).Sum();
        }
        
        if (_features.Any(f => f.TrickeryModifier != 0))
        {
            TrickeryModificator = BaseModificator + _features.Where(f => f.TrickeryModifier != 0).Select(f => f.TrickeryModifier - BaseModificator).Sum();
        }
    }

    public double LevelModificator { get; }
    public double AttackModificator { get; } = 1.0;
    public double DefenceModificator { get; } = 1.0;
    public double HealthModificator { get; } = 1.0;
    public double PerceptionModificator { get; } = 1.0;
    public double WillModificator { get; } = 1.0;
    public double TrickeryModificator { get; } = 1.0;

    public IReadOnlyCollection<MonsterFeature> Features => _features;
}