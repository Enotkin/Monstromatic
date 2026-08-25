using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Monstromatic.Models;
using ReactiveUI;

namespace Monstromatic.ViewModels;

/// <summary>
/// Редактор набора скиллов профиля. Слева собирается монстр-пример, который
/// пересчитывается на каждое изменение, справа правится сам набор.
/// </summary>
public class SkillsEditorViewModel : ViewModelBase
{
    private const int FallbackPreviewLevel = 2;

    private readonly IReadOnlyDictionary<string, int> _qualities;
    private readonly IReadOnlyCollection<MonsterFeature> _features;
    private readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> _featureUsage;
    private readonly IReadOnlyCollection<string> _originalTags;

    private IReadOnlyCollection<SkillCounterViewModel> _previewSkills = [];
    private string _previewName = "Монстр";
    private decimal _previewLevel;
    private string _previewMessage = string.Empty;

    public SkillsEditorViewModel(
        IEnumerable<SkillDefinition> skills,
        IReadOnlyDictionary<string, int> qualities,
        IEnumerable<MonsterFeature> features,
        IReadOnlyDictionary<string, IReadOnlyCollection<string>>? featureUsage = null,
        bool isWizardStep = false)
    {
        _qualities = qualities;
        _features = features.ToArray();
        _featureUsage = featureUsage ?? new Dictionary<string, IReadOnlyCollection<string>>();
        IsWizardStep = isWizardStep;

        var existingSkills = skills.ToArray();
        _originalTags = existingSkills
            .Select(skill => skill.Tag)
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .ToArray();

        Skills = new ObservableCollection<SkillDraftViewModel>(
            existingSkills.Select(CreateDraft));

        _previewLevel = qualities.Count > 0
            ? qualities.Values.Min()
            : FallbackPreviewLevel;

        AddSkillCommand = ReactiveCommand.Create(AddSkill);
        Refresh();
    }

    public ObservableCollection<SkillDraftViewModel> Skills { get; }

    public ReactiveCommand<Unit, Unit> AddSkillCommand { get; }

    /// <summary>
    /// Окно открыто как шаг мастера создания профиля.
    /// </summary>
    public bool IsWizardStep { get; }

    public string ConfirmButtonTooltip => IsWizardStep ? "Далее" : "Сохранить";

    public string PreviewName
    {
        get => _previewName;
        set
        {
            this.RaiseAndSetIfChanged(ref _previewName, value ?? string.Empty);
            RebuildPreview();
        }
    }

    public decimal PreviewLevel
    {
        get => _previewLevel;
        set
        {
            this.RaiseAndSetIfChanged(ref _previewLevel, value);
            RebuildPreview();
        }
    }

    public IReadOnlyCollection<SkillCounterViewModel> PreviewSkills
    {
        get => _previewSkills;
        private set => this.RaiseAndSetIfChanged(ref _previewSkills, value);
    }

    /// <summary>Почему монстр-пример сейчас не собирается.</summary>
    public string PreviewMessage
    {
        get => _previewMessage;
        private set => this.RaiseAndSetIfChanged(ref _previewMessage, value);
    }

    public bool CanSave => Skills.Count > 0 && Skills.All(skill => skill.IsValid);

    public string ValidationMessage
    {
        get
        {
            if (Skills.Count == 0)
            {
                return "Добавьте хотя бы один скилл";
            }

            return CanSave ? string.Empty : "Исправьте отмеченные скиллы";
        }
    }

    /// <summary>
    /// Особенности, которые нельзя трогать: их уже используют монстры из
    /// бестиария. Пока они есть, удалить скилл нельзя — то же правило, что и
    /// при удалении особенности напрямую. null, если таких нет.
    /// </summary>
    public string? GetBlockedFeaturesMessage()
    {
        var blocked = GetDependentFeatures()
            .Select(feature => new { Feature = feature, Monsters = GetBestiaryUsage(feature) })
            .Where(usage => usage.Monsters.Count > 0)
            .ToArray();

        if (blocked.Length == 0)
        {
            return null;
        }

        return string.Join(
            Environment.NewLine,
            blocked.Select(usage =>
                $"• {usage.Feature.DisplayName} — {string.Join(", ", usage.Monsters.Select(name => $"«{name}»"))}"));
    }

