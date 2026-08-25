using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;

namespace Monstromatic.ViewModels;

/// <summary>
/// Ввод названия профиля — и при создании, и при переименовании.
/// </summary>
public class ProfileNameViewModel : ViewModelBase
{
    private readonly HashSet<string> _takenNames;
    private string _name;

    public ProfileNameViewModel(string heading, string name, IEnumerable<string> takenNames)
    {
        Heading = heading;
        _name = name;
        _takenNames = takenNames.ToHashSet(StringComparer.CurrentCultureIgnoreCase);
    }

    public string Heading { get; }

    public string Name
    {
        get => _name;
        set
        {
            this.RaiseAndSetIfChanged(ref _name, value ?? string.Empty);
            this.RaisePropertyChanged(nameof(CanSave));
            this.RaisePropertyChanged(nameof(ValidationMessage));
        }
    }

    public bool CanSave => !string.IsNullOrWhiteSpace(Name) && !_takenNames.Contains(Name.Trim());

    public string ValidationMessage
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return "Введите название профиля";
            }

            return _takenNames.Contains(Name.Trim())
                ? "Профиль с таким названием уже есть"
                : string.Empty;
        }
    }
}
