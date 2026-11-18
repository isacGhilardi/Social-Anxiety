namespace Social_Anxiety.Core.Profile;

public static class StreakCalculator
{
    /// <summary>
    /// Dias seguidos com check-in. Se hoje ainda não tem check-in, a sequência continua valendo
    /// a partir de ontem (só zera quando passa um dia inteiro sem registro).
    /// </summary>
    public static int Calculate(IEnumerable<DateOnly> checkInDays, DateOnly today)
    {
        var days = checkInDays.ToHashSet();
        var day = days.Contains(today) ? today : today.AddDays(-1);

        var streak = 0;
        while (days.Contains(day))
        {
            streak++;
            day = day.AddDays(-1);
        }
        return streak;
    }
}
