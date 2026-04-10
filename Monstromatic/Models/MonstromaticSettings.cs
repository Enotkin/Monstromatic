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
        public double AttackModifier { get; set; }
        public double DefenceModifier { get; set; }
        public double BraveryModifier { get; set; }
        public double TrickeryModifier { get; set; }
    }
}