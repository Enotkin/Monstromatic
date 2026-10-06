using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Monstromatic.Views;

// Input belongs to fixed slots; only their inner content is magnified.
public sealed class SkillLensPanel : UniformGrid
{
    private const double Magnification = 0.20;
    private const double InfluenceRadius = 1.6;
    private readonly List<SkillCounterView> _skills = new();
    private readonly SkillLensState<SkillCounterView> _state = new();
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly DispatcherTimer _dwellTimer;
    private Point? _pointerPosition;

    public SkillLensPanel()
    {
        Background = Brushes.Transparent;
        // Dispatcher ticks can arrive just before the dwell deadline. A short
        // polling interval avoids waiting a second full dwell in that case.
        _dwellTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(25) };
        _dwellTimer.Tick += OnDwell;
        AddHandler(PointerMovedEvent, OnPointerMovedInRow, RoutingStrategies.Tunnel, handledEventsToo: true);
        PointerEntered += OnPointerMovedInRow;
        PointerExited += (_, _) => MoveTo(null, null);
    }

    internal void Register(SkillCounterView skill)
    {
        if (!_skills.Contains(skill))
            _skills.Add(skill);
    }

    internal void Unregister(SkillCounterView skill)
    {
        _skills.Remove(skill);
        if (_state.HoveredSkill == skill || _state.ActiveSkill == skill)
            ResetSelection();
    }

    internal void ResetSelection()
    {
        _dwellTimer.Stop();
        _state.Reset();
        _pointerPosition = null;
        UpdateLenses();
    }

    internal void ActivateFromKeyboard(SkillCounterView skill)
    {
        ResetOtherRows();
        _dwellTimer.Stop();
        _pointerPosition = null;
        _state.MoveTo(null, _clock.Elapsed);
        _state.MoveTo(skill, _clock.Elapsed - SkillLensState<SkillCounterView>.ActivationDelay);
        _state.TryActivate(_clock.Elapsed);
        UpdateLenses();
    }

    private void OnPointerMovedInRow(object? sender, PointerEventArgs e)
    {
        var position = e.GetPosition(this);
        SkillCounterView? hovered = null;
        foreach (var skill in _skills)
        {
            if (GetSlot(skill) is { } slot && slot.Contains(position))
            {
                hovered = skill;
                break;
            }
        }
        MoveTo(hovered, hovered == null ? null : position);
    }

    private void MoveTo(SkillCounterView? skill, Point? position)
    {
        if (_state.HoveredSkill != skill)
        {
            if (skill != null)
                ResetOtherRows();
            _dwellTimer.Stop();
            _state.MoveTo(skill, _clock.Elapsed);
            if (skill != null && _state.ActiveSkill != skill)
                _dwellTimer.Start();
        }
        _pointerPosition = position;
        UpdateLenses();
    }

    private void ResetOtherRows()
    {
        if (TopLevel.GetTopLevel(this) is not { } window)
            return;

        foreach (var row in window.GetVisualDescendants().OfType<SkillLensPanel>())
        {
            if (row != this)
                row.ResetSelection();
        }
    }

    private void OnDwell(object? sender, EventArgs e)
    {
        if (!IsEffectivelyVisible || Bounds.Height <= 0)
        {
            ResetSelection();
            return;
        }
        if (_state.TryActivate(_clock.Elapsed))
        {
            _dwellTimer.Stop();
            UpdateLenses();
        }
    }

    private Rect? GetSlot(SkillCounterView skill)
    {
        var origin = skill.TranslatePoint(default, this);
        return origin is { } point ? new Rect(point, skill.Bounds.Size) : null;
    }

    private void UpdateLenses()
    {
        foreach (var skill in _skills)
        {
            var active = _state.ActiveSkill == skill;
            var influence = active ? 1.0 : 0.0;
            if (!active && _pointerPosition is { } pointer && GetSlot(skill) is { Width: > 0 } slot)
            {
                var distance = Math.Abs(pointer.X - slot.Center.X) / slot.Width;
                if (distance < InfluenceRadius)
                {
                    var wave = 0.5 * (1 + Math.Cos(Math.PI * distance / InfluenceRadius));
                    influence = wave * wave;
                }
            }
            skill.SetLens(1 + Magnification * influence, active);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (_dwellTimer != null &&
            ((change.Property == BoundsProperty && Bounds.Height <= 0) ||
             (change.Property == IsVisibleProperty && !IsVisible)))
            ResetSelection();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ResetSelection();
        base.OnDetachedFromVisualTree(e);
    }
}
