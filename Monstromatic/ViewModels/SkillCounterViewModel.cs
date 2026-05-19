using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using Monstromatic.Models;
using ReactiveUI;

namespace Monstromatic.ViewModels;

public class SkillCounterViewModel : ViewModelBase
{
    private readonly Skill _skill;

    public SkillCounterViewModel(Skill skill)
    {
        _skill = skill;

        Increase = ReactiveCommand.Create(IncreaseValue);
        Decrease = ReactiveCommand.Create(DecreaseValue);
        Reset = ReactiveCommand.Create(ResetValue);
    }

    public string SkillName => _skill.Name;

    public int SkillValue => _skill.Value;

    public IReadOnlyCollection<SkillComment> Comments => _skill.Comments;

    public bool HasComments => Comments.Any();

    public string? CommentsToolTipText =>
        HasComments
            ? string.Join(
                "\n\n",
                Comments.Select(comment => $"{comment.FeatureName}: {comment.Text}"))
            : null;

    public ReactiveCommand<Unit, Unit> Increase { get; }

    public ReactiveCommand<Unit, Unit> Decrease { get; }

    public ReactiveCommand<Unit, Unit> Reset { get; }

    private void IncreaseValue()
    {
        _skill.Increment();
        this.RaisePropertyChanged(nameof(SkillValue));
    }

    private void DecreaseValue()
    {
        _skill.Decrement();
        this.RaisePropertyChanged(nameof(SkillValue));
    }

    private void ResetValue()
    {
        _skill.Reset();
        this.RaisePropertyChanged(nameof(SkillValue));
    }
}
