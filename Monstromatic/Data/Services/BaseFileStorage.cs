using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Monstromatic.Data.Services;

public class BaseFileStorage<T>
{
    private readonly string _defaultFeatureFilePath;

    protected T Value { get; private set; }

    protected BaseFileStorage(string fileName)
    {
        _defaultFeatureFilePath = AppDomain.CurrentDomain.BaseDirectory + fileName;
        Reload();
    }
    
    public void Reload()
    {
        if (!File.Exists(_defaultFeatureFilePath)) 
            CreateFeatureFile();

        using var stream = File.OpenRead(_defaultFeatureFilePath);
        Value = JsonSerializer.Deserialize<T>(stream);
    }
    
    private void CreateFeatureFile()
    {
        var defaultData = Resources.Resources.DefaultFeaturesData;

        using var inputStream = File.Create(_defaultFeatureFilePath);
        inputStream.Write(Encoding.UTF8.GetBytes(defaultData));
    }
    
    public void ResetToDefault()
    {
        if (File.Exists(_defaultFeatureFilePath))
        {
            File.Delete(_defaultFeatureFilePath);
        }
        CreateFeatureFile();
    }
}