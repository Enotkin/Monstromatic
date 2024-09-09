using System.Collections.Generic;
using System.Linq;
using DynamicData.Kernel;
using Monstromatic.Data.AppSettingsProvider;
using Monstromatic.Utils;

namespace Monstromatic.Models;

public class FeaturesBundle
{
    private readonly List<MonsterFeature> _features;
    private const double BaseModificator = 1.0;

    public FeaturesBundle(IEnumerable<MonsterFeature> features)
    {
        var appSettingsProvider = ServiceHub.Default.ServiceProvider.Get<IAppSettingsProvider>();
        var defaultModifiers = appSettingsProvider.Settings.DefaultModifiers;
        
        _features = features.AsList();
        
        LevelModificator = (int)_features.Sum(f => f.LevelModifier);

        AttackModificator = defaultModifiers.AttackModifier + (_features.Any(f => f.AttackModifier != 0)
            ? _features.Where(f => f.AttackModifier != 0)
                .Select(f => f.AttackModifier).Sum()
            : 0);

        DefenceModificator = defaultModifiers.DefenceModifier + (_features.Any(f => f.DefenceModifier != 0)
            ? _features.Where(f => f.DefenceModifier != 0)
                .Select(f => f.DefenceModifier).Sum()
            : 0);
        
        HealthModificator = defaultModifiers.HealthModifier + (_features.Any(f => f.HealthModifier != 0)
            ? _features.Where(f => f.HealthModifier != 0)
                .Select(f => f.HealthModifier).Sum()
            : 0);
        
        KnowledgeModificator = defaultModifiers.KnowledgeModifier + (_features.Any(f => f.KnowledgeModifier != 0)
            ? _features.Where(f => f.KnowledgeModifier != 0)
                .Select(f => f.KnowledgeModifier).Sum()
            : 0);
        
        TemperModificator = defaultModifiers.TemperModifier + (_features.Any(f => f.TemperModifier != 0)
            ? _features.Where(f => f.TemperModifier != 0)
                .Select(f => f.TemperModifier).Sum()
            : 0);
        
        TrickeryModificator = defaultModifiers.TrickeryModifier + (_features.Any(f => f.TrickeryModifier != 0)
            ? _features.Where(f => f.TrickeryModifier != 0)
                .Select(f => f.TrickeryModifier).Sum()
            : 0);
    }

    public int LevelModificator { get; }
    public int AttackModificator { get; }
    public int DefenceModificator { get; }
    public int HealthModificator { get; }
    public int KnowledgeModificator { get; }
    public int TemperModificator { get; }
    public int TrickeryModificator { get; }

    public IReadOnlyCollection<MonsterFeature> Features => _features;
}