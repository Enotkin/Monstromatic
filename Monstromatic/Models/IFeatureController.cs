using System.Collections.Generic;
using DynamicData;

namespace Monstromatic.Models
{
    public interface IFeatureController
    {
        SourceList<MonsterFeature> SelectedFeatures { get; }

        void AddFeature(MonsterFeature feature);

        void RemoveFeature(MonsterFeature feature);

        IEnumerable<MonsterFeature> CreateBundle();
        
        FeaturesBundle CreateFeaturesBundle();
    }
}
