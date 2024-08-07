using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using Monstromatic.Models;
using ReactiveUI;

namespace Monstromatic.ViewModels;

public class EncounterViewModel : ViewModelBase
{
    private readonly Encounter _encounter;
    
    public EncounterViewModel(Encounter encounter)
    {
        _encounter = encounter;
        var monsterViewModel = new MonsterViewModel(_encounter.Monsters.First());
        monsterViewModel.RemovingMonsterEventInv += RemoveMonster;
        Monsters = new ObservableCollection<MonsterViewModel> { monsterViewModel };
        
        Monsters.CollectionChanged += MonstersOnCollectionChanged;

        AddMonsterCommand = ReactiveCommand.Create(AddMonster);
        RemoveMonsterCommand = ReactiveCommand.Create<Guid>(RemoveMonster);

        AddLevelCommand = ReactiveCommand.Create(() =>
        {
            Level++;
        });
        
        RemoveLevelCommand = ReactiveCommand.Create(() =>
        {
            Level--;
        });

        // this.WhenValueChanged(vm => vm.Level).Subscribe(_ => UpdateLevels());
    }
    
    public Interaction<Unit, Unit> MonsterCreated { get; } = new();

    public string Name => _encounter.Name;

    public int Level
    {
        get => _encounter.Level;
        set
        {
            _encounter.Level = value;
            this.RaisePropertyChanged();
            foreach (var monsterVm in Monsters)
            {
                monsterVm.UpdateLevel();
            }
        }
    }
    
    public int MonsterCount => _encounter.Monsters.Count;

    public ObservableCollection<MonsterViewModel> Monsters { get; set; }

    public ReactiveCommand<Unit, Unit> AddMonsterCommand { get; }
    
    public ReactiveCommand<Guid, Unit> RemoveMonsterCommand { get; set; }
    
    public ReactiveCommand<Unit, Unit> AddLevelCommand { get; set; }
    
    public ReactiveCommand<Unit, Unit> RemoveLevelCommand { get; set; }
    
    public IEnumerable<MonsterFeature> DescriptiveFeatures =>
        _encounter.Features.Where(f => !string.IsNullOrEmpty(f.Description));
    
    private void MonstersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        MonsterCreated.Handle(Unit.Default);
    }
    
    private void AddMonster()
    {
        var monster = _encounter.AddMonster();
        var monsterViewModel = new MonsterViewModel(monster);
        monsterViewModel.RemovingMonsterEventInv += RemoveMonster;
        Monsters.Add(monsterViewModel);
    }

    private void RemoveMonster(Guid monsterId = default)
    {
        if (Monsters.Count == 0)
        {
            return;
        }
        var removingMonsterViewModel = GetMonsterViewModelForRemoving(monsterId);
        Monsters.Remove(removingMonsterViewModel);
        _encounter.RemoveMonster(monsterId);
    }

    private MonsterViewModel GetMonsterViewModelForRemoving(Guid monsterId)
    {
        return monsterId == Guid.Empty 
            ? Monsters.OrderBy(m => m.Name.Last()).Last() 
            : Monsters.First(m => m.Id == monsterId);
    }
}