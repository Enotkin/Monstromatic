using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using Monstromatic.Models;

namespace Monstromatic.Data.Profiles;

public class ProfileService : IProfileService
{
    private const string LegacyProfileId = "default";
    private const string LegacyProfileName = "Основной";
    private const string FirstProfileName = "Мой профиль";

    private static readonly string[] ProfileFileNames =
    [
        Resources.SettingsFileName,
        Resources.FeaturesFileName,
        Resources.BestiaryFileName
    ];

    private readonly string _profilesDirectory;
    private readonly string _registryFilePath;
    private readonly string _legacyDirectory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private ProfilesRegistry _registry = new();
    private Profile? _current;

    public ProfileService()
        : this(Resources.ProfilesDirectory, Resources.ProfilesFilePath, Resources.BaseDirectory)
    {
    }

    /// <param name="legacyDirectory">
    /// Где лежат файлы старых версий программы — оттуда данные переносятся
    /// в первый профиль.
    /// </param>
    public ProfileService(string profilesDirectory, string registryFilePath, string legacyDirectory)
    {
        _profilesDirectory = profilesDirectory;
        _registryFilePath = registryFilePath;
        _legacyDirectory = legacyDirectory;
    }

    public IReadOnlyList<Profile> Profiles => _registry.Profiles;

    public Profile Current => _current
        ?? throw new InvalidOperationException("Профиль ещё не выбран.");

    public string CurrentProfileDirectory => GetProfileDirectory(Current);

    public string? RememberedProfileId => _registry.RememberedProfileId;

    public void Initialize()
    {
        Directory.CreateDirectory(_profilesDirectory);
        _registry = LoadRegistry();

        if (_registry.Profiles.Count == 0)
        {
            CreateStartingProfile();
        }

        // Запомненный профиль мог быть удалён вручную — тогда спрашиваем заново.
        if (_registry.RememberedProfileId is not null &&
            _registry.Profiles.All(profile => profile.Id != _registry.RememberedProfileId))
        {
            _registry.RememberedProfileId = null;
            SaveRegistry();
        }

        _current ??= _registry.Profiles[0];
    }

    public Profile Create(string name)
    {
        var profile = new Profile
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = name.Trim()
        };

        Directory.CreateDirectory(GetProfileDirectory(profile));
        _registry.Profiles.Add(profile);
        SaveRegistry();

        return profile;
    }

    public void Rename(Profile profile, string name)
    {
        profile.Name = name.Trim();
        SaveRegistry();
    }

    public void Delete(Profile profile)
    {
        var directory = GetProfileDirectory(profile);
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }

        _registry.Profiles.RemoveAll(item => item.Id == profile.Id);

        if (_registry.RememberedProfileId == profile.Id)
        {
            _registry.RememberedProfileId = null;
        }

        SaveRegistry();

        if (_current?.Id == profile.Id)
        {
            _current = _registry.Profiles.FirstOrDefault();
        }
    }

    public void SetCurrent(Profile profile)
    {
        _current = profile;
    }

    public void Remember(Profile? profile)
    {
        _registry.RememberedProfileId = profile?.Id;
        SaveRegistry();
    }

    public string GetProfileDirectory(Profile profile) =>
        Path.Combine(_profilesDirectory, profile.Id);

    /// <summary>
    /// Первый запуск после обновления: данные лежали рядом с программой без
    /// профилей. Копируем их в профиль «Основной», оригиналы оставляем на месте
    /// как страховочную копию.
    /// </summary>
    private void CreateStartingProfile()
    {
        var legacyFiles = ProfileFileNames
            .Select(fileName => Resources.GetFilePath(_legacyDirectory, fileName))
            .Where(File.Exists)
            .ToArray();

        var profile = new Profile
        {
            Id = legacyFiles.Length > 0 ? LegacyProfileId : Guid.NewGuid().ToString("N"),
            Name = legacyFiles.Length > 0 ? LegacyProfileName : FirstProfileName
        };

        var directory = GetProfileDirectory(profile);
        Directory.CreateDirectory(directory);

        foreach (var legacyFile in legacyFiles)
        {
            var target = Path.Combine(directory, Path.GetFileName(legacyFile));
            if (!File.Exists(target))
            {
                File.Copy(legacyFile, target);
            }
        }

        _registry.Profiles.Add(profile);
        SaveRegistry();
    }

    private ProfilesRegistry LoadRegistry()
    {
        if (!File.Exists(_registryFilePath))
        {
            return new ProfilesRegistry();
        }

        try
        {
            using var stream = File.OpenRead(_registryFilePath);
            return JsonSerializer.Deserialize<ProfilesRegistry>(stream, _jsonOptions)
                   ?? new ProfilesRegistry();
        }
        catch (JsonException)
        {
            return new ProfilesRegistry();
        }
        catch (IOException)
        {
            return new ProfilesRegistry();
        }
    }

    private void SaveRegistry()
    {
        Directory.CreateDirectory(_profilesDirectory);

        var temporaryFilePath = _registryFilePath + ".tmp";
        using (var stream = File.Create(temporaryFilePath))
        {
            JsonSerializer.Serialize(stream, _registry, _jsonOptions);
        }

        File.Move(temporaryFilePath, _registryFilePath, true);
    }
}
