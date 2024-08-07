using System.Collections.Generic;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Monstromatic.Views;

public partial class CustomHeader : UserControl
{
    public delegate void ToggleExpandedState(bool state);
    public event ToggleExpandedState ExpandedStateSender;
    
    public delegate void CloseButtonEvent();
    public event CloseButtonEvent CloseSender;

    public delegate void ChangeColor(IBrush brush);
    public event ChangeColor ChangeColorSender;

    public bool IsExpanded { get; set; } = true;
    public IBrush CurrentBrush { get; private set; }
    public CustomHeader()
    {
        InitializeComponent();

        var coloredBrushes = GetBrushes();
        
        FillColorSelector(coloredBrushes);
    }
    
    public static readonly StyledProperty<ICommand> DecreaseLevelCommandProperty =
        AvaloniaProperty.Register<CustomHeader, ICommand>(nameof(DecreaseLevelCommand));

    public ICommand DecreaseLevelCommand
    {
        get => GetValue(DecreaseLevelCommandProperty);
        set => SetValue(DecreaseLevelCommandProperty, value);
    }
    
    public static readonly StyledProperty<ICommand> IncreaseLevelCommandProperty =
        AvaloniaProperty.Register<CustomHeader, ICommand>(nameof(IncreaseLevelCommand));

    public ICommand IncreaseLevelCommand
    {
        get => GetValue(IncreaseLevelCommandProperty);
        set => SetValue(IncreaseLevelCommandProperty, value);
    }
    
    public static readonly StyledProperty<ICommand> CloseButtonCommandProperty =
        AvaloniaProperty.Register<CustomHeader, ICommand>(nameof(CloseButtonCommand));
    
    public ICommand CloseButtonCommand
    {
        get => GetValue(CloseButtonCommandProperty);
        set => SetValue(CloseButtonCommandProperty, value);
    }
    
    public static readonly StyledProperty<string> MonsterNameProperty =
        AvaloniaProperty.Register<CustomHeader, string>(nameof(MonsterName));
    
    public string MonsterName
    {
        get => GetValue(MonsterNameProperty);
        set => SetValue(MonsterNameProperty, value);
    }
    
    public static readonly StyledProperty<int> MonsterLevelProperty =
        AvaloniaProperty.Register<CustomHeader, int>(nameof(MonsterLevel));
    
    public int MonsterLevel
    {
        get => GetValue(MonsterLevelProperty);
        set => SetValue(MonsterLevelProperty, value);
    }
    
    public static readonly StyledProperty<bool> IsColorSelectorVisibleProperty =
        AvaloniaProperty.Register<CustomHeader, bool>(nameof(IsColorSelectorVisible), true);

    public bool IsColorSelectorVisible
    {
        get => GetValue(IsColorSelectorVisibleProperty);
        set => SetValue(IsColorSelectorVisibleProperty, value);
    }

    private static List<IBrush> GetBrushes()
    {
        return new List<IBrush>
        {
            new SolidColorBrush(0xFFA0A0A0), // grey
            new SolidColorBrush(0xFFFFFFFF), // white
            new SolidColorBrush(0xFFFF7F7E), // red
            new SolidColorBrush(0xFF81C8FE), // blue
            new SolidColorBrush(0xFFFFE87E), // yellow
            new SolidColorBrush(0xFFFFB180), // orange
            new SolidColorBrush(0xFFFF7FEC), // pink
            new SolidColorBrush(0xFFA27FFF), // purple
            new SolidColorBrush(0xFF7FFF8E) // green
        };
    }

    private void ExpandButton_Click(object sender, RoutedEventArgs e)
    {
        IsExpanded = !IsExpanded;
        ExpandedStateSender!.Invoke(IsExpanded);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        CloseButtonCommand?.Execute(null);
        CloseSender?.Invoke();
    }
    
    private void ColorSelectorOnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var colorSelector = this.GetControl<ComboBox>("ColorSelector");
        var header = this.GetControl<Grid>("Header");
        
        CurrentBrush = (IBrush)colorSelector.SelectedItem;
        
        header.Background = CurrentBrush;
        ChangeColorSender?.Invoke(CurrentBrush);
    }
    
    private void FillColorSelector(IEnumerable<IBrush> colors)
    {
        var colorSelector = this.GetControl<ComboBox>("ColorSelector");
        colorSelector.ItemsSource = colors;
        colorSelector.SelectedIndex = 0;
    }
}