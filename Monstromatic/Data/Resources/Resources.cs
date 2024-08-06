using System.IO;
using System.Reflection;
using System.Text;

namespace Monstromatic.Data.Resources;

public static class Resources
{
    public static string DefaultFeaturesData => GetData("DefaultFeatures");

    public static string DefaultSettingsData => GetData("DefaultSettings");

    private static string GetData(string fileName)
    {
        var name = Assembly.GetExecutingAssembly().GetName().Name;
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{name}.Data.Resources.{fileName}.json")!;
         using var streamReader = new StreamReader(stream, Encoding.UTF8);
        return streamReader.ReadToEnd();
    }
}