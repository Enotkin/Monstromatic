using Monstromatic.Models;

namespace Monstromatic.Data.Services;

public class SettingsService() : BaseFileStorage<MonstromaticSettings>(Resources.SettingsFileName)
{
    public MonstromaticSettings Settings => Value;
}