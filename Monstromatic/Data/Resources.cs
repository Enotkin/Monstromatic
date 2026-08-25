using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Monstromatic.Data;

public class Resources
{
    private const string FileExtension = ".json";
    private const string ProfilesDirectoryName = "Profiles";

    public const string SettingsFileName = "settings";

    public const string FeaturesFileName = "features";

    public const string BestiaryFileName = "bestiary";

    public const string ProfilesFileName = "profiles";

    public static string BaseDirectory { get; } = AppDomain.CurrentDomain.BaseDirectory;

    /// <summary>Папка со всеми профилями — лежит рядом с программой.</summary>
    public static string ProfilesDirectory { get; } =
        Path.Combine(BaseDirectory, ProfilesDirectoryName);

    public static string ProfilesFilePath { get; } =
        GetFilePath(ProfilesDirectory, ProfilesFileName);

    public static string GetFilePath(string directory, string fileName) =>
        Path.Combine(directory, fileName + FileExtension);

    public static string GetData(string fileName)
    {
        var name = Assembly.GetExecutingAssembly().GetName().Name;
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{name}.Data.Resources.{fileName}.json")!;
        using var streamReader = new StreamReader(stream, Encoding.UTF8);
        return streamReader.ReadToEnd();
    }
}
