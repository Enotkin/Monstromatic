using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using Monstromatic.Data.FeatureService;
using Monstromatic.Models;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Monstromatic.ViewModels;

public partial class FeatureViewModel : ViewModelBase
{
    public readonly MonsterFeature Feature;
    private readonly IFeatureController _featureController;

    public string Key => Feature.Key;

    public string DisplayName => Feature.DisplayName;
    
    [ObservableAsProperty]
    private bool _isFeatureSelected;

    public FeatureViewModel(MonsterFeature feature, IFeatureController featureController)
    {
        Feature = feature;
        _featureController = featureController;

        var canAddFeature = _featureController.SelectedFeatures
            .Connect()
            .QueryWhenChanged(CanAddFeature);

        canAddFeature = canAddFeature.StartWith(true);

        AddFeatureCommand = ReactiveCommand.Create<bool>(AddFeature, canAddFeature);

        _featureController.SelectedFeatures
            .Connect()
            .QueryWhenChanged(x => x.Contains(Feature))
            .ToProperty(this, x => x.IsFeatureSelected);
    }

    private bool CanAddFeature(IReadOnlyCollection<MonsterFeature> selectedFeatures)
    {
        var containsIncompatibleFeatures = selectedFeatures.Any(
            f => Feature.IncompatibleFeatures.Contains(f));

        return !containsIncompatibleFeatures;
    }

    private void AddFeature(bool isChecked)
    {
        if (isChecked)
        {
            _featureController.AddFeature(Feature);
        }
        else
            _featureController.RemoveFeature(Feature);
    }

    public ReactiveCommand<bool, Unit> AddFeatureCommand { get; }
}
