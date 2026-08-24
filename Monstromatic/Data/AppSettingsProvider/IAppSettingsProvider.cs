using System.Collections.Generic;
using Monstromatic.Models;

namespace Monstromatic.Data.AppSettingsProvider;

public interface IAppSettingsProvider
{
    MonstromaticSettings Settings { get; }
    IEnumerable<MonsterFeature> Features { get; }
    void AddFeature(MonsterFeature feature);
    void RemoveFeatures(IEnumerable<MonsterFeature> features);
    void Reload();
    void Reset();
}
