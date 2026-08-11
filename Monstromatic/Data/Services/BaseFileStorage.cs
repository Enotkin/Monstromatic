using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Monstromatic.Data.Services;

public class BaseFileStorage<T>
{
    private readonly string _defaultFilePath;
    private readonly string _filename;
    private const string Template = "{0}{1}.json";

    protected T Value { get; private set; }

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    protected BaseFileStorage(string fileName)
    {
        _defaultFilePath = string.Format(Template, AppDomain.CurrentDomain.BaseDirectory, fileName);
        _filename = fileName;
        Reload();
    }
    
    public void Reload()
    {
        if (!File.Exists(_defaultFilePath)) 
            CreateFeatureFile();

        using var stream = File.OpenRead(_defaultFilePath);

        try
        {
            Value = JsonSerializer.Deserialize<T>(stream, _jsonOptions)!;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    private void CreateFeatureFile()
    {
        var defaultData = Resources.GetData(_filename);

        using var inputStream = File.Create(_defaultFilePath);
        inputStream.Write(Encoding.UTF8.GetBytes(defaultData));
    }
    
    public void ResetToDefault()
    {
        if (File.Exists(_defaultFilePath))
        {
            File.Delete(_defaultFilePath);
        }
        CreateFeatureFile();
    }

    protected void Save(T value)
    {
        var temporaryFilePath = _defaultFilePath + ".tmp";
        using (var stream = File.Create(temporaryFilePath))
        {
            JsonSerializer.Serialize(stream, value, _jsonOptions);
        }

        File.Move(temporaryFilePath, _defaultFilePath, true);
        Value = value;
    }
}
