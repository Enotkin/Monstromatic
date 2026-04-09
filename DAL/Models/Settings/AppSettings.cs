using DAL.Models.Features;
using DAL.Models.Skills;

namespace DAL.Models.Settings;

public record AppSettings
{
    public required Dictionary<string, int> MonsterQualities { get; init; } = [];
    public required Skill[] Skills { get; init; } = [];
    public required Feature[] Features { get; init; } = [];
}
