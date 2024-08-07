using System.Reactive;
using Monstromatic.Models;
using ReactiveUI;

namespace Monstromatic.ViewModels;

public class SkillCounterViewModel : ViewModelBase
{
    private readonly Skill _skill;

    public string SkillName => _skill.Name;
        
    public int SkillValue => _skill.Value;

    public ReactiveCommand<Unit, Unit> Increase { get; }
    public ReactiveCommand<Unit, Unit> Decrease { get; }
    
    public ReactiveCommand<Unit, Unit> Reset { get; }

    public SkillCounterViewModel(Skill skill)
    {
        _skill = skill;
            
        Increase = ReactiveCommand.Create(IncreaseValue);
        Decrease = ReactiveCommand.Create(DecreaseValue);
        Reset = ReactiveCommand.Create(ResetValue);
    }

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