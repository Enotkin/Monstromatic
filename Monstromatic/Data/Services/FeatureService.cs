using System.Collections.Generic;
using Monstromatic.Models;

namespace Monstromatic.Data.Services;

public class FeatureService() : BaseFileStorage<MonsterFeature[]>("features.json")
{
    public IReadOnlyCollection<MonsterFeature> Features => Value;
}