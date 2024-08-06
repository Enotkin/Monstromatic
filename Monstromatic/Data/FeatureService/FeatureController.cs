using System.Collections.Generic;
using System.Linq;
using DynamicData;
using Monstromatic.Models;
using Monstromatic.Utils;

namespace Monstromatic.Data.FeatureService;

public class FeatureController : IFeatureController
{
    public SourceList<MonsterFeature> SelectedFeatures { get; } = new();
        
    public void AddFeature(MonsterFeature feature)
    {
        SelectedFeatures.AddOnce(feature);

        foreach (var includedFeature in feature.IncludedFeatures)
        {
            SelectedFeatures.AddOnce(includedFeature);
        }
    }

    public void RemoveFeature(MonsterFeature feature)
    {
        SelectedFeatures.Remove(feature);
    }

    public IEnumerable<MonsterFeature> CreateBundle()
    {
        var mutexes = SelectedFeatures.Items.SelectMany(f => f.ExcludedFeatures);
        return SelectedFeatures.Items.Except(mutexes);
    }

    public FeaturesBundle CreateFeaturesBundle()
    {
        return new FeaturesBundle(CreateBundle());
    }
}