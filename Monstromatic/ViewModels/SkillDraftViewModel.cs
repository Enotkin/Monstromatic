using System;
using System.Reactive;
using Monstromatic.Models;
using ReactiveUI;

namespace Monstromatic.ViewModels;

/// <summary>
/// Черновик одного скилла в редакторе. Правки живут здесь и попадают в
/// настройки только при сохранении, поэтому «Отмена» ничего не портит.
/// </summary>
public class SkillDraftViewModel : ViewModelBase
{
    private readonly Action _changed;
    private string _name;
    private string _tag;
    private decimal _baseModifier;
    private decimal _boostValue;
    private int _number;
    private bool _isDuplicateTag;
    private string _levelValidationMessage = string.Empty;

    public SkillDraftViewModel(
        SkillDefinition? skill,
        Action<SkillDraftViewModel> remove,
        Action changed)
    {
        _name = skill?.Name ?? string.Empty;
        _tag = skill?.Tag ?? string.Empty;
        _baseModifier = skill is null ? 1m : Convert.ToDecimal(skill.BaseModifier);
        _boostValue = skill?.BoostValue ?? 0;
        OriginalTag = skill?.Tag ?? string.Empty;
        _changed = changed;
        RemoveCommand = ReactiveCommand.Create(() => remove(this));
    }

    /// <summary>
    /// Тег на момент открытия окна. Пустой у только что добавленного скилла.
    /// По нему определяется, что скилл переименовали, а не создали заново.
    /// </summary>
    public string OriginalTag { get; }

    public int Number
    {
        get => _number;
        set => this.RaiseAndSetIfChanged(ref _number, value);
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

    public string Tag
    {
        get => _tag;
        set
        {
            this.RaiseAndSetIfChanged(ref _tag, value ?? string.Empty);
            RaiseValidationProperties();
            _changed();
        }
    }

    public decimal BaseModifier
    {
        get => _baseModifier;
        set
        {
            this.RaiseAndSetIfChanged(ref _baseModifier, value);
            RaiseValidationProperties();
            _changed();
        }
    }

    public decimal BoostValue
    {
        get => _boostValue;
        set
        {
            this.RaiseAndSetIfChanged(ref _boostValue, value);
            RaiseValidationProperties();
            _changed();
        }
    }

    /// <summary>Выставляется редактором, когда такой же Tag есть в другой строке.</summary>
    public bool IsDuplicateTag
    {
        get => _isDuplicateTag;
        set
        {
            this.RaiseAndSetIfChanged(ref _isDuplicateTag, value);
            RaiseValidationProperties();
        }
    }

    /// <summary>
    /// Заполняется редактором, если базовый модификатор даёт дробное значение
    /// на одном из стартовых уровней.
    /// </summary>
    public string LevelValidationMessage
    {
        get => _levelValidationMessage;
        set
        {
            this.RaiseAndSetIfChanged(ref _levelValidationMessage, value);
            RaiseValidationProperties();
        }
    }

    public bool IsBoostValueValid => BoostValue >= 0 && BoostValue == decimal.Truncate(BoostValue);

    public bool IsValid => !string.IsNullOrWhiteSpace(Name)
                           && !string.IsNullOrWhiteSpace(Tag)
                           && !IsDuplicateTag
                           && IsBoostValueValid
                           && string.IsNullOrEmpty(LevelValidationMessage);

    public string ValidationMessage
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return "Введите название скилла";
            }

            if (string.IsNullOrWhiteSpace(Tag))
            {
                return "Введите Tag — короткое служебное имя скилла";
            }

            if (IsDuplicateTag)
            {
                return "Такой Tag уже есть у другого скилла";
            }

            if (!IsBoostValueValid)
            {
                return "Модификатор буста должен быть целым и не меньше нуля";
            }

            return LevelValidationMessage;
        }
    }

    public ReactiveCommand<Unit, Unit> RemoveCommand { get; }

    public SkillDefinition CreateDefinition() =>
        new()
        {
            Name = Name.Trim(),
            Tag = Tag.Trim(),
            BaseModifier = decimal.ToDouble(BaseModifier),
            BoostValue = decimal.ToInt32(decimal.Truncate(BoostValue))
        };

    private void RaiseValidationProperties()
    {
        this.RaisePropertyChanged(nameof(IsValid));
        this.RaisePropertyChanged(nameof(ValidationMessage));
    }
}
