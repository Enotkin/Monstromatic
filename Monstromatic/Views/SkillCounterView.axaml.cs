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
    private double _targetOffset;
    private bool _showActions;

    public SkillCounterView()
    {
        InitializeComponent();
        _lens = this.GetControl<Grid>("Lens");
        _actions = this.GetControl<Grid>("SkillActions");
        _glow = this.GetControl<Border>("LensGlow");
    }

    internal double LensWidth => _lens.Width;

    internal Rect? GetLensBounds(Visual relativeTo) => GetBounds(_lens, relativeTo);

    internal bool HitActions(Point position, Visual relativeTo) =>
        _showActions && GetBounds(_actions, relativeTo) is { } bounds && bounds.Contains(position);

    private static Rect? GetBounds(Control control, Visual relativeTo)
    {
        var topLeft = control.TranslatePoint(default, relativeTo);
        var bottomRight = control.TranslatePoint(new Point(control.Bounds.Width, control.Bounds.Height), relativeTo);
        return topLeft is { } start && bottomRight is { } end ? new Rect(start, end) : null;
    }

    internal void SetLens(double scale, double offset, bool showActions)
    {
        if (Math.Abs(_targetScale - scale) > 0.0001 || Math.Abs(_targetOffset - offset) > 0.01)
        {
            _targetScale = scale;
            _targetOffset = offset;
            var transform = new TransformOperations.Builder(2);
            transform.AppendScale(scale, scale);
            transform.AppendTranslate(offset, 0);
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
        SetLens(1, 0, false);
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        _lensPanel?.ResetSelection();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BoundsProperty && _lens != null)
        {
            _lens.Width = Math.Clamp(Bounds.Width - 8, 16, 112);
            _lensPanel?.RefreshLayout();
        }
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
