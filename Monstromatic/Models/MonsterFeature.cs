using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Monstromatic.Models;

public class MonsterFeature
{  
    /// <summary>
    /// Id особенности
    /// </summary>
    public string Key { get; init; }
        
    /// <summary>
    /// Отображаемое имя
    /// </summary>
    public string DisplayName { get; init; }
        
    /// <summary>
    /// Расширенное отображаемое имя
    /// </summary>
    public string DetailsDisplayName { get; init; }
        
    /// <summary>
    /// Модификатор уровня
    /// </summary>
    public double LevelModifier { get; set; }
        
    /// <summary>
    /// Модификатор атаки
    /// </summary>
    public double AttackModifier { get; set; }
        
    /// <summary>
    /// Модификатор защиты
    /// </summary>
    public double DefenceModifier { get; set; }
        
    /// <summary>
    /// Модификатор здоровья
    /// </summary>
    public double HealthModifier { get; set; }
        
    /// <summary>
    /// Модификатор восприятия
    /// </summary>
    public double PerceptionModifier { get; set; }
        
    /// <summary>
    /// Модификатор воли
    /// </summary>
    public double WillModifier { get; set; }
        
    /// <summary>
    /// Модификатор хитрости
    /// </summary>
    public double TrickeryModifier { get; set; }
        
    /// <summary>
    /// Описание эффекта особенности
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    /// Флаг сокрытия отображения особеннсти
    /// </summary>
    public bool IsHidden { get; init; } = false;

    /// <summary>
    /// Список Id особенностей несовместимых с текущей
    /// </summary>
    [JsonPropertyName("IncompatibleFeatures")]
    public IReadOnlyCollection<string> IncompatibleFeaturesKeys { get; init; } = Array.Empty<string>();
        
    /// <summary>
    /// Список Id особенностей включенных в текущую
    /// </summary>
    [JsonPropertyName("IncludedFeatures")]
    public IReadOnlyCollection<string> IncludedFeaturesKeys { get; init; } = Array.Empty<string>();
        
    /// <summary>
    /// Список Id особенностей исключенных при взятие текущей
    /// </summary>
    [JsonPropertyName("ExcludedFeatures")]
    public IReadOnlyCollection<string> ExcludedFeaturesKeys { get; init; } = Array.Empty<string>();

    [JsonIgnore]
    public IReadOnlyCollection<MonsterFeature> IncompatibleFeatures { get; set; } = Array.Empty<MonsterFeature>();
        
    [JsonIgnore]
    public IReadOnlyCollection<MonsterFeature> IncludedFeatures { get; set; } = Array.Empty<MonsterFeature>();
        
    [JsonIgnore]
    public IReadOnlyCollection<MonsterFeature> ExcludedFeatures { get; set; } = Array.Empty<MonsterFeature>();

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;
        if (obj is MonsterFeature feature)
            return Equals(feature);
        return false;
    }

    protected bool Equals(MonsterFeature other)
    {
        return other.Key == Key;
    }

    public override int GetHashCode() => Key.GetHashCode();
}