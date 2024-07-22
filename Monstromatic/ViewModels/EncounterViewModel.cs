﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Interactivity;
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
        Monsters = new ObservableCollection<MonsterViewModel> { monsterViewModel };
        
        Monsters.CollectionChanged += MonstersOnCollectionChanged;

        AddMonsterCommand = ReactiveCommand.Create(AddMonster);
    }
    
    public Interaction<Unit, Unit> MonsterCreated { get; } = new();

    public string Name => _encounter.Name;

    public int Level
    {
        get => _encounter.Level;
        set => _encounter.Level = value;
    }

    public ObservableCollection<MonsterViewModel> Monsters { get; set; }

    public ReactiveCommand<Unit, Unit> AddMonsterCommand { get; }
    
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
        Monsters.Add(monsterViewModel);
    }
}