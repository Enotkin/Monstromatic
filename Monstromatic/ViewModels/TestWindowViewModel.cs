using System;
using System.Collections.Generic;
using Monstromatic.Data;
using Monstromatic.Data.AppSettingsProvider;
using Monstromatic.Data.Interfaces;
using Monstromatic.Models;
using Monstromatic.Utils;
using ReactiveUI.Fody.Helpers;

namespace Monstromatic.ViewModels;

public class TestWindowViewModel : ViewModelBase
{
    private readonly IAppSettingsProvider _settingsProvider;
    public IProcessHelper ProcessHelper { get; }
    public MonsterViewModel MonsterViewModel { get; }
        
    [Reactive]
    public string Pidor { get; set; }

    public TestWindowViewModel(IAppSettingsProvider settingsProvider, IProcessHelper processHelper) : this()
    {
        ProcessHelper = processHelper;
        _settingsProvider = settingsProvider;
    }

    public TestWindowViewModel()
    {
        Pidor = "ТЫ ПИДОР";
        var monster = new Monster(2, Pidor, new FeaturesBundle(new List<MonsterFeature>()));
        MonsterViewModel = new MonsterViewModel(monster);
    }
}