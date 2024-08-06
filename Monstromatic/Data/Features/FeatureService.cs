using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Monstromatic.Models;

namespace Monstromatic.Data.Features;

public class FeatureService : IFeatureService
{
    private readonly string _defaultFeatureFilePath = AppDomain.CurrentDomain.BaseDirectory + "features.json";
    private MonsterFeature[] _features;
    
    public IReadOnlyCollection<MonsterFeature> Features => _features;

    public FeatureService()
    {
        Reload();
    }
    
    public void Reload()
    {
        if (!File.Exists(_defaultFeatureFilePath)) 
            CreateFeatureFile();

        using var stream = File.OpenRead(_defaultFeatureFilePath);
        _features = JsonSerializer.Deserialize<MonsterFeature[]>(stream);
    }
    
    private void CreateFeatureFile()
    {
        var defaultData = Resources.Resources.DefaultFeaturesData;

        using var inputStream = File.Create(_defaultFeatureFilePath);
        inputStream.Write(Encoding.UTF8.GetBytes(defaultData));
    }
}