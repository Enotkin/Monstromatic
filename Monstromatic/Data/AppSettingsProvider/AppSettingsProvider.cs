using System.Collections.Generic;
using Monstromatic.Data.Services;
using Monstromatic.Models;

namespace Monstromatic.Data.AppSettingsProvider;

public class AppSettingsProvider : IAppSettingsProvider
{
    private readonly Services.FeatureService _featureService;
    private readonly SettingsService _settingsService;

    public MonstromaticSettings Settings => _settingsService.Settings;
    public IEnumerable<MonsterFeature> Features => _featureService.Features;

    public AppSettingsProvider()
    {
        _featureService = new Services.FeatureService();
        _settingsService = new SettingsService();
        
        Reload();
    }

    public void Reload()
    {
        _settingsService.Reload();
        _featureService.Reload();
    }

    public void Reset()
    {
        _settingsService.ResetToDefault();
        _featureService.ResetToDefault();
        
        Reload();
    }
}