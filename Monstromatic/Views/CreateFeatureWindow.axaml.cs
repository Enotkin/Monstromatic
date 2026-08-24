using Avalonia.Controls;
using Avalonia.Interactivity;
using Monstromatic.Models;
using Monstromatic.ViewModels;

namespace Monstromatic.Views;

public partial class CreateFeatureWindow : Window
{
    public CreateFeatureWindow()
    {
        InitializeComponent();
    }

    private void SaveButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CreateFeatureViewModel viewModel)
        {
            return;
        }

        var feature = viewModel.CreateFeature();
        if (feature is not null)
        {
            Close(feature);
        }
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(default(MonsterFeature));
    }
}
