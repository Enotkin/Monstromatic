using System.Collections.Generic;
using Monstromatic.Models;

namespace Monstromatic.Data.AppSettingsProvider;

public interface IAppSettingsProvider
{
    MonstromaticSettings Settings { get; }
    IEnumerable<MonsterFeature> Features { get; }
    void AddFeature(MonsterFeature feature);
    void RemoveFeatures(IEnumerable<MonsterFeature> features);
    /// <summary>Сохранить набор стартовых уровней из редактора.</summary>
    void ApplyQualities(Dictionary<string, int> qualities);

    /// <summary>
    /// Сохранить набор скиллов из редактора и привести особенности в
    /// соответствие с переименованными и удалёнными тегами.
    /// </summary>
    void ApplySkills(SkillsEditorResult result);
    /// <summary>Переключить данные на другой профиль.</summary>
    void UseProfile(string directory);
    void Reload();
    void Reset();
}
