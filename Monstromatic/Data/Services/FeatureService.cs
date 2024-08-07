using System.Collections.Generic;
using Monstromatic.Models;

namespace Monstromatic.Data.Services;

public class FeatureService() : BaseFileStorage<MonsterFeature[]>(Resources.FeaturesFileName)
{
    public IReadOnlyCollection<MonsterFeature> Features => Value;
}