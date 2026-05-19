using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Monstromatic.Models;

/// <summary>
/// Навык существа
/// </summary>
public class Skill
{
    /// <summary>
    /// Минимальное значение навыка 
    /// </summary>
    private const int MinValue = 0;

    private readonly double _standardModifier;

    /// <summary>
    /// Модификаторы навыка от особенностей
    /// </summary>
    private readonly IReadOnlyCollection<double> _featureModifiers;

    /// <summary>
    /// Ручная поправка навыка
    /// </summary>
    private int _manualDelta;

    private int _level;

    public Skill(string name, int level, double standardModifier, IEnumerable<double>? featureModifiers = null)
        : this(name, name, level, standardModifier, featureModifiers)
    {
    }

    public Skill(
        string name,
        string tag,
        int level,
        double standardModifier,
        IEnumerable<double>? featureModifiers = null,
        IEnumerable<SkillComment>? comments = null)
    {
        Name = name;
        Tag = tag;
        Level = level;
        _standardModifier = standardModifier;
        _featureModifiers = featureModifiers?.ToArray() ?? [];
        Comments = comments?.ToArray() ?? [];
    }

    /// <summary>
    /// Название навыка
    /// </summary>
    public string Name { get; }

    public string Tag { get; }

    public IReadOnlyCollection<SkillComment> Comments { get; }

    /// <summary>
    /// Значение навыка
    /// </summary>
    public int Value => GetValue();

    /// <summary>
    /// Уровень существа владеющего навыком
    /// </summary>
    public int Level
    {
        get => _level;
        set
        {
            MonsterLevelRules.ValidateEvenLevel(value);
            _level = value;
        }
    }

    /// <summary>
    /// Увеличить значение навыка
    /// </summary>
    public void Increment() => _manualDelta++;

    /// <summary>
    /// Уменьшить значение навыка
    /// </summary>
    public void Decrement()
    {
        if (Value - 1 >= MinValue)
            _manualDelta--;
    }

    public void Reset() => _manualDelta = 0;

    private int GetValue()
    {
        var value = Level
                    + CalculateDelta(Level, _standardModifier)
                    + _featureModifiers.Sum(modifier => CalculateDelta(Level, modifier))
                    + _manualDelta;

        return value < MinValue ? MinValue : value;
    }

    private static int CalculateDelta(int level, double modifier)
    {
        var delta = level * (Convert.ToDecimal(modifier) - 1m);

        if (delta != decimal.Truncate(delta))
            throw new ValidationException(
                $"Skill modifier {modifier} produces non-integer delta {delta} for monster level {level}.");

        return decimal.ToInt32(delta);
    }
}
