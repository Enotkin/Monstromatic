namespace Monstromatic.Models;

public class Skill 
{
    private readonly int _featuresModificator;
    private int _modificator = 0;
    private int _level;
    
    public Skill(string name, int level, int featuresModificator)
    {
        Name = name;
        _level = level;
        _featuresModificator = featuresModificator;
    }
    public string Name { get; }
    public int Value => Level + _modificator + _featuresModificator;
    public int Level
    {
        get => _level;
        set
        {
            _level = value;
            
        }
    }

    public int SetModificator(int value)
    {
        _modificator = value;
        return Value;
    }
}