using System;
using System.IO;

namespace Monstromatic.Utils;

public class StorageHelper
{
    public static string ConfigurationDirectory { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Monstromatic");
    public static string ApplicationSettingsFileName { get; } = Path.Combine(ConfigurationDirectory, "settings.json");
    public static string FeaturesFileName { get;  } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "features.json");
}