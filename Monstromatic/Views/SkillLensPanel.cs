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

// Layout slots stay fixed. The lenses themselves grow and make room for each other.
public sealed class SkillLensPanel : UniformGrid
{
    private const double ExpandedScale = 1.85;
    private const double NeighborScale = 0.80;
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

    internal void RefreshLayout() => UpdateLenses();

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

        // Expanded buttons can extend past the original input slot. Keep them
        // usable all the way to their edges, including during the transition.
        if (_state.ActiveSkill is { } active && active.HitActions(position, this))
            hovered = active;

        if (hovered == null)
        {
            hovered = _skills
                .OrderByDescending(skill => skill == _state.ActiveSkill || skill == _state.HoveredSkill)
                .FirstOrDefault(skill => skill.GetLensBounds(this) is { } lens && lens.Contains(position));
        }

        // Empty space around the lenses still belongs to the original slots.
        if (hovered == null)
        {
            foreach (var skill in _skills)
            {
                if (GetSlot(skill) is { } slot && slot.Contains(position))
                {
                    hovered = skill;
                    break;
                }
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
        var entries = _skills
            .Select(skill => (Skill: skill, Slot: GetSlot(skill)))
            .Where(entry => entry.Slot is { Width: > 0 })
            .OrderBy(entry => entry.Slot!.Value.Left)
            .ToArray();
        var focus = _state.ActiveSkill ?? _state.HoveredSkill;
        if (focus == null)
        {
            foreach (var skill in _skills)
            {
                skill.SetLens(1, 0, false);
                SetContainerZIndex(skill, 0);
            }
            return;
        }

        var focusX = _state.ActiveSkill != null
            ? GetSlot(focus)?.Center.X
            : _pointerPosition?.X ?? GetSlot(focus)?.Center.X;
        if (focusX == null || entries.Length == 0)
            return;

        var scales = new double[entries.Length];
        double totalWidth = 0;
        for (var index = 0; index < entries.Length; index++)
        {
            var entry = entries[index];
            var slot = entry.Slot!.Value;
            var distance = Math.Abs(focusX.Value - slot.Center.X) / slot.Width;
            var wave = distance < InfluenceRadius
                ? Math.Pow(0.5 * (1 + Math.Cos(Math.PI * distance / InfluenceRadius)), 2)
                : 0;
            if (entry.Skill == focus)
                wave = _state.ActiveSkill != null ? 1 : Math.Max(0.86, wave);

            scales[index] = NeighborScale + (ExpandedScale - NeighborScale) * wave;
            totalWidth += (entry.Skill.LensWidth + 4) * scales[index];
        }

        // Pack visible lenses inside the row: edge skills remain readable and
        // their buttons are reachable even in the 450px encounter window.
        var minimumGaps = 4 * (entries.Length - 1);
        var fit = Math.Min(1, Math.Max(1, Bounds.Width - minimumGaps) / totalWidth);
        var gap = entries.Length > 1 ? (Bounds.Width - totalWidth * fit) / (entries.Length - 1) : 0;
        var left = entries.Length == 1 ? (Bounds.Width - totalWidth * fit) / 2 : 0;
        for (var index = 0; index < entries.Length; index++)
        {
            var entry = entries[index];
            var scale = scales[index] * fit;
            var width = (entry.Skill.LensWidth + 4) * scale;
            var offset = left + width / 2 - entry.Slot!.Value.Center.X;
            entry.Skill.SetLens(scale, offset, entry.Skill == _state.ActiveSkill);
            SetContainerZIndex(entry.Skill, entry.Skill == focus ? 1 : 0);
            left += width + gap;
        }
    }

    private void SetContainerZIndex(SkillCounterView skill, int zIndex)
    {
        Visual container = skill;
        while (container.GetVisualParent() is { } parent && parent != this)
            container = parent;
        if (container.GetVisualParent() == this)
            container.ZIndex = zIndex;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (_dwellTimer == null)
            return;

        if (change.Property == BoundsProperty)
        {
            if (Bounds.Height <= 0)
                ResetSelection();
            else
                UpdateLenses();
        }
        else if (change.Property == IsVisibleProperty && !IsVisible)
        {
            ResetSelection();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ResetSelection();
        base.OnDetachedFromVisualTree(e);
    }
}
