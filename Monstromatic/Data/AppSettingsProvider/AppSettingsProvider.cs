using System.Collections.Generic;
using Monstromatic.Data.Profiles;
using Monstromatic.Data.Services;
using Monstromatic.Models;

namespace Monstromatic.Data.AppSettingsProvider;

public class AppSettingsProvider : IAppSettingsProvider
{
    private readonly Services.FeatureService _featureService;
    private readonly SettingsService _settingsService;

    public MonstromaticSettings Settings => _settingsService.Settings;
    public IEnumerable<MonsterFeature> Features => _featureService.Features;

    public AppSettingsProvider(IProfileService profileService)
    {
        var directory = profileService.CurrentProfileDirectory;
        _featureService = new Services.FeatureService(directory);
        _settingsService = new SettingsService(directory);
    }

    public void UseProfile(string directory)
    {
        _settingsService.UseDirectory(directory);
        _featureService.UseDirectory(directory);
    }

    public void Reload()
    {
        _settingsService.Reload();
        _featureService.Reload();
    }

    public void AddFeature(MonsterFeature feature)
    {
        _featureService.Add(feature);
    }

    public void RemoveFeatures(IEnumerable<MonsterFeature> features)
    {
        _featureService.Remove(features);
    }

    public void ApplyQualities(Dictionary<string, int> qualities)
    {
        var settings = Settings;
        settings.MonsterQualities = qualities;
        _settingsService.SaveSettings(settings);
    }

    public void ApplySkills(SkillsEditorResult result)
    {
        // Особенности правим раньше настроек: если запись на диск сорвётся,
        // набор скиллов и особенности останутся согласованными.
        if (result.RenamedTags.Count > 0 || result.RemovedTags.Count > 0)
        {
            _featureService.Replace(SkillTagMigration.Apply(
                Features,
                result.RenamedTags,
                result.RemovedTags));
        }

        var settings = Settings;
        settings.Skills = result.Skills;
        _settingsService.SaveSettings(settings);
    }

    public void Reset()
    {
        _settingsService.ResetToDefault();
        _featureService.ResetToDefault();
        
        Reload();
    }
}
