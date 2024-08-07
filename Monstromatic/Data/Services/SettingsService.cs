using Monstromatic.Models;

namespace Monstromatic.Data.Services;

public class SettingsService() : BaseFileStorage<MonstromaticSettings>("settings.json")
{
    public MonstromaticSettings Settings => Value;
}