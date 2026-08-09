using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Monstromatic.Data.AppSettingsProvider;
using Monstromatic.Data.Bestiary;
using Monstromatic.Data.FeatureService;
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
    private readonly IBestiaryService _bestiaryService;
    private IReadOnlyCollection<FeatureCategoryViewModel> _featureCategories = [];

    [Reactive] private string? _name;
    [Reactive] private string? _selectedQuality;
    [Reactive] private BestiaryEntry? _selectedBestiaryEntry;
    [Reactive] private string? _bestiarySearchText;

    public MainWindowViewModel(
        IAppSettingsProvider settingsProvider,
        IBestiaryService bestiaryService,
        IProcessHelper processHelper)
    {
        ProcessHelper = processHelper;
        _settingsProvider = settingsProvider;
        _bestiaryService = bestiaryService;

        ResolveMissingQualityNames();
        ((INotifyCollectionChanged)_bestiaryService.Entries).CollectionChanged += (_, _) =>
        {
            ResolveMissingQualityNames();
            this.RaisePropertyChanged(nameof(BestiaryEntries));
        };

        this.WhenAnyValue(x => x.BestiarySearchText).Subscribe(_ =>
        {
            if (SelectedBestiaryEntry is not null && !MatchesBestiarySearch(SelectedBestiaryEntry))
            {
                SelectedBestiaryEntry = null;
            }

            this.RaisePropertyChanged(nameof(BestiaryEntries));
        });

        var canGenerateEncounter = this
            .WhenAnyValue(x => x.Name, x => x.SelectedQuality,
                (name, quality) => !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(quality));

        GenerateEncounterCommand = ReactiveCommand.CreateFromTask(GenerateNewEncounter, canGenerateEncounter);
        ShowAboutCommand = ReactiveCommand.CreateFromTask(async () => await ShowAboutDialog.Handle(Unit.Default));
        ShowSettingsCommand = ReactiveCommand.CreateFromTask<string>(ShowSettings);
        ResetSettingsCommand = ReactiveCommand.CreateFromTask(ResetSettings);

        var hasSelectedBestiaryEntry = this
            .WhenAnyValue(x => x.SelectedBestiaryEntry)
            .Select(entry => entry is not null);

        SelectBestiaryEntryCommand = ReactiveCommand.CreateFromTask(
            SelectBestiaryEntry,
            hasSelectedBestiaryEntry);
        DeleteBestiaryEntryCommand = ReactiveCommand.Create(
            DeleteBestiaryEntry,
            hasSelectedBestiaryEntry);

        RebuildFeatureCategories();
    }

    public IReadOnlyCollection<FeatureCategoryViewModel> FeatureCategories
    {
        get => _featureCategories;
        private set => this.RaiseAndSetIfChanged(ref _featureCategories, value);
    }

    public IEnumerable<string> Qualities => _settingsProvider.Settings.MonsterQualities.Select(x => x.Key);

    public IEnumerable<BestiaryEntry> BestiaryEntries => _bestiaryService.Entries
        .Where(MatchesBestiarySearch)
        .OrderBy(entry => entry.Name, StringComparer.CurrentCultureIgnoreCase)
        .ToArray();

    public ReactiveCommand<Unit, Unit> GenerateEncounterCommand { get; }

    public ReactiveCommand<Unit, Unit> ShowAboutCommand { get; }

    public ReactiveCommand<string, Unit> ShowSettingsCommand { get; }

    public ReactiveCommand<Unit, Unit> ResetSettingsCommand { get; }

    public ReactiveCommand<Unit, Unit> SelectBestiaryEntryCommand { get; }

    public ReactiveCommand<Unit, Unit> DeleteBestiaryEntryCommand { get; }

    public Interaction<EncounterViewModel, Unit> ShowNewMonsterWindow { get; } = new();

    public Interaction<Unit, Unit> ShowAboutDialog { get; } = new();

    public Interaction<Unit, bool> ConfirmResetChanges { get; } = new();

    private async Task ResetSettings()
    {
        var result = await ConfirmResetChanges.Handle(Unit.Default);
        if (result)
        {
            _settingsProvider.Reset();
            RefreshControls();
        }
    }

    private async Task GenerateNewEncounter()
    {
        var baseMonsterLevel = _settingsProvider.Settings.MonsterQualities[SelectedQuality!];
        var encounter = new Encounter(
            Name!,
            baseMonsterLevel,
            _featureController.CreateFeaturesBundle(),
            SelectedQuality);
        var encounterViewModel = new EncounterViewModel(encounter, _bestiaryService);
        await ShowNewMonsterWindow.Handle(encounterViewModel);
    }

    private async Task SelectBestiaryEntry()
    {
        if (SelectedBestiaryEntry is null)
        {
            return;
        }

        var encounter = SelectedBestiaryEntry.CreateEncounter();
        var encounterViewModel = new EncounterViewModel(
            encounter,
            _bestiaryService,
            canAddToBestiary: false);
        await ShowNewMonsterWindow.Handle(encounterViewModel);
    }

    private void DeleteBestiaryEntry()
    {
        if (SelectedBestiaryEntry is null)
        {
            return;
        }

        _bestiaryService.Remove(SelectedBestiaryEntry);
        SelectedBestiaryEntry = null;
    }

    private async Task ShowSettings(string path)
    {
        await ProcessHelper.StartNewAndWaitAsync(path);
        RefreshControls();
    }

    private void RefreshControls()
    {
        _settingsProvider.Reload();
        ResolveMissingQualityNames();
        RebuildFeatureCategories();
        this.RaisePropertyChanged(nameof(Qualities));
        this.RaisePropertyChanged(nameof(BestiaryEntries));
    }

    private bool MatchesBestiarySearch(BestiaryEntry entry)
    {
        return string.IsNullOrWhiteSpace(BestiarySearchText)
               || entry.Name.Contains(BestiarySearchText.Trim(), StringComparison.CurrentCultureIgnoreCase);
    }

    private void ResolveMissingQualityNames()
    {
        foreach (var entry in _bestiaryService.Entries.Where(entry => string.IsNullOrWhiteSpace(entry.QualityName)))
        {
            var baseLevel = entry.Level - entry.Features.Sum(feature => feature.LevelModifier);
            var qualityName = _settingsProvider.Settings.MonsterQualities
                .Where(quality => quality.Value == baseLevel)
                .Select(quality => quality.Key)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(qualityName))
            {
                entry.QualityName = qualityName;
            }
        }
    }

    private void RebuildFeatureCategories()
    {
        var features = GetFeatureViewModels().ToArray();
        var categories = new List<FeatureCategoryViewModel>
        {
            new("Все особенности", features)
        };

        categories.Add(new(
            "Уровень",
            features.Where(feature => feature.Feature.LevelModifier != 0).ToArray()));

        categories.AddRange(_settingsProvider.Settings.SkillDefinitions.Select(skill =>
            new FeatureCategoryViewModel(
                skill.Name,
                features.Where(feature => feature.Feature.HasSkillModifier(skill.Tag)).ToArray())));

        FeatureCategories = categories;
    }

    private IEnumerable<FeatureViewModel> GetFeatureViewModels()
    {
        return _settingsProvider.Features
            .Where(f => !f.IsHidden)
            .Select(f => new FeatureViewModel(f, _featureController))
            .OrderBy(f => f.DisplayName);
    }
}
