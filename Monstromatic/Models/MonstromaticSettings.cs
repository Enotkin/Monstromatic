using System.Collections.Generic;

namespace Monstromatic.Models
{
    public class MonstromaticSettings
    {
        public Dictionary<string, int> MonsterQualities { get; init; }
        
        public Dictionary<string, double> SkillDefaultModifiers { get; init; }
    }
}