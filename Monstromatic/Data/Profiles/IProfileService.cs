using System.Collections.Generic;
using Monstromatic.Models;

namespace Monstromatic.Data.Profiles;

public interface IProfileService
{
    IReadOnlyList<Profile> Profiles { get; }

    /// <summary>Профиль, с которым программа работает прямо сейчас.</summary>
    Profile Current { get; }

    /// <summary>Папка с файлами текущего профиля.</summary>
    string CurrentProfileDirectory { get; }

    /// <summary>Профиль, запомненный галочкой «Запомнить выбор».</summary>
    string? RememberedProfileId { get; }

    /// <summary>
    /// Переносит данные старых версий в профиль и следит, чтобы хотя бы один
    /// профиль существовал. Вызывается один раз при запуске.
    /// </summary>
    void Initialize();

    Profile Create(string name);

    void Rename(Profile profile, string name);

    void Delete(Profile profile);

    void SetCurrent(Profile profile);

    /// <summary>Запомнить профиль для следующего запуска или забыть выбор.</summary>
    void Remember(Profile? profile);

    string GetProfileDirectory(Profile profile);
}
