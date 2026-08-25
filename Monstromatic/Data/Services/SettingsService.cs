using Monstromatic.Models;

namespace Monstromatic.Data.Services;

public class SettingsService(string directory)
    : BaseFileStorage<MonstromaticSettings>(directory, Resources.SettingsFileName)
{
    public MonstromaticSettings Settings => Value;

    public void SaveSettings(MonstromaticSettings settings)
    {
        // Устаревшее поле подменяет собой явный список скиллов, поэтому при
        // сохранении из редактора его нужно убрать.
        settings.DefaultModifiers = null;
        Save(settings);
    }
}
