using System.Collections.Generic;
using DynamicData;
using Monstromatic.Models;

namespace Monstromatic.Data.FeatureService;

public interface IFeatureController
{
    SourceList<MonsterFeature> SelectedFeatures { get; }

    void AddFeature(MonsterFeature feature);

    void RemoveFeature(MonsterFeature feature);

    void Resynchronize(IEnumerable<MonsterFeature> features);

    /// <summary>Снять выбор со всех особенностей.</summary>
    void Clear();

    IEnumerable<MonsterFeature> CreateBundle();
        
    FeaturesBundle CreateFeaturesBundle();
}