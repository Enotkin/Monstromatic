using System;
using Monstromatic.Models;
using Monstromatic.Utils;

namespace Monstromatic.Data.Storages;

public class FeaturesStorage : AppBaseDataFileStorageBase<MonsterFeature[]>
{
    public FeaturesStorage() : base(StorageHelper.FeaturesFileName, "Features.json")
    {
    }

    protected override MonsterFeature[] GetDefaultValue()
    {
        return Array.Empty<MonsterFeature>();
    }
}