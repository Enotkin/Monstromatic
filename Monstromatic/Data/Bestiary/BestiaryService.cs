using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using Monstromatic.Data.Profiles;
using Monstromatic.Models;

namespace Monstromatic.Data.Bestiary;

public class BestiaryService : IBestiaryService
{
    private readonly ObservableCollection<BestiaryEntry> _entries;
    private string _filePath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public BestiaryService(IProfileService profileService)
        : this(GetBestiaryFilePath(profileService.CurrentProfileDirectory))
    {
    }

    public BestiaryService(string filePath)
    {
        _filePath = filePath;
        _entries = new ObservableCollection<BestiaryEntry>(Load());
        Entries = new ReadOnlyObservableCollection<BestiaryEntry>(_entries);
    }

    public ReadOnlyObservableCollection<BestiaryEntry> Entries { get; }

    public void UseProfile(string directory)
    {
        _filePath = GetBestiaryFilePath(directory);

        _entries.Clear();
        foreach (var entry in Load())
        {
            _entries.Add(entry);
        }
    }

    public bool TryAdd(Encounter encounter)
    {
        if (_entries.Any(entry => HasSameName(entry.Name, encounter.Name)))
        {
            return false;
        }

        _entries.Add(BestiaryEntry.FromEncounter(encounter));
        Save();
        return true;
    }

    public bool Remove(BestiaryEntry entry)
    {
        if (!_entries.Remove(entry))
        {
            return false;
        }

        Save();
        return true;
    }

    private IReadOnlyCollection<BestiaryEntry> Load()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        try
        {
            using var stream = File.OpenRead(_filePath);
            return JsonSerializer.Deserialize<List<BestiaryEntry>>(stream, _jsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
        catch (IOException)
        {
            return [];
        }
    }

    private void Save()
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var temporaryFilePath = _filePath + ".tmp";
        using (var stream = File.Create(temporaryFilePath))
        {
            JsonSerializer.Serialize(stream, _entries, _jsonOptions);
        }

        File.Move(temporaryFilePath, _filePath, true);
    }

    private static string GetBestiaryFilePath(string directory) =>
        Resources.GetFilePath(directory, Resources.BestiaryFileName);

    private static bool HasSameName(string firstName, string secondName) =>
        string.Equals(firstName.Trim(), secondName.Trim(), StringComparison.CurrentCultureIgnoreCase);
}
