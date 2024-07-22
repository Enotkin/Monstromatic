using System.Collections.Generic;
using Monstromatic.Models;
using Monstromatic.Utils;

namespace Monstromatic.Data.Storages;

public class SettingsStorage : AppBaseDataFileStorageBase<MonstromaticSettings>
{
    public SettingsStorage() : base(StorageHelper.ApplicationSettingsFileName, "DefaultSettings.json")
    { }

    protected override MonstromaticSettings GetDefaultValue()
    {
        return new MonstromaticSettings()
        {
            MonsterQualities = new Dictionary<string, int>()
        };
    }
}