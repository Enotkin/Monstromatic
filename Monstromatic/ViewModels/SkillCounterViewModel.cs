using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Monstromatic.Models;
using Monstromatic.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Monstromatic.ViewModels;

public class SkillCounterViewModel : ViewModelBase
{
    private readonly Skill _skill;

    public string SkillName => _skill.Name;
        
    public int SkillValue => _skill.Value;

    public ReactiveCommand<Unit, Unit> Increase { get; }
    public ReactiveCommand<Unit, Unit> Decrease { get; }

    public SkillCounterViewModel(Skill skill)
    {
        _skill = skill;
            
        Increase = ReactiveCommand.Create(IncreaseValue);
        Decrease = ReactiveCommand.Create(DecreaseValue);
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
}