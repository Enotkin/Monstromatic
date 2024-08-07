using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Monstromatic.Data;

public class Resources
{
    private const string FileExtension = ".json";
    
    public const string SettingsFileName = "settings";

    public const string FeaturesFileName = "features";

    public static string SettingsFilePath { get; } =
        AppDomain.CurrentDomain.BaseDirectory + SettingsFileName + FileExtension;

    public static string FeaturesFilePath { get; } = 
        AppDomain.CurrentDomain.BaseDirectory + FeaturesFileName + FileExtension;
    public static string GetData(string fileName)
    {
        var name = Assembly.GetExecutingAssembly().GetName().Name;
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{name}.Data.Resources.{fileName}.json")!;
        using var streamReader = new StreamReader(stream, Encoding.UTF8);
        return streamReader.ReadToEnd();
    }
}