namespace Monstromatic.Models;

public class Skill
{
    private const int MinValue = 1;
    private readonly int _featuresModificator;
    
    public Skill(string name, int level, int featuresModificator)
    {
        Name = name;
        Level = level;
        Modificator = 0;
        _featuresModificator = featuresModificator;
    }
    public string Name { get; }
    public int Value => GetValue();
    private int Modificator { get; set; }
    public int Level { get; set; }
    
    public void Increment()
    {
        Modificator++;
    }

    public void Decrement()
    {
        if (Value - 1 >= MinValue)
            Modificator--;
    }

    private int GetValue()
    {
        var value = Level + Modificator + _featuresModificator;
        return value < MinValue ? MinValue : value;
    }
}