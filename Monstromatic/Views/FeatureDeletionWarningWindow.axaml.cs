using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Monstromatic.Views;

public partial class FeatureDeletionWarningWindow : Window
{
    public FeatureDeletionWarningWindow()
    {
        InitializeComponent();
    }

    public FeatureDeletionWarningWindow(string warningText)
        : this()
    {
        WarningTextBlock.Text = warningText;
    }

    private void CloseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
