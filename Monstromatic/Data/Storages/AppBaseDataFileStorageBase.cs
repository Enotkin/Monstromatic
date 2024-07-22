using System;
using System.IO;
using System.Reflection;
using Monstromatic.Data.Interfaces;
using Monstromatic.Extensions;
using Monstromatic.Utils;

namespace Monstromatic.Data.Storages;

public abstract class AppBaseDataFileStorageBase<T> : IAppBaseDataStorage<T>
{
    private readonly string _resourceName;
    private readonly IBaseDataStorage<T> _baseDataStorage;

    protected AppBaseDataFileStorageBase(string fileName, string resourceName)
    {
        _resourceName = resourceName;
        _baseDataStorage = new FileBaseDataStorage<T>(fileName);

        var directory = Path.GetDirectoryName(Path.GetFullPath(fileName));
            
        if (directory != null && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        if (!File.Exists(fileName))
            ResetToDefault();
    }

    private T GetDefaultSettings()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(typeof(SettingsStorage), _resourceName) ??
                               throw new InvalidOperationException("Cannot open resources stream");
            return stream.FromJson<T>();
        }
        catch(Exception e)
        {
            throw new AppException("Failed to load settings", e);
        }
    }

    public T Read()
    {
        try
        {
            return _baseDataStorage.Read();
        }
        catch
        {
            return GetDefaultValue();
        }
    }

    protected abstract T GetDefaultValue();

    public void Save(T data)
    {
        _baseDataStorage.Save(data);
    }

    public void ResetToDefault()
    {
        _baseDataStorage.Save(data: GetDefaultSettings());
    }
}