using System;
using System.Reactive;
using Monstromatic.Models;
using ReactiveUI;

namespace Monstromatic.ViewModels;

public class FeatureSkillModifierViewModel : ViewModelBase
{
    private decimal _modifier;
    private readonly Action _modifierChanged;

    public FeatureSkillModifierViewModel(
        SkillDefinition skill,
        Action<FeatureSkillModifierViewModel> remove,
        Action modifierChanged)
        : this(skill, 1m, remove, modifierChanged)
    {
    }

    private FeatureSkillModifierViewModel(
        SkillDefinition? skill,
        decimal initialModifier,
        Action<FeatureSkillModifierViewModel> remove,
        Action modifierChanged)
    {
        Skill = skill;
        _modifier = initialModifier;
        _modifierChanged = modifierChanged;
        RemoveCommand = ReactiveCommand.Create(() => remove(this));
    }

    public SkillDefinition? Skill { get; }

    public bool IsLevel => Skill is null;

    public string DisplayName => IsLevel
        ? "Модификатор уровня"
        : $"Модификатор ({Skill!.Name})";

    public decimal Increment => IsLevel ? 2m : 0.5m;

    public string FormatString => IsLevel ? "0" : "0.0";

    public decimal Modifier
    {
        get => _modifier;
        set
        {
            this.RaiseAndSetIfChanged(ref _modifier, value);
            this.RaisePropertyChanged(nameof(ValidationMessage));
            _modifierChanged();
        }
    }

    public bool IsValid => IsLevel
        ? Modifier == decimal.Truncate(Modifier) && Modifier % 2m == 0m
        : Modifier * 2m == decimal.Truncate(Modifier * 2m);

    public string ValidationMessage => IsValid
        ? string.Empty
        : IsLevel
            ? "Используйте целое чётное число"
            : "Используйте шаг 0,5";

    public ReactiveCommand<Unit, Unit> RemoveCommand { get; }

    public static FeatureSkillModifierViewModel ForLevel(
        Action<FeatureSkillModifierViewModel> remove,
        Action modifierChanged) =>
        new(null, 2m, remove, modifierChanged);
}
