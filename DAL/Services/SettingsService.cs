using System.Text.Json;
using DAL.Models.Settings;

namespace DAL.Services;

public sealed class SettingsService : ISettingsService
{
    private readonly string _settingsFilePath;
    private readonly Lazy<AppSettings> _cachedSettings;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public SettingsService(string? settingsFilePath = null)
    {
        _settingsFilePath = settingsFilePath ?? Path.Combine(AppContext.BaseDirectory, "settings.json");
        _cachedSettings = new Lazy<AppSettings>(LoadSettings, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public AppSettings Settings => _cachedSettings.Value;

    private AppSettings LoadSettings()
    {
        if (!File.Exists(_settingsFilePath))
        {
            throw new FileNotFoundException($"Settings file was not found: '{_settingsFilePath}'.", _settingsFilePath);
        }

        using var stream = File.OpenRead(_settingsFilePath);
        var settings = JsonSerializer.Deserialize<AppSettings>(stream, JsonOptions);

        return settings ?? 
               throw new InvalidOperationException($"Unable to deserialize settings from '{_settingsFilePath}'.");
    }
}
