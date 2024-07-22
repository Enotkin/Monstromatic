using System.Collections.Generic;
using System.Linq;
using Monstromatic.Data.Interfaces;
using Monstromatic.Models;

namespace Monstromatic.Data.AppSettingsProvider;

public class AppSettingsProvider : IAppSettingsProvider
{
    private readonly IAppBaseDataStorage<MonstromaticSettings> _settingsStorage;
    private readonly IAppBaseDataStorage<MonsterFeature[]> _featuresStorage;
    
    public MonstromaticSettings Settings { get; private set; }
    public IEnumerable<MonsterFeature> Features { get; private set; }

    public AppSettingsProvider(IAppBaseDataStorage<MonstromaticSettings> settingsStorage, IAppBaseDataStorage<MonsterFeature[]> featuresStorage)
    {
        _settingsStorage = settingsStorage;
        _featuresStorage = featuresStorage;
        Reload();
    }

    public void Reload()
    {
        Settings = _settingsStorage.Read();
        
        var features = _featuresStorage.Read().ToDictionary(feature => feature.Key);
        
        //todo: there is should be an error handling mechanism
        foreach (var feature in features.Values)
        {
            feature.IncludedFeatures = feature.IncludedFeaturesKeys.Select(key => features[key]).ToArray();
            feature.ExcludedFeatures = feature.ExcludedFeaturesKeys.Select(key => features[key]).ToArray();
            feature.IncompatibleFeatures = feature.IncompatibleFeaturesKeys.Select(key => features[key]).ToArray();
        }

        Features = features.Values;
    }

    public void Reset()
    {
        _settingsStorage.ResetToDefault();
        _featuresStorage.ResetToDefault();
        Reload();
    }
}