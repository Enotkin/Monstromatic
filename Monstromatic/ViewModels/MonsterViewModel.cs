using System.Collections.Generic;
using System.Reactive;
using Monstromatic.Models;
using ReactiveUI;

namespace Monstromatic.ViewModels;

public class MonsterViewModel : ViewModelBase
{
    private readonly Monster _monster;
    private readonly List<SkillCounterViewModel> _skillsVm;

    public ReactiveCommand<Unit, Unit> AddLevel { get; }
    public ReactiveCommand<Unit, Unit> RemoveLevel { get; }
    public string Name => _monster.Name;
    public SkillCounterViewModel Attack { get; }
    public SkillCounterViewModel Defence { get; }
    public SkillCounterViewModel Health { get; }
    public SkillCounterViewModel Perception { get; }
    public SkillCounterViewModel Will { get; }
    public SkillCounterViewModel Trickery { get; }
    
    public MonsterViewModel(Monster monster)
    {
        _monster = monster;

        AddLevel = ReactiveCommand.Create(() =>
        {        
            Level++;
            this.RaisePropertyChanged(nameof(Level));
        });
        
        RemoveLevel = ReactiveCommand.Create(() =>
        {        
            Level--;
            this.RaisePropertyChanged(nameof(Level));
        });

        Attack = new SkillCounterViewModel(monster.Attack);
        Defence = new SkillCounterViewModel(monster.Defence);
        Health = new SkillCounterViewModel(monster.Health);
        Perception = new SkillCounterViewModel(monster.Perception);
        Will = new SkillCounterViewModel(monster.Speed);
        Trickery = new SkillCounterViewModel(monster.Trickery);

        _skillsVm = new List<SkillCounterViewModel>
        {
            Attack, Defence, Health, Perception, Will, Trickery
        };
    }
    
    public int Level
    {
        get => _monster.Level;
        set {
            _monster.Level = value;
            foreach (var skillVm in _skillsVm)
            {
                skillVm.RaisePropertyChanged(nameof(skillVm.SkillValue));
            }
        }
    }


    
}