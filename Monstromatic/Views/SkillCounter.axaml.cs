using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Monstromatic.Views;

public partial class SkillCounter : UserControl
{
    public SkillCounter()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static readonly StyledProperty<int> Counter =
        AvaloniaProperty.Register<SkillCounter, int>(nameof(CounterSource));
        
    public int CounterSource
    {
        get => GetValue(Counter);
        set => SetValue(Counter, value);
    }

    private void IncreaseButtonClick(object sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void DecreaseButtonClick(object sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }
}