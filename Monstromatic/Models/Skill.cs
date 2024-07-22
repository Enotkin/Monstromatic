namespace Monstromatic.Models;

/// <summary>
/// Навык существа
/// </summary>
public class Skill(string name, int level, double defaultModificator, double featuresModificator)
{
    public Skill(string name, int level, double featuresModificator) : this(name, level, 1.0, featuresModificator) {}
    
    /// <summary>
    /// Минимальное значение навыка 
    /// </summary>
    private const int MinValue = 1;
    
    /// <summary>
    /// Модификатор навыка от особенностей
    /// </summary>
    private readonly double _featuresModificator = featuresModificator;

    /// <summary>
    /// Суммароное значние модификатора
    /// </summary>
    private int _modificator = 0;

    /// <summary>
    /// Название навыка
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Значение навыка
    /// </summary>
    public int Value => GetValue();
    
    /// <summary>
    /// Уровень существа владеющего навыком
    /// </summary>
    public int Level { get; set; } = level;
    
    /// <summary>
    /// Увеличить значение навыка
    /// </summary>
    public void Increment()
    {
        _modificator++;
    }

    /// <summary>
    /// Уменьшить значение навыка
    /// </summary>
    public void Decrement()
    {
        if (Value - 1 >= MinValue)
            _modificator--;
    }

    private int GetValue()
    {
        var value = (int)(Level * defaultModificator * _featuresModificator+ _modificator);
        return value < MinValue ? MinValue : value;
    }
}