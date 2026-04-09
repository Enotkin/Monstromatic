namespace DAL.Models.Features;

public record Feature
{
    public required string Key { get; init; }
    public required string DisplayName { get; init; }
    public required string Description { get; init; }
    public required FeatureModificator[] Modificators { get; init; } = [];
}
