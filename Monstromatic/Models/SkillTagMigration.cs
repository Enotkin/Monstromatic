using System.Collections.Generic;
using System.Linq;

namespace Monstromatic.Models;

/// <summary>
/// Приводит особенности в соответствие с изменённым набором скиллов.
/// Переименование тега чинится автоматически, а особенности, завязанные на
/// удалённый скилл, удаляются — но только после подтверждения пользователем.
/// </summary>
public static class SkillTagMigration
{
    /// <summary>
    /// Особенности, которые перестанут работать при удалении перечисленных тегов.
    /// </summary>
    public static IReadOnlyCollection<MonsterFeature> FindDependentFeatures(
        IEnumerable<MonsterFeature> features,
        IReadOnlyCollection<string> removedTags)
    {
        if (removedTags.Count == 0)
        {
            return [];
        }

        return features
            .Where(feature => removedTags.Any(feature.HasSkillModifier))
            .ToArray();
    }

    /// <summary>
    /// Новый список особенностей: зависящие от удалённых тегов выброшены,
    /// у остальных теги переименованы.
    /// </summary>
    public static IReadOnlyCollection<MonsterFeature> Apply(
        IEnumerable<MonsterFeature> features,
        IReadOnlyDictionary<string, string> renamedTags,
        IReadOnlyCollection<string> removedTags)
    {
        return features
            .Where(feature => !removedTags.Any(feature.HasSkillModifier))
            .Select(feature => Rename(feature, renamedTags))
            .ToArray();
    }

    private static MonsterFeature Rename(
        MonsterFeature feature,
        IReadOnlyDictionary<string, string> renamedTags)
    {
        if (renamedTags.Count == 0)
        {
            return feature;
        }

        var modifiers = feature.GetSkillModifiers();
        if (!modifiers.Any(modifier => renamedTags.ContainsKey(modifier.Tag)))
        {
            return feature;
        }

        var renamed = modifiers
            .Select(modifier => renamedTags.TryGetValue(modifier.Tag, out var newTag)
                ? new SkillModifier { Tag = newTag, Modifier = modifier.Modifier }
                : modifier)
            .ToArray();

        return feature.WithSkillModifiers(renamed);
    }
}
