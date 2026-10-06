using System;

namespace Monstromatic.Views;

// Leaving the row does not mean explicitly moving onto another skill.
internal sealed class SkillLensState<T> where T : class
{
    public static readonly TimeSpan ActivationDelay = TimeSpan.FromMilliseconds(220);
    private TimeSpan _hoverStarted;

    public T? HoveredSkill { get; private set; }
    public T? ActiveSkill { get; private set; }

    public void MoveTo(T? skill, TimeSpan now)
    {
        if (ReferenceEquals(HoveredSkill, skill))
            return;

        HoveredSkill = skill;
        _hoverStarted = now;
        if (skill != null && !ReferenceEquals(ActiveSkill, skill))
            ActiveSkill = null;
    }

    public bool TryActivate(TimeSpan now)
    {
        if (HoveredSkill == null || ReferenceEquals(ActiveSkill, HoveredSkill) ||
            now - _hoverStarted < ActivationDelay)
            return false;

        ActiveSkill = HoveredSkill;
        return true;
    }

    public void Reset()
    {
        HoveredSkill = null;
        ActiveSkill = null;
        _hoverStarted = default;
    }
}
