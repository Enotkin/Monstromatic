using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Monstromatic.Data.Services;

public class BaseFileStorage<T>
{
    private readonly string _fileName;
    private string _filePath = string.Empty;

    protected T Value { get; private set; } = default!;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    protected BaseFileStorage(string directory, string fileName)
    {
        _fileName = fileName;
        UseDirectory(directory);
    }

    /// <summary>
    /// Переключает хранилище на папку другого профиля и перечитывает данные.
    /// </summary>
    public void UseDirectory(string directory)
    {
        _filePath = Resources.GetFilePath(directory, _fileName);
        Reload();
    }

    public void Reload()
    {
        if (!File.Exists(_filePath)) 
            CreateDefaultFile();

        using var stream = File.OpenRead(_filePath);

        try
        {
            Value = JsonSerializer.Deserialize<T>(stream, _jsonOptions)!;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    private void CreateDefaultFile()
    {
        var defaultData = Resources.GetData(_fileName);

        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var inputStream = File.Create(_filePath);
        inputStream.Write(Encoding.UTF8.GetBytes(defaultData));
    }
    
    public void ResetToDefault()
    {
        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }
        CreateDefaultFile();
    }

    protected void Save(T value)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var temporaryFilePath = _filePath + ".tmp";
        using (var stream = File.Create(temporaryFilePath))
        {
            JsonSerializer.Serialize(stream, value, _jsonOptions);
        }

        File.Move(temporaryFilePath, _filePath, true);
        Value = value;
    }
}
