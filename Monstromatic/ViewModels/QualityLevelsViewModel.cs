using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using ReactiveUI;

namespace Monstromatic.ViewModels;

/// <summary>
/// Редактор стартовых уровней: набор качеств монстра («Слабый», «БОСС!!!») и
/// уровень, с которого монстр такого качества начинается.
/// </summary>
public class QualityLevelsViewModel : ViewModelBase
{
    private const int DefaultLevel = 2;

    public QualityLevelsViewModel(IReadOnlyDictionary<string, int> qualities, bool isWizardStep = false)
    {
        IsWizardStep = isWizardStep;
        Qualities = new ObservableCollection<QualityLevelViewModel>(
            qualities.Select(quality => CreateRow(quality.Key, quality.Value)));

        AddQualityCommand = ReactiveCommand.Create(AddQuality);
        RefreshValidation();
    }

    public ObservableCollection<QualityLevelViewModel> Qualities { get; }

    public ReactiveCommand<Unit, Unit> AddQualityCommand { get; }

    /// <summary>
    /// Окно открыто как шаг мастера создания профиля: закрывать его без
    /// заполнения нельзя, а кнопка сохранения называется «Далее».
    /// </summary>
    public bool IsWizardStep { get; }

    public string ConfirmButtonTooltip => IsWizardStep ? "Далее" : "Сохранить";

    public bool CanSave => Qualities.Count > 0 && Qualities.All(quality => quality.IsValid);

    public string ValidationMessage
    {
        get
        {
            if (Qualities.Count == 0)
            {
                return "Добавьте хотя бы одно качество монстра";
            }

            return CanSave ? string.Empty : "Исправьте отмеченные строки";
        }
    }

    /// <summary>
    /// Названия качеств, которые были в наборе на момент открытия окна, но
    /// исчезли после правки. Нужны, чтобы предупредить о записях бестиария.
    /// </summary>
    public IReadOnlyCollection<string> GetRemovedQualityNames(IEnumerable<string> originalNames)
    {
        var currentNames = Qualities
            .Select(quality => quality.Name.Trim())
            .ToHashSet(StringComparer.CurrentCultureIgnoreCase);

        return originalNames
            .Where(name => !currentNames.Contains(name))
            .ToArray();
    }

    public Dictionary<string, int> CreateQualities() =>
        Qualities.ToDictionary(
            quality => quality.Name.Trim(),
            quality => quality.LevelValue);

    private void AddQuality()
    {
        var lastLevel = Qualities.Count > 0
            ? Qualities.Max(quality => quality.LevelValue)
            : 0;

        Qualities.Add(CreateRow(string.Empty, lastLevel + DefaultLevel));
        RefreshValidation();
    }

    private QualityLevelViewModel CreateRow(string name, int level) =>
        new(name, level, RemoveQuality, RefreshValidation);

    private void RemoveQuality(QualityLevelViewModel quality)
    {
        Qualities.Remove(quality);
        RefreshValidation();
    }

    private void RefreshValidation()
    {
        var duplicateNames = Qualities
            .Select(quality => quality.Name.Trim())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .GroupBy(name => name, StringComparer.CurrentCultureIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.CurrentCultureIgnoreCase);

        foreach (var quality in Qualities)
        {
            quality.IsDuplicateName = duplicateNames.Contains(quality.Name.Trim());
        }

        this.RaisePropertyChanged(nameof(CanSave));
        this.RaisePropertyChanged(nameof(ValidationMessage));
    }
}
