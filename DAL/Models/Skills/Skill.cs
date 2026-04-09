namespace DAL.Models.Skills;

public record Skill
{
    public required string Key { get; init; }
    public required string Name { get; init; }
    public required SkillModificator? DefaultModifier { get; init; }
}