    /// <summary>
    /// Текст предупреждения об особенностях, которые сломает удаление скиллов,
    /// или null, если удалять нечего.
    /// </summary>
    public string? GetFeatureRemovalWarning()
    {
        var removable = GetDependentFeatures()
            .Where(feature => GetBestiaryUsage(feature).Count == 0)
            .ToArray();

        if (removable.Length == 0)
        {
            return null;
        }

        return string.Join(
            Environment.NewLine,
            removable.Select(feature => $"• {feature.DisplayName}"));
    }

    private IReadOnlyCollection<MonsterFeature> GetDependentFeatures() =>
        SkillTagMigration.FindDependentFeatures(_features, GetRemovedTags());

    private IReadOnlyCollection<string> GetBestiaryUsage(MonsterFeature feature) =>
        _featureUsage.TryGetValue(feature.Key, out var monsters) ? monsters : [];

    public SkillsEditorResult CreateResult() =>
        new()
        {
            Skills = Skills.Select(skill => skill.CreateDefinition()).ToArray(),
            RenamedTags = GetRenamedTags(),
            RemovedTags = GetRemovedTags()
        };

    private IReadOnlyCollection<string> GetRemovedTags()
    {
        var survivingTags = Skills
            .Select(skill => skill.OriginalTag)
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .ToHashSet(StringComparer.Ordinal);

        return _originalTags
            .Where(tag => !survivingTags.Contains(tag))
            .ToArray();
    }

    private IReadOnlyDictionary<string, string> GetRenamedTags() =>
        Skills
            .Where(skill => !string.IsNullOrWhiteSpace(skill.OriginalTag))
            .Where(skill => !string.Equals(skill.OriginalTag, skill.Tag.Trim(), StringComparison.Ordinal))
            .ToDictionary(skill => skill.OriginalTag, skill => skill.Tag.Trim(), StringComparer.Ordinal);

    private void AddSkill()
    {
        Skills.Add(CreateDraft(null));
        Refresh();
    }

    private SkillDraftViewModel CreateDraft(SkillDefinition? skill) =>
        new(skill, RemoveSkill, Refresh);

    private void RemoveSkill(SkillDraftViewModel skill)
    {
        Skills.Remove(skill);
        Refresh();
    }

    private void Refresh()
    {
        RefreshValidation();
        RebuildPreview();
    }

    private void RefreshValidation()
    {
        var duplicateTags = Skills
            .Select(skill => skill.Tag.Trim())
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .GroupBy(tag => tag, StringComparer.CurrentCultureIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.CurrentCultureIgnoreCase);

        var number = 1;
        foreach (var skill in Skills)
        {
            skill.Number = number++;
            skill.IsDuplicateTag = duplicateTags.Contains(skill.Tag.Trim());
            skill.LevelValidationMessage = GetLevelValidationMessage(skill);
        }

        this.RaisePropertyChanged(nameof(CanSave));
        this.RaisePropertyChanged(nameof(ValidationMessage));
    }

    /// <summary>
    /// Базовый модификатор должен давать целую прибавку на каждом стартовом
    /// уровне — иначе расчёт навыка упадёт уже при генерации монстра.
    /// </summary>
    private string GetLevelValidationMessage(SkillDraftViewModel skill)
    {
        var modifier = decimal.ToDouble(skill.BaseModifier);
        var brokenQuality = _qualities
            .Where(quality => !Skill.IsModifierValidForLevel(quality.Value, modifier))
            .Select(quality => quality.Key)
            .FirstOrDefault();

        return brokenQuality is null
            ? string.Empty
            : $"При качестве «{brokenQuality}» этот модификатор даёт дробное значение";
    }

    private void RebuildPreview()
    {
        if (!CanSave)
        {
            PreviewSkills = [];
            PreviewMessage = "Монстр появится, когда все скиллы будут заполнены верно";
            return;
        }

        var level = decimal.ToInt32(decimal.Truncate(PreviewLevel));
        if (PreviewLevel != decimal.Truncate(PreviewLevel) || level <= 0 || level % 2 != 0)
        {
            PreviewSkills = [];
            PreviewMessage = "Уровень монстра-примера должен быть чётным и больше нуля";
            return;
        }

        try
        {
            var definitions = Skills.Select(skill => skill.CreateDefinition()).ToArray();
            var bundle = new FeaturesBundle([], definitions);
            var monster = new Monster(level, PreviewName, bundle);

            PreviewSkills = monster.Skills
                .Select(skill => new SkillCounterViewModel(skill))
                .ToArray();
            PreviewMessage = string.Empty;
        }
        catch (Exception exception)
        {
            PreviewSkills = [];
            PreviewMessage = exception.Message;
        }
    }
}
