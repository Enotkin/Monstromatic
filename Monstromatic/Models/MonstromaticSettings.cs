using System.Collections.Generic;

namespace Monstromatic.Models
{
    public class MonstromaticSettings
    {
        public Dictionary<string, int> MonsterQualities { get; set; }
        
        public DefaultModifiers DefaultModifiers { get; set; }
    }

    public class DefaultModifiers
    {
        public int AttackModifier { get; set; }
        public int DefenceModifier { get; set; }
        public int HealthModifier { get; set; }
        public int KnowledgeModifier { get; set; }
        public int TemperModifier { get; set; }
        public int TrickeryModifier { get; set; }
    }
}