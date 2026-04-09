using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DAL.Services;
using Monstromatic.Data.AppSettingsProvider;
using Monstromatic.Data.FeatureService;
using Monstromatic.Data.Services;
using Monstromatic.Models;
using Monstromatic.Utils;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Monstromatic.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public IProcessHelper ProcessHelper { get; }
    private readonly FeatureController _featureController = new();
    private readonly IAppSettingsProvider _settingsProvider;
    private readonly FeatureService _featureService;
    private readonly ISettingsService SettingsService;

    [Reactive] 
    private string _name;

    [Reactive] 
    private string _selectedQuality;

    [Reactive]
    private IEnumerable<string> _qualities;

    public IEnumerable<FeatureViewModel> Features => GetFeatureViewModels();

    public ReactiveCommand<Unit, Unit> GenerateEncounterCommand { get; }

    public Interaction<EncounterViewModel, Unit> ShowNewMonsterWindow { get; } = new ();
    public Interaction<Unit, Unit> ShowAboutDialog { get; } = new ();
    public Interaction<Unit, bool> ConfirmResetChanges { get; } = new();
        
    public MainWindowViewModel(IAppSettingsProvider settingsProvider, IProcessHelper processHelper, ISettingsService settingsService)
    {
        ProcessHelper = processHelper;
        SettingsService = settingsService;
        _settingsProvider = settingsProvider;
        _featureService = new FeatureService();

        var canGenerateEncounter = this
            .WhenAnyValue(x => x.Name, x => x.SelectedQuality,
                (name, quality) => !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(quality));
            
        GenerateEncounterCommand = ReactiveCommand.CreateFromTask(GenerateEncounter, canGenerateEncounter);
        
        Qualities = settingsService.Settings.MonsterQualities.Select(x => x.Key);
    }

    [ReactiveCommand]
    private async Task ResetSettings()
    {
        var result = await ConfirmResetChanges.Handle(Unit.Default);
        if (result)
        {
            _settingsProvider.Reset();
            RefreshControls();
        }
    }

    private async Task GenerateEncounter()
    {
        var baseMonsterLevel = _settingsProvider.Settings.MonsterQualities[SelectedQuality];
        var encounter = new Encounter(Name, baseMonsterLevel, _featureController.CreateFeaturesBundle());
        var encounterViewModel = new EncounterViewModel(encounter);
        await ShowNewMonsterWindow.Handle(encounterViewModel);
    }

    [ReactiveCommand]
    private async Task ShowSettings(string path) => await ProcessHelper.StartNewAndWaitAsync(path);

    [ReactiveCommand]
    private async Task ShowAbout() => await ShowAboutDialog.Handle(Unit.Default);

    private void RefreshControls()
    {
        _settingsProvider.Reload();
        this.RaisePropertyChanged(nameof(Features));
        this.RaisePropertyChanged(nameof(Qualities));
    }

    private IEnumerable<FeatureViewModel> GetFeatureViewModels()
    {
        return _settingsProvider.Features
            .Where(f => f is { IsHidden: false })
            .Select(f => new FeatureViewModel(f, _featureController))
            .OrderBy(f => f.DisplayName);
    }
}