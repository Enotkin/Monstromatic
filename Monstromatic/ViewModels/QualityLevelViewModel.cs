using System;
using System.Reactive;
using ReactiveUI;

namespace Monstromatic.ViewModels;

/// <summary>
/// Одна строка редактора стартовых уровней: название качества монстра и уровень,
/// с которого монстр такого качества начинает.
/// </summary>
public class QualityLevelViewModel : ViewModelBase
{
    private readonly Action _changed;
    private string _name;
    private decimal _level;
    private bool _isDuplicateName;

    public QualityLevelViewModel(
        string name,
        int level,
        Action<QualityLevelViewModel> remove,
        Action changed)
    {
        _name = name;
        _level = level;
        _changed = changed;
        RemoveCommand = ReactiveCommand.Create(() => remove(this));
    }

    public string Name
    {
        get => _name;
        set
        {
            this.RaiseAndSetIfChanged(ref _name, value ?? string.Empty);
            RaiseValidationProperties();
            _changed();
        }
    }

    public decimal Level
    {
        get => _level;
        set
        {
            this.RaiseAndSetIfChanged(ref _level, value);
            RaiseValidationProperties();
            _changed();
        }
    }

    /// <summary>
    /// Выставляется редактором, когда такое же название уже есть в другой строке.
    /// </summary>
    public bool IsDuplicateName
    {
        get => _isDuplicateName;
        set
        {
            this.RaiseAndSetIfChanged(ref _isDuplicateName, value);
            RaiseValidationProperties();
        }
    }

    public int LevelValue => decimal.ToInt32(decimal.Truncate(Level));

    public bool IsLevelValid => Level == decimal.Truncate(Level) && Level > 0 && Level % 2m == 0m;

    public bool IsValid => !string.IsNullOrWhiteSpace(Name) && !IsDuplicateName && IsLevelValid;

    public string ValidationMessage
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return "Введите название качества";
            }

            if (IsDuplicateName)
            {
                return "Такое название уже есть";
            }

            return IsLevelValid ? string.Empty : "Уровень должен быть чётным и больше нуля";
        }
    }

    public ReactiveCommand<Unit, Unit> RemoveCommand { get; }

    private void RaiseValidationProperties()
    {
        this.RaisePropertyChanged(nameof(IsValid));
        this.RaisePropertyChanged(nameof(ValidationMessage));
    }
}
