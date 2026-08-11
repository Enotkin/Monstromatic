using System.Collections.Generic;
using System.Windows.Input;

namespace Monstromatic.ViewModels;

public class FeatureCategoryViewModel
{
    public FeatureCategoryViewModel(
        string displayName,
        IReadOnlyCollection<FeatureViewModel> features,
        bool canManageFeatures = false,
        ICommand? createFeatureCommand = null,
        ICommand? deleteFeaturesCommand = null)
    {
        DisplayName = displayName;
        Features = features;
        CanManageFeatures = canManageFeatures;
        CreateFeatureCommand = createFeatureCommand;
        DeleteFeaturesCommand = deleteFeaturesCommand;
    }

    public string DisplayName { get; }

    public IReadOnlyCollection<FeatureViewModel> Features { get; }

    public bool CanManageFeatures { get; }

    public ICommand? CreateFeatureCommand { get; }

    public ICommand? DeleteFeaturesCommand { get; }
}
