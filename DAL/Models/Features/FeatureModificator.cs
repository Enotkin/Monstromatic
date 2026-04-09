using DAL.Models.Skills;

namespace DAL.Models.Features;

public record FeatureModificator
{
    public required string SkillKey { get; init; }
    public OperatorType Type { get; init; }
    public double Value { get; init; }
}
