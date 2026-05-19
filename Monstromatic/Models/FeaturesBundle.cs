using System;
using System.Collections.Generic;
using System.Linq;
using DynamicData.Kernel;
using Monstromatic.Data.AppSettingsProvider;
using Monstromatic.Utils;

namespace Monstromatic.Models;

public class FeaturesBundle
{
    private readonly List<MonsterFeature> _features;

    public FeaturesBundle(IEnumerable<MonsterFeature> features)
    {
        var appSettingsProvider = ServiceHub.Default.ServiceProvider.Get<IAppSettingsProvider>();
        var defaultModifiers = appSettingsProvider.Settings.DefaultModifiers;

        _features = features.AsList();

        LevelModificator = _features.Sum(f => f.LevelModifier);

        AttackStandardModifier = defaultModifiers.AttackModifier;
        DefenceStandardModifier = defaultModifiers.DefenceModifier;
        BraveryStandardModifier = defaultModifiers.BraveryModifier;
        TrickeryStandardModifier = defaultModifiers.TrickeryModifier;

        AttackFeatureModifiers = GetFeatureModifiers(f => f.AttackModifier);
        DefenceFeatureModifiers = GetFeatureModifiers(f => f.DefenceModifier);
        BraveryFeatureModifiers = GetFeatureModifiers(f => f.BraveryModifier);
        TrickeryFeatureModifiers = GetFeatureModifiers(f => f.TrickeryModifier);
    }

    public int LevelModificator { get; }
    public double AttackStandardModifier { get; }
    public double DefenceStandardModifier { get; }
    public double BraveryStandardModifier { get; }
    public double TrickeryStandardModifier { get; }
    public IReadOnlyCollection<double> AttackFeatureModifiers { get; }
    public IReadOnlyCollection<double> DefenceFeatureModifiers { get; }
    public IReadOnlyCollection<double> BraveryFeatureModifiers { get; }
    public IReadOnlyCollection<double> TrickeryFeatureModifiers { get; }
    public IReadOnlyCollection<MonsterFeature> Features => _features;

    private IReadOnlyCollection<double> GetFeatureModifiers(Func<MonsterFeature, double> selector)
    {
        return _features
            .Select(selector)
            .Where(modifier => modifier != 0)
            .ToArray();
    }
}
