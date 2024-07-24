using System;
using System.Collections.Generic;
using System.Reactive;
using Monstromatic.Models;
using ReactiveUI;

namespace Monstromatic.ViewModels;

public class MonsterViewModel : ViewModelBase
{
    public delegate void RemovingMonsterEvent(Guid monsterId);

    private readonly Monster _monster;
    private readonly List<SkillCounterViewModel> _skillsVm;
    
    public event RemovingMonsterEvent RemovingMonsterEventInv;
    
    public ReactiveCommand<Unit, Unit> CloseCommand { get; }
    
    public Guid Id { get; }
    public bool IsAlive { get; set; } = true;
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

        Id = _monster.Id;
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

        CloseCommand = ReactiveCommand.Create(RemoveMonster);

        Attack = new SkillCounterViewModel(monster.Attack);
        Defence = new SkillCounterViewModel(monster.Defence);
        Health = new SkillCounterViewModel(monster.Health);
        Perception = new SkillCounterViewModel(monster.Perception);
        Will = new SkillCounterViewModel(monster.Speed);
        Trickery = new SkillCounterViewModel(monster.Trickery);

        _skillsVm = [Attack, Defence, Health, Perception, Will, Trickery];

        this.WhenAnyValue(x => x.IsAlive).Subscribe(x => RemoveMonster());
        this.WhenAnyValue(vm => vm._monster.Level).Subscribe(_ => UpdateSkills());
    }
    
    public int Level
    {
        get => _monster.Level;
        set
        {
            _monster.Level = value;
            this.RaisePropertyChanged();
            UpdateSkills();
        }
    }

    private void UpdateSkills()
    {
        foreach (var skillVm in _skillsVm)
        {
            skillVm.RaisePropertyChanged(nameof(skillVm.SkillValue));
        }
    }

    private void RemoveMonster()
    {
        RemovingMonsterEventInv?.Invoke(_monster.Id);
    }

    public void UpdateLevel()
    {
        Level = _monster.Level;
    }
}