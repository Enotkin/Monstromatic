using System.Collections.Generic;
using Monstromatic.Models;

namespace Monstromatic.Data.Features;

public interface IFeatureService
{
    public IReadOnlyCollection<MonsterFeature> Features { get; }

    public void Reload();

    public void ResetToDefault();
}