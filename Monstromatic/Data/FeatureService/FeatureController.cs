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

    /// <summary>
    /// Приводит выбор пользователя в соответствие с текущим списком особенностей:
    /// исчезнувшие убирает, а изменённые заменяет свежими экземплярами. Без этого
    /// после правки скиллов в выборе остались бы особенности со старыми тегами.
    /// </summary>
    public void Resynchronize(IEnumerable<MonsterFeature> features)
    {
        var actualFeatures = features.ToDictionary(feature => feature.Key);
        var selected = SelectedFeatures.Items
            .Where(feature => actualFeatures.ContainsKey(feature.Key))
            .Select(feature => actualFeatures[feature.Key])
            .ToArray();

        SelectedFeatures.Edit(list =>
        {
            list.Clear();
            list.AddRange(selected);
        });
    }

    public void Clear()
    {
        SelectedFeatures.Clear();
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