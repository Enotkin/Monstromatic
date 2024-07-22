using System.IO;
using Monstromatic.Data.Interfaces;
using Monstromatic.Extensions;

namespace Monstromatic.Data.Storages;

public class FileBaseDataStorage<T> : IBaseDataStorage<T>
{
    private readonly string _fileName;

    public FileBaseDataStorage(string fileName)
    {
        _fileName = fileName;
    }

    public T Read()
    {
        using var stream = File.OpenRead(_fileName);
        return stream.FromJson<T>();
    }

    public void Save(T data)
    {
        using var stream = File.Open(_fileName, FileMode.Create);
        using var jsonStream = data.ToJson();
        jsonStream.CopyTo(stream);
    }
}