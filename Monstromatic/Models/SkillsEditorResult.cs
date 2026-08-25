using System.Collections.Generic;

namespace Monstromatic.Models;

/// <summary>
/// Итог работы редактора скиллов: новый набор скиллов и то, что нужно
/// поправить в особенностях, чтобы они остались рабочими.
/// </summary>
public class SkillsEditorResult
{
    public IReadOnlyCollection<SkillDefinition> Skills { get; init; } = [];

    /// <summary>Старый Tag → новый Tag.</summary>
    public IReadOnlyDictionary<string, string> RenamedTags { get; init; } = new Dictionary<string, string>();

    /// <summary>Теги скиллов, которых больше нет в наборе.</summary>
    public IReadOnlyCollection<string> RemovedTags { get; init; } = [];
}
