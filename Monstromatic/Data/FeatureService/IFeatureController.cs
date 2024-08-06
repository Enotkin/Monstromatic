using System.Collections.Generic;
using DynamicData;
using Monstromatic.Models;

namespace Monstromatic.Data.FeatureService;

public interface IFeatureController
{
    SourceList<MonsterFeature> SelectedFeatures { get; }

    void AddFeature(MonsterFeature feature);

    void RemoveFeature(MonsterFeature feature);

    IEnumerable<MonsterFeature> CreateBundle();
        
    FeaturesBundle CreateFeaturesBundle();
}