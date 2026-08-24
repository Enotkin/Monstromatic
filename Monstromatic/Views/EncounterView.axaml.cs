using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using Monstromatic.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace Monstromatic.Views;

public partial class EncounterView : ReactiveWindow<EncounterViewModel>
{
    private bool _isExpanded = true;
    private double _expanderHeight;

    public EncounterView()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
            disposables(ViewModel?.ShowBestiaryMessage.RegisterHandler(DoShowBestiaryMessage)
                        ?? throw new InvalidOperationException()));

        this.FindControl<Grid>("Header")?.AddHandler(
            PointerPressedEvent, Header_PointerPressed, RoutingStrategies.Bubble | RoutingStrategies.Direct, true);

        Height = 360;
        this.AddHandler(SizeChangedEvent, WindowResized);

        ExpandableGrid.PropertyChanged += (sender, args) =>
        {
            if (args.Property.Name == "Height")
            {
                UpdateWindowMeasureAsync();
            }
        };

        this.MaxHeight = 0.9 * Screens.ScreenFromWindow(this)!.WorkingArea.Height;

        FillColorSelector(GetBrushes());

#if DEBUG
        this.AttachDevTools();
#endif
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        EnsureFirstMonsterIsFullyVisibleAsync();
        UpdateWindowMeasureAsync();
    }

    private async void EnsureFirstMonsterIsFullyVisibleAsync()
    {
        // Даём окну завершить раскладку, затем при необходимости увеличиваем его
        // ровно настолько, чтобы первая карточка монстра была видна целиком.
        for (var attempt = 0; attempt < 3; attempt++)
        {
            await Task.Delay(50);

            var firstMonster = this.GetVisualDescendants().OfType<MonsterView>().FirstOrDefault();
            var monsterBottom = firstMonster?.TranslatePoint(
                new Point(0, firstMonster.Bounds.Height),
                this);

            if (monsterBottom is null)
            {
                continue;
            }

            const double bottomMargin = 12;
            var missingHeight = monsterBottom.Value.Y + bottomMargin - ClientSize.Height;
            if (missingHeight <= 0)
            {
                return;
            }

            var newHeight = Math.Min(MaxHeight, Height + missingHeight);
            if (newHeight <= Height)
            {
                return;
            }

            Height = Math.Ceiling(newHeight);
        }
    }

    private static List<IBrush> GetBrushes()
    {
        return
        [
            new SolidColorBrush(0xFFA0A0A0), // grey
            new SolidColorBrush(0xFFFFFFFF), // white
            new SolidColorBrush(0xFFFF7F7E), // red
            new SolidColorBrush(0xFF81C8FE), // blue
            new SolidColorBrush(0xFFFFE87E), // yellow
            new SolidColorBrush(0xFFFFB180), // orange
            new SolidColorBrush(0xFFFF7FEC), // pink
            new SolidColorBrush(0xFFA27FFF), // purple
            new SolidColorBrush(0xFF7FFF8E) // green
        ];
    }

    private void FillColorSelector(IEnumerable<IBrush> colors)
    {
        var colorSelector = this.GetControl<ComboBox>("ColorSelector");
        colorSelector.ItemsSource = colors;
        colorSelector.SelectedIndex = 0;
    }

    private void ColorSelectorOnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var colorSelector = this.GetControl<ComboBox>("ColorSelector");
        var header = this.GetControl<Grid>("Header");

        if (colorSelector.SelectedItem is not IBrush selectedBrush)
        {
            return;
        }

        header.Background = selectedBrush;
        ChangeBorderColor(selectedBrush);
    }

    private void ChangeBorderColor(IBrush brush)
    {
        var border = this.GetControl<Border>("Border");
        border.BorderBrush = brush;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private Task DoShowBestiaryMessage(IInteractionContext<string, Unit> interactionContext)
    {
        var button = this.GetControl<Button>("AddToBestiaryButton");
        var flyout = new Flyout
        {
            Content = new Border
            {
                Padding = new Thickness(10),
                Child = new TextBlock
                {
                    Text = interactionContext.Input,
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 230
                }
            }
        };

        flyout.ShowAt(button);
        interactionContext.SetOutput(Unit.Default);
        return Task.CompletedTask;
    }

    private void Header_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.Source is Grid { Name: "Header" })
        {
            BeginMoveDrag(e);
        }
    }

    private void WindowResized(object sender, SizeChangedEventArgs e)
    {
        ExpandableGrid.Height += e.NewSize.Height - e.PreviousSize.Height;
    }

    private void ChangeExpandState(bool state)
    {
        if (_isExpanded)
        {
            _expanderHeight = ExpandableGrid.Bounds.Height;
            ExpandableGrid.Height = 0;
        }
        else
        {
            ExpandableGrid.Height = _expanderHeight;
        }

        _isExpanded = !_isExpanded;

        UpdateWindowMeasureAsync();
    }

    private async void UpdateWindowMeasureAsync()
    {
        await Task.Delay(100);
        this.InvalidateMeasure();
    }
}
