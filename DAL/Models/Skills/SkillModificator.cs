namespace DAL.Models.Skills;

public record SkillModificator
{
    public OperatorType OperatorType { get; init; }
    public double Value { get; init; }
}