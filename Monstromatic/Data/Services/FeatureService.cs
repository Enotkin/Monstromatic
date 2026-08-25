using System.Collections.Generic;
using System.Linq;
using Monstromatic.Models;

namespace Monstromatic.Data.Services;

public class FeatureService(string directory)
    : BaseFileStorage<MonsterFeature[]>(directory, Resources.FeaturesFileName)
{
    public IReadOnlyCollection<MonsterFeature> Features => Value;

    public void Add(MonsterFeature feature)
    {
        Save([..Value, feature]);
    }

    public void Remove(IEnumerable<MonsterFeature> features)
    {
        var removingKeys = features.Select(feature => feature.Key).ToHashSet();
        Save(Value.Where(feature => !removingKeys.Contains(feature.Key)).ToArray());
    }

    public void Replace(IEnumerable<MonsterFeature> features)
    {
        Save(features.ToArray());
    }
}
