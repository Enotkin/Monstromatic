using System.Collections.Generic;
using Monstromatic.Models;
using Monstromatic.Utils;

namespace Monstromatic.ViewModels.Design
{
    public class DesignVmLocator
    {
        private readonly MonsterFeature _monsterFeature = new() { Key = "Test", DisplayName = "Test Display Name" };
        
        public MonsterViewModel MonsterViewModel => new(new Monster(1, "Test", new FeaturesBundle(new List<MonsterFeature>()
        {
            _monsterFeature
        })));
        public SkillCounterViewModel SkillCounterViewModel => new(new Skill("TestName", 1, 1));
        public MonsterDetailsViewModel DetailsVm => new(
            "TestName",
            5,
            new List<MonsterFeature>()
            {
                _monsterFeature
            });

        public EncounterViewModel EncounterViewModel => new(new Encounter("TestName", 1, new List<MonsterFeature>()
        {
            _monsterFeature
        }));
        
        public FeatureViewModel FeatureVm => new(new MonsterFeature(){Key = "Test", DisplayName = "Test Display Name"}, new FeatureController());

        public MainWindowViewModel MainWindowVM => ServiceHub.Default.ServiceProvider.Get<MainWindowViewModel>();
    }
}
