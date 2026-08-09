using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Monstromatic.Data.Bestiary;
using Monstromatic.Models;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Monstromatic.ViewModels;

public partial class EncounterViewModel : ViewModelBase
{
    private readonly Encounter _encounter;
    private readonly IBestiaryService _bestiaryService;
    
    public EncounterViewModel(
        Encounter encounter,
        IBestiaryService bestiaryService,
        bool canAddToBestiary = true)
    {
        _encounter = encounter;
        _bestiaryService = bestiaryService;
        CanAddToBestiary = canAddToBestiary;
        Monsters = new ObservableCollection<MonsterViewModel>(
            _encounter.Monsters.Select(CreateMonsterViewModel));
        
        Monsters.CollectionChanged += MonstersOnCollectionChanged;
    }

    private Interaction<Unit, Unit> MonsterCreated { get; } = new();

    public Interaction<string, Unit> ShowBestiaryMessage { get; } = new();

    public string Name => _encounter.Name;

    public bool CanAddToBestiary { get; }

    public int Level
    {
        get => _encounter.Level;
        set
        {
            MonsterLevelRules.ValidateEvenLevel(value);
            _encounter.Level = value;
            this.RaisePropertyChanged();
        }
    }
    public ObservableCollection<MonsterViewModel> Monsters { get; set; }
    
    public IEnumerable<MonsterFeature> DescriptiveFeatures =>
        _encounter.Features.Where(f => !string.IsNullOrEmpty(f.Description));
    
    private void MonstersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => MonsterCreated.Handle(Unit.Default);

    [ReactiveCommand]
    private void AddMonster()
    {
        var monster = _encounter.AddMonster();
        Monsters.Add(CreateMonsterViewModel(monster));
    }

    [ReactiveCommand]
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

    [ReactiveCommand]
    private void AddLevel() => Level += 2;

    [ReactiveCommand]
    private void RemoveLevel() => Level -= 2;

    [ReactiveCommand]
    private void ResetModifications()
    {
        _encounter.ApplyLevelToMonsters();

        foreach (var monsterViewModel in Monsters)      
        {
            monsterViewModel.UpdateLevelAndSkills();
        }
        this.RaisePropertyChanged(nameof(Monsters));
    }

    [ReactiveCommand]
    private async Task AddToBestiary()
    {
        if (!_bestiaryService.TryAdd(_encounter))
        {
            await ShowBestiaryMessage.Handle("Такой монстр уже есть в бестиарии");
        }
    }

    private MonsterViewModel GetMonsterViewModelForRemoving(Guid monsterId)
    {
        return monsterId == Guid.Empty 
            ? Monsters.OrderBy(m => m.Name.Last()).Last()   
            : Monsters.First(m => m.Id == monsterId);
    }

    private MonsterViewModel CreateMonsterViewModel(Monster monster)
    {
        var monsterViewModel = new MonsterViewModel(monster);
        monsterViewModel.RemovingMonsterEventInv += RemoveMonster;
        return monsterViewModel;
    }
}
