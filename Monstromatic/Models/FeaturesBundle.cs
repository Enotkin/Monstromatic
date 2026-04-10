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
        
        LevelModificator = _features.Sum(f => f.LevelModifier);

        AttackModificator = defaultModifiers.AttackModifier + (_features.Any(f => f.AttackModifier != 0)
            ? _features.Where(f => f.AttackModifier != 0)
                .Select(f => f.AttackModifier).Sum()
            : 0);

        DefenceModificator = defaultModifiers.DefenceModifier + (_features.Any(f => f.DefenceModifier != 0)
            ? _features.Where(f => f.DefenceModifier != 0)
                .Select(f => f.DefenceModifier).Sum()
            : 0);
        
        BraveryModificator = defaultModifiers.BraveryModifier + (_features.Any(f => f.BraveryModifier != 0)
            ? _features.Where(f => f.BraveryModifier != 0)
                .Select(f => f.BraveryModifier).Sum()
            : 0);
        
        TrickeryModificator = defaultModifiers.TrickeryModifier + (_features.Any(f => f.TrickeryModifier != 0)
            ? _features.Where(f => f.TrickeryModifier != 0)
                .Select(f => f.TrickeryModifier).Sum()
            : 0);
    }

    public int LevelModificator { get; }
    public double AttackModificator { get; }
    public double DefenceModificator { get; }
    public double BraveryModificator { get; }
    public double TrickeryModificator { get; }
    public IReadOnlyCollection<MonsterFeature> Features => _features;
}