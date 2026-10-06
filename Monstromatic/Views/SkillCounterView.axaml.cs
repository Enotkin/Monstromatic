using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia;
using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Transformation;
using Avalonia.VisualTree;
using Monstromatic.ViewModels;

namespace Monstromatic.Views;

public partial class SkillCounterView : ReactiveUserControl<SkillCounterViewModel>
{
    private SkillLensPanel? _lensPanel;
    private readonly Grid _lens;
    private readonly Grid _actions;
    private readonly Border _glow;
    private double _targetScale = 1;
    private bool _showActions;

    public SkillCounterView()
    {
        InitializeComponent();
        _lens = this.GetControl<Grid>("Lens");
        _actions = this.GetControl<Grid>("SkillActions");
        _glow = this.GetControl<Border>("LensGlow");
    }

    internal void SetLens(double scale, bool showActions)
    {
        if (Math.Abs(_targetScale - scale) > 0.0001)
        {
            _targetScale = scale;
            var transform = new TransformOperations.Builder(1);
            transform.AppendScale(scale, scale);
            _lens.RenderTransform = transform.Build();
        }
        if (_showActions != showActions)
        {
            _showActions = showActions;
            _glow.Opacity = showActions ? 1 : 0;
            _actions.Opacity = showActions ? 1 : 0;
            _actions.IsHitTestVisible = showActions;
            _actions.IsEnabled = showActions;
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _lensPanel = this.GetVisualAncestors().OfType<SkillLensPanel>().FirstOrDefault();
        _lensPanel?.Register(this);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _lensPanel?.Unregister(this);
        _lensPanel = null;
        SetLens(1, false);
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        _lensPanel?.ResetSelection();
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
        // Keyboard users can reveal actions without having to hover first.
        if (e.NavigationMethod is NavigationMethod.Tab or NavigationMethod.Directional)
            _lensPanel?.ActivateFromKeyboard(this);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
