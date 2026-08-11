using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Monstromatic.Models;
using ReactiveUI;

namespace Monstromatic.ViewModels;

public class CreateFeatureViewModel : ViewModelBase
{
    private readonly IReadOnlyCollection<FeatureModifierOption> _modifierOptions;
    private readonly HashSet<string> _existingNames;
    private string? _name;
    private string? _comment;
    private FeatureModifierOption? _selectedModifierOption;

    public CreateFeatureViewModel(
        IEnumerable<SkillDefinition> skills,
        IEnumerable<string> existingFeatureNames)
    {
        var availableSkills = skills.ToArray();
        _modifierOptions =
        [
            FeatureModifierOption.ForLevel(),
            ..availableSkills.Select(FeatureModifierOption.ForSkill)
        ];
        _existingNames = existingFeatureNames.ToHashSet(StringComparer.CurrentCultureIgnoreCase);
        Modifiers = new ObservableCollection<FeatureSkillModifierViewModel>();
    }

    public string? Name
    {
        get => _name;
        set
        {
            this.RaiseAndSetIfChanged(ref _name, value);
            this.RaisePropertyChanged(nameof(CanSave));
            this.RaisePropertyChanged(nameof(NameValidationMessage));
        }
    }

    public string? Comment
    {
        get => _comment;
        set => this.RaiseAndSetIfChanged(ref _comment, value);
    }

    public FeatureModifierOption? SelectedModifierOption
    {
        get => _selectedModifierOption;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedModifierOption, value);
            if (value is not null)
            {
                AddModifier(value);
            }
        }
    }

    public bool CanAddModifier => AvailableModifierOptions.Any();

    public IEnumerable<FeatureModifierOption> AvailableModifierOptions =>
        _modifierOptions.Where(option => option.IsLevel
            ? !HasLevelModifier
            : Modifiers.All(modifier => modifier.Skill?.Tag != option.Skill!.Tag));

    public bool CanSave => !string.IsNullOrWhiteSpace(Name) &&
                           !_existingNames.Contains(Name.Trim()) &&
                           Modifiers.All(modifier => modifier.IsValid);

    public string NameValidationMessage
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return "Введите название особенности";
            }

            return _existingNames.Contains(Name.Trim())
                ? "Особенность с таким названием уже существует"
                : string.Empty;
        }
    }

    public ObservableCollection<FeatureSkillModifierViewModel> Modifiers { get; }

    public MonsterFeature? CreateFeature()
    {
        if (!CanSave)
        {
            return null;
        }

        return new MonsterFeature
        {
            Key = $"User_{Guid.NewGuid():N}",
            DisplayName = Name!.Trim(),
            DetailsDisplayName = Name.Trim(),
            LevelModifier = Modifiers
                .Where(modifier => modifier.IsLevel)
                .Select(modifier => decimal.ToInt32(modifier.Modifier))
                .SingleOrDefault(),
            Description = Comment?.Trim() ?? string.Empty,
            SkillModifiers = Modifiers
                .Where(modifier => !modifier.IsLevel)
                .Select(modifier => new SkillModifier
                {
                    Tag = modifier.Skill!.Tag,
                    Modifier = decimal.ToDouble(modifier.Modifier)
                })
                .ToArray()
        };
    }

    private void AddModifier(FeatureModifierOption option)
    {
        if (option.IsLevel && !HasLevelModifier)
        {
            Modifiers.Insert(0, FeatureSkillModifierViewModel.ForLevel(
                RemoveModifier,
                RaiseModifierValidationProperties));
        }
        else if (option.Skill is not null)
        {
            AddSkillModifier(option.Skill);
        }

        _selectedModifierOption = null;
        this.RaisePropertyChanged(nameof(SelectedModifierOption));
        RaiseAvailableModifierProperties();
    }

    private void AddSkillModifier(SkillDefinition skill)
    {
        if (Modifiers.Any(modifier => modifier.Skill?.Tag == skill.Tag))
        {
            return;
        }

        Modifiers.Insert(0, new FeatureSkillModifierViewModel(
            skill,
            RemoveModifier,
            RaiseModifierValidationProperties));
        RaiseAvailableModifierProperties();
    }

    private void RemoveModifier(FeatureSkillModifierViewModel modifier)
    {
        Modifiers.Remove(modifier);
        RaiseAvailableModifierProperties();
    }

    private void RaiseAvailableModifierProperties()
    {
        this.RaisePropertyChanged(nameof(AvailableModifierOptions));
        this.RaisePropertyChanged(nameof(CanAddModifier));
        RaiseModifierValidationProperties();
    }

    private bool HasLevelModifier => Modifiers.Any(modifier => modifier.IsLevel);

    private void RaiseModifierValidationProperties()
    {
        this.RaisePropertyChanged(nameof(CanSave));
    }
}

public sealed class FeatureModifierOption
{
    private FeatureModifierOption(string displayName, SkillDefinition? skill)
    {
        DisplayName = displayName;
        Skill = skill;
    }

    public string DisplayName { get; }

    public SkillDefinition? Skill { get; }

    public bool IsLevel => Skill is null;

    public static FeatureModifierOption ForLevel() =>
        new("Добавить модификатор уровня", null);

    public static FeatureModifierOption ForSkill(SkillDefinition skill) =>
        new($"Добавить модификатор {skill.Name.ToLower(CultureInfo.CurrentCulture)}", skill);
}
