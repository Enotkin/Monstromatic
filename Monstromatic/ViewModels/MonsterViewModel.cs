using System;
using System.Collections.Generic;
using System.Reactive;
using Monstromatic.Models;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Monstromatic.ViewModels;

public partial class MonsterViewModel : ViewModelBase
{
    public delegate void RemovingMonsterEvent(Guid monsterId);

    private readonly Monster _monster;
    
    public List<SkillCounterViewModel> SkillsVm { get; }

    public event RemovingMonsterEvent RemovingMonsterEventInv;
    
    public ReactiveCommand<Unit, Unit> CloseCommand { get; }
    
    public Guid Id { get; }
    
    public bool IsAlive { get; set; } = true;
    
    public string Name => _monster.Name;
    
    public SkillCounterViewModel Attack { get; }
    
    public SkillCounterViewModel Defence { get; }
    
    public SkillCounterViewModel Bravery { get; }
    
    public SkillCounterViewModel Trickery { get; }
    
    public MonsterViewModel(Monster monster)
    {
        _monster = monster;

        Id = _monster.Id;

        CloseCommand = ReactiveCommand.Create(RemoveMonster);

        Attack = new SkillCounterViewModel(monster.Attack);
        Defence = new SkillCounterViewModel(monster.Defence);
        Bravery = new SkillCounterViewModel(monster.Bravery);
        Trickery = new SkillCounterViewModel(monster.Trickery);

        SkillsVm = [Attack, Defence, Bravery, Trickery];

        this.WhenAnyValue(x => x.IsAlive).Subscribe(x => RemoveMonster());
        this.WhenAnyValue(vm => vm._monster.Level).Subscribe(_ => UpdateSkills());
    }

    [ReactiveCommand]
    private void ResetModifications()
    {
        UpdateSkills();
    }
    
    [ReactiveCommand]
    private void AddLevel()
    {
        _monster.PersonalLevel++;
        this.RaisePropertyChanged(nameof(Level));
    }

    [ReactiveCommand]
    private void RemoveLevel()
    {
        _monster.PersonalLevel--;
        this.RaisePropertyChanged(nameof(Level));
    }

    public int Level => _monster.Level;

    private void UpdateSkills() =>
        SkillsVm.ForEach(skillVm => skillVm.RaisePropertyChanged(nameof(skillVm.SkillValue)));

    private void RemoveMonster() => RemovingMonsterEventInv?.Invoke(_monster.Id);

    public void UpdateLevel()
    {
        this.RaisePropertyChanged(nameof(Level));
        UpdateSkills();
    }
}