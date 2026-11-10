using SQLite;
using Social_Anxiety.Core.Challenges;

namespace Social_Anxiety.Data;

[Table("challenge_progress")]
internal sealed class ChallengeProgressRow
{
    [PrimaryKey] public Guid Id { get; set; }
    [NotNull, Unique(Name = "ux_challenge_progress_level")] public int Level { get; set; }
    public DateTime CompletedAtUtc { get; set; }

    public ChallengeProgress ToProgress() => new(Id, Level, DateTime.SpecifyKind(CompletedAtUtc, DateTimeKind.Utc));
}
