using System.Collections.Generic;

namespace Monstromatic.ViewModels;

public class FeatureCategoryViewModel
{
    public FeatureCategoryViewModel(string displayName, IReadOnlyCollection<FeatureViewModel> features)
    {
        DisplayName = displayName;
        Features = features;
    }

    public string DisplayName { get; }

    public IReadOnlyCollection<FeatureViewModel> Features { get; }
}
