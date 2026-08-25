using System.Collections.Generic;

namespace Monstromatic.Models;

/// <summary>
/// Профиль — независимый набор данных: стартовые уровни, скиллы, особенности
/// и бестиарий. Между профилями переключаются при запуске программы.
/// </summary>
public class Profile
{
    /// <summary>
    /// Имя папки профиля. Не меняется при переименовании, чтобы не трогать
    /// файлы на диске.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Содержимое общего файла со списком профилей.
/// </summary>
public class ProfilesRegistry
{
    public List<Profile> Profiles { get; set; } = [];

    /// <summary>
    /// Профиль, выбранный галочкой «Запомнить выбор». Пусто — спрашивать заново.
    /// </summary>
    public string? RememberedProfileId { get; set; }
}
