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
using Avalonia.ReactiveUI;
using Monstromatic.Models;
using Monstromatic.ViewModels;
using ReactiveUI;

namespace Monstromatic.Views;

public partial class EncounterView : ReactiveWindow<EncounterViewModel>
{
    private bool _isExpanded = true;
    private double _expanderHeight = 0;
    public EncounterView()
    {
        InitializeComponent();
        
        this.FindControl<Grid>("Header")?.AddHandler(
            PointerPressedEvent, Header_PointerPressed,RoutingStrategies.Bubble | RoutingStrategies.Direct, true);

        Height = 300;
        this.AddHandler(SizeChangedEvent, WindowResized);
        
        CustomHeader.CloseSender += Close;
        CustomHeader.ExpandedStateSender += ChangeExpandState;
        CustomHeader.ChangeColorSender += ChangeBorderColor;
        
        ExpandableGrid.PropertyChanged += (sender, args) =>
        {
            if (args.Property.Name == "Height")
                UpdateWindowMeasureAsync();
        };
        this.MaxHeight = 0.9 * Screens.ScreenFromWindow(this)!.WorkingArea.Height;

        
        ChangeBorderColor(CustomHeader.CurrentBrush);
        
#if DEBUG
        this.AttachDevTools();
#endif
        
    }
    
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        UpdateWindowMeasureAsync();
    }
    
    
    private void ChangeBorderColor(IBrush brush)
    {
        var border = this.GetControl<Border>("Border");
        border.BorderBrush = brush;
    }

    private void Header_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.Source is Grid {Name: "Header"})
            BeginMoveDrag(e);
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