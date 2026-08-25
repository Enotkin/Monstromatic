using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Monstromatic.ViewModels;

namespace Monstromatic.Views;

public partial class QualityLevelsWindow : Window
{
    public QualityLevelsWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Итог работы окна. Дублирует результат ShowDialog, чтобы окно можно было
    /// использовать и как шаг мастера, когда модального диалога ещё нет.
    /// </summary>
    public Dictionary<string, int>? Result { get; private set; }

    private void SaveButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not QualityLevelsViewModel viewModel || !viewModel.CanSave)
        {
            return;
        }

        Result = viewModel.CreateQualities();
        Close(Result);
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(default(Dictionary<string, int>));
    }
}
