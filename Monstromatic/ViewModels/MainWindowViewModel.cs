using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using Monstromatic.Data.AppSettingsProvider;
using Monstromatic.Data.Bestiary;
using Monstromatic.Data.FeatureService;
using Monstromatic.Data.Profiles;
using Monstromatic.Models;
using Monstromatic.Utils;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Monstromatic.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public IProcessHelper ProcessHelper { get; }

    /// <summary>Нужен окну, чтобы запустить мастер настройки нового профиля.</summary>
    public IAppSettingsProvider SettingsProvider => _settingsProvider;

    /// <summary>Нужен окну, чтобы показать список профилей.</summary>
    public IProfileService ProfileService => _profileService;

    private readonly FeatureController _featureController = new();
    private readonly IAppSettingsProvider _settingsProvider;
    private readonly IBestiaryService _bestiaryService;
    private readonly IProfileService _profileService;
    private IReadOnlyCollection<FeatureCategoryViewModel> _featureCategories = [];

    [Reactive] private string? _name;
    [Reactive] private string? _selectedQuality;
    [Reactive] private BestiaryEntry? _selectedBestiaryEntry;
    [Reactive] private string? _bestiarySearchText;
    [Reactive] private bool _hasRememberedProfile;

    public MainWindowViewModel(
        IAppSettingsProvider settingsProvider,
        IBestiaryService bestiaryService,
        IProfileService profileService,
        IProcessHelper processHelper)
    {
        ProcessHelper = processHelper;
        _settingsProvider = settingsProvider;
        _bestiaryService = bestiaryService;
        _profileService = profileService;
        _hasRememberedProfile = profileService.RememberedProfileId is not null;

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
        EditQualityLevelsCommand = ReactiveCommand.CreateFromTask(EditQualityLevels);
        RenameProfileCommand = ReactiveCommand.CreateFromTask(RenameProfile);
        SwitchProfileCommand = ReactiveCommand.CreateFromTask(SwitchProfile);
        CreateProfileCommand = ReactiveCommand.CreateFromTask(CreateProfile);
        ForgetRememberedProfileCommand = ReactiveCommand.Create(
            ForgetRememberedProfile,
            this.WhenAnyValue(x => x.HasRememberedProfile));
        EditSkillsCommand = ReactiveCommand.CreateFromTask(EditSkills);
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

        var hasSelectedFeatures = _featureController.SelectedFeatures
            .Connect()
            .QueryWhenChanged(features => features.Count > 0)
            .StartWith(false);

        CreateFeatureCommand = ReactiveCommand.CreateFromTask(CreateFeature);
        DeleteFeaturesCommand = ReactiveCommand.CreateFromTask(
            DeleteSelectedFeatures,
            hasSelectedFeatures);

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

    public ReactiveCommand<Unit, Unit> EditQualityLevelsCommand { get; }

    public ReactiveCommand<Unit, Unit> EditSkillsCommand { get; }

    public ReactiveCommand<Unit, Unit> RenameProfileCommand { get; }

    public ReactiveCommand<Unit, Unit> SwitchProfileCommand { get; }

    public ReactiveCommand<Unit, Unit> CreateProfileCommand { get; }

    public ReactiveCommand<Unit, Unit> ForgetRememberedProfileCommand { get; }

    public string ProfileMenuHeader => $"Профиль: {_profileService.Current.Name}";

    public ReactiveCommand<Unit, Unit> ResetSettingsCommand { get; }

    public ReactiveCommand<Unit, Unit> SelectBestiaryEntryCommand { get; }

    public ReactiveCommand<Unit, Unit> DeleteBestiaryEntryCommand { get; }

    public ReactiveCommand<Unit, Unit> CreateFeatureCommand { get; }

    public ReactiveCommand<Unit, Unit> DeleteFeaturesCommand { get; }

    public Interaction<EncounterViewModel, Unit> ShowNewMonsterWindow { get; } = new();

    public Interaction<Unit, Unit> ShowAboutDialog { get; } = new();

    public Interaction<Unit, bool> ConfirmResetChanges { get; } = new();

    public Interaction<CreateFeatureViewModel, MonsterFeature?> ShowCreateFeatureDialog { get; } = new();

    public Interaction<MessageRequest, Unit> ShowMessage { get; } = new();

    public Interaction<QualityLevelsViewModel, Dictionary<string, int>?> ShowQualityLevelsDialog { get; } = new();

    public Interaction<SkillsEditorViewModel, SkillsEditorResult?> ShowSkillsEditorDialog { get; } = new();

    public Interaction<ProfileNameViewModel, string?> ShowProfileNameDialog { get; } = new();

    public Interaction<Unit, ProfileSelectionResult?> ShowProfileSelectionDialog { get; } = new();

    /// <summary>Проводит новый профиль через обязательные шаги настройки.</summary>
    public Interaction<Unit, bool> RunProfileSetupWizard { get; } = new();

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

        // Следующий монстр начинается с чистого листа: иначе он молча унаследует
        // особенности предыдущего — галочки прячутся за открывшимся окном.
        _featureController.Clear();
        Name = null;
    }

    private async Task CreateFeature()
    {
        var viewModel = new CreateFeatureViewModel(
            _settingsProvider.Settings.SkillDefinitions,
            _settingsProvider.Features.Select(feature => feature.DisplayName));

        var feature = await ShowCreateFeatureDialog.Handle(viewModel);
        if (feature is null)
        {
            return;
        }

        _settingsProvider.AddFeature(feature);
        RebuildFeatureCategories();
    }

    private async Task DeleteSelectedFeatures()
    {
        var selectedFeatures = _featureController.SelectedFeatures.Items.ToArray();
        if (selectedFeatures.Length == 0)
        {
            return;
        }

        var featureUsage = GetBestiaryFeatureUsage();
        var blockedFeatures = selectedFeatures
            .Select(feature => new
            {
                Feature = feature,
                MonsterNames = featureUsage.TryGetValue(feature.Key, out var monsterNames)
                    ? monsterNames
                    : []
            })
            .Where(usage => usage.MonsterNames.Count > 0)
            .ToArray();

        var blockedKeys = blockedFeatures
            .Select(usage => usage.Feature.Key)
            .ToHashSet();
        var removableFeatures = selectedFeatures
            .Where(feature => !blockedKeys.Contains(feature.Key))
            .ToArray();

        if (removableFeatures.Length > 0)
        {
            _settingsProvider.RemoveFeatures(removableFeatures);
            foreach (var feature in removableFeatures)
            {
                _featureController.RemoveFeature(feature);
            }

            RebuildFeatureCategories();
        }

        if (blockedFeatures.Length > 0)
        {
            var warningLines = blockedFeatures.Select(usage =>
                $"• «{usage.Feature.DisplayName}» — {string.Join(", ", usage.MonsterNames.Select(name => $"«{name}»"))}");
            await ShowMessage.Handle(new MessageRequest(
                "Удаление особенностей",
                "Некоторые особенности нельзя удалить",
                "Программа сохранила особенности, которые уже используются монстрами.",
                string.Join(Environment.NewLine, warningLines)));
        }
    }

    /// <summary>
    /// Какие монстры из бестиария используют каждую особенность: ключ особенности
    /// → названия монстров. По этому списку решается, что удалять нельзя.
    /// </summary>
    private IReadOnlyDictionary<string, IReadOnlyCollection<string>> GetBestiaryFeatureUsage()
    {
        var usage = new Dictionary<string, List<string>>();

        foreach (var entry in _bestiaryService.Entries)
        {
            foreach (var feature in entry.Features)
            {
                if (!usage.TryGetValue(feature.Key, out var monsterNames))
                {
                    monsterNames = [];
                    usage[feature.Key] = monsterNames;
                }

                if (!monsterNames.Contains(entry.Name, StringComparer.CurrentCultureIgnoreCase))
                {
                    monsterNames.Add(entry.Name);
                }
            }
        }

        return usage.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlyCollection<string>)pair.Value);
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

    private async Task RenameProfile()
    {
        var current = _profileService.Current;
        var otherNames = _profileService.Profiles
            .Where(profile => profile.Id != current.Id)
            .Select(profile => profile.Name);

        var name = await ShowProfileNameDialog.Handle(
            new ProfileNameViewModel("Название профиля", current.Name, otherNames));

        if (name is null)
        {
            return;
        }

        _profileService.Rename(current, name);
        this.RaisePropertyChanged(nameof(ProfileMenuHeader));
    }

    private async Task SwitchProfile()
    {
        var result = await ShowProfileSelectionDialog.Handle(Unit.Default);
        if (result is null)
        {
            return;
        }

        if (result.Remember is { } remember)
        {
            _profileService.Remember(remember ? result.Profile : null);
            HasRememberedProfile = remember;
        }

        if (result.IsNew)
        {
            await SetUpNewProfile(result.Profile);
            return;
        }

        if (result.Profile.Id != _profileService.Current.Id)
        {
            SwitchToProfile(result.Profile);
        }
    }

    private async Task CreateProfile()
    {
        var name = await ShowProfileNameDialog.Handle(new ProfileNameViewModel(
            "Название нового профиля",
            string.Empty,
            _profileService.Profiles.Select(profile => profile.Name)));

        if (name is null)
        {
            return;
        }

        await SetUpNewProfile(_profileService.Create(name));
    }

    /// <summary>
    /// Переключается на только что созданный профиль и проводит его через
    /// обязательные шаги настройки. Если мастер отменили, профиль удаляется,
    /// а программа возвращается к прежнему — полупустых профилей не остаётся.
    /// </summary>
    private async Task SetUpNewProfile(Profile profile)
    {
        var previous = _profileService.Current;
        SwitchToProfile(profile);

        if (await RunProfileSetupWizard.Handle(Unit.Default))
        {
            RefreshControls();
            return;
        }

        _profileService.Delete(profile);
        SwitchToProfile(previous);
    }

    private void ForgetRememberedProfile()
    {
        _profileService.Remember(null);
        HasRememberedProfile = false;
    }

    private void SwitchToProfile(Profile profile)
    {
        _profileService.SetCurrent(profile);

        var directory = _profileService.GetProfileDirectory(profile);
        _settingsProvider.UseProfile(directory);
        _bestiaryService.UseProfile(directory);

        // Выбор монстра и особенностей относился к прошлому профилю.
        _featureController.Clear();
        SelectedBestiaryEntry = null;
        SelectedQuality = null;
        Name = null;

        RefreshControls();
        this.RaisePropertyChanged(nameof(ProfileMenuHeader));
    }

    private async Task EditQualityLevels()
    {
        var originalNames = _settingsProvider.Settings.MonsterQualities.Keys.ToArray();
        var viewModel = new QualityLevelsViewModel(_settingsProvider.Settings.MonsterQualities);

        var qualities = await ShowQualityLevelsDialog.Handle(viewModel);
        if (qualities is null)
        {
            return;
        }

        _settingsProvider.ApplyQualities(qualities);
        RefreshControls();

        await WarnAboutBestiaryQualities(viewModel.GetRemovedQualityNames(originalNames));
    }

    private async Task EditSkills()
    {
        var viewModel = new SkillsEditorViewModel(
            _settingsProvider.Settings.SkillDefinitions,
            _settingsProvider.Settings.MonsterQualities,
            _settingsProvider.Features,
            GetBestiaryFeatureUsage());

        var result = await ShowSkillsEditorDialog.Handle(viewModel);
        if (result is null)
        {
            return;
        }

        _settingsProvider.ApplySkills(result);
        RefreshControls();

        _featureController.Resynchronize(_settingsProvider.Features);
    }

    /// <summary>
    /// Записи бестиария хранят название качества текстом. Если такого качества
    /// больше нет, подпись у них станет устаревшей — предупреждаем об этом.
    /// </summary>
    private async Task WarnAboutBestiaryQualities(IReadOnlyCollection<string> removedQualityNames)
    {
        if (removedQualityNames.Count == 0)
        {
            return;
        }

        var affectedMonsters = _bestiaryService.Entries
            .Where(entry => removedQualityNames.Contains(entry.QualityName, StringComparer.CurrentCultureIgnoreCase))
            .Select(entry => $"• {entry.Name}")
            .ToArray();

        if (affectedMonsters.Length == 0)
        {
            return;
        }

        await ShowMessage.Handle(new MessageRequest(
            "Стартовые уровни",
            "У этих монстров устарела подпись качества",
            "Качества, с которыми они были созданы, больше нет в наборе. Сами монстры работают как раньше.",
            string.Join(Environment.NewLine, affectedMonsters)));
    }

    private void RefreshControls()
    {
        _settingsProvider.Reload();
        ResolveMissingQualityNames();
        RebuildFeatureCategories();

        // Выбранное качество могло исчезнуть при правке стартовых уровней —
        // иначе генерация монстра пошла бы за несуществующим уровнем.
        if (SelectedQuality is not null &&
            !_settingsProvider.Settings.MonsterQualities.ContainsKey(SelectedQuality))
        {
            SelectedQuality = null;
        }

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
            new(
                "Все особенности",
                features,
                canManageFeatures: true,
                createFeatureCommand: CreateFeatureCommand,
                deleteFeaturesCommand: DeleteFeaturesCommand)
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
            .Select(f => new FeatureViewModel(
                f,
                _featureController,
                _settingsProvider.Settings.SkillDefinitions))
            .OrderBy(f => f.DisplayName);
    }
}
