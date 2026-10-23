using System.Globalization;
using SQLite;
using Social_Anxiety.Core.CheckIns;

namespace Social_Anxiety.Data;

[Table("checkins")]
internal sealed class CheckInRow
{
    internal const string DayFormat = "yyyy-MM-dd";

    [PrimaryKey] public Guid Id { get; set; }
    public int Score { get; set; }
    public DateTime RecordedAtUtc { get; set; }
    // Texto ISO (aaaa-mm-dd): ordena e compara corretamente; o índice único garante um check-in por dia.
    [NotNull, Unique(Name = "ux_checkins_local_day")] public string LocalDay { get; set; } = "";

    public CheckIn ToCheckIn() => new(Id, Score, DateTime.SpecifyKind(RecordedAtUtc, DateTimeKind.Utc),
        DateOnly.ParseExact(LocalDay, DayFormat, CultureInfo.InvariantCulture));

    public static CheckInRow From(CheckIn checkIn) => new()
    {
        Id = checkIn.Id, Score = checkIn.Score, RecordedAtUtc = checkIn.RecordedAtUtc, LocalDay = Format(checkIn.LocalDay)
    };

    public static string Format(DateOnly day) => day.ToString(DayFormat, CultureInfo.InvariantCulture);
}
