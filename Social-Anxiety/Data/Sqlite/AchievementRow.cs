using SQLite;
using Social_Anxiety.Core.Challenges;

namespace Social_Anxiety.Data;

[Table("achievements")]
internal sealed class AchievementRow
{
    [PrimaryKey] public Guid Id { get; set; }
    [NotNull, Unique(Name = "ux_achievements_key")] public string Key { get; set; } = "";
    public DateTime ObtainedAtUtc { get; set; }

    public Achievement ToAchievement() => new(Id, Key, DateTime.SpecifyKind(ObtainedAtUtc, DateTimeKind.Utc));
}
