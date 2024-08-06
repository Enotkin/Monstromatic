using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using JetBrains.Annotations;
using Monstromatic.Data.FeatureService;
using Monstromatic.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Monstromatic.ViewModels;

public class FeatureViewModel : ViewModelBase
{
    private readonly MonsterFeature _feature;
    private readonly IFeatureController _featureController;

    public string Key => _feature.Key;

    public string DisplayName => _feature.DisplayName;

    [UsedImplicitly] 
    public bool IsFeatureSelected { [ObservableAsProperty] get; }

    public FeatureViewModel(MonsterFeature feature, IFeatureController featureController)
    {
        _feature = feature;
        _featureController = featureController;

        var canAddFeature = _featureController.SelectedFeatures
            .Connect()
            .QueryWhenChanged(CanAddFeature);

        canAddFeature = canAddFeature.StartWith(true);

        AddFeatureCommand = ReactiveCommand.Create<bool>(AddFeature, canAddFeature);

        _featureController.SelectedFeatures
            .Connect()
            .QueryWhenChanged(x => x.Contains(_feature))
            .ToPropertyEx(this, x => x.IsFeatureSelected);
    }

    private bool CanAddFeature(IReadOnlyCollection<MonsterFeature> selectedFeatures)
    {
        var containsIncompatibleFeatures = selectedFeatures.Any(
            f => _feature.IncompatibleFeatures.Contains(f));

        return !containsIncompatibleFeatures;
    }

    private void AddFeature(bool isChecked)
    {
        if (isChecked)
        {
            _featureController.AddFeature(_feature);
        }
        else
            _featureController.RemoveFeature(_feature);
    }

    public ReactiveCommand<bool, Unit> AddFeatureCommand { get; }
}