using Avalonia.Controls;
using Avalonia.Interactivity;
using Monstromatic.ViewModels;

namespace Monstromatic.Views;

public partial class ProfileNameWindow : Window
{
    public ProfileNameWindow()
    {
        InitializeComponent();
    }

    public string? Result { get; private set; }

    private void SaveButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ProfileNameViewModel viewModel || !viewModel.CanSave)
        {
            return;
        }

        Result = viewModel.Name.Trim();
        Close(Result);
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(default(string));
    }
}
