namespace Social_Anxiety.Core.CheckIns;

/// <summary>Regras do check-in: nota de 1 a 5 e um registro por dia local. Os registros ficam no aparelho, sem vínculo com usuário.</summary>
public sealed class CheckInService(ICheckInRepository repository, TimeProvider clock)
{
    public DateOnly Today() => DateOnly.FromDateTime(clock.GetLocalNow().DateTime);

    /// <summary>Salva a nota de hoje. Se já houver check-in hoje, ele é substituído e mantém o mesmo id.</summary>
    public async Task<CheckIn> SaveTodayAsync(int score)
    {
        if (!CheckIn.IsValidScore(score))
            throw new ArgumentOutOfRangeException(nameof(score), score, "A nota precisa estar entre 1 e 5.");

        var now = clock.GetUtcNow();
        var localDay = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, clock.LocalTimeZone).DateTime);
        var previous = await repository.GetByDayAsync(localDay);
        var checkIn = new CheckIn(previous?.Id ?? Guid.NewGuid(), score, now.UtcDateTime, localDay);
        await repository.SaveForDayAsync(checkIn);
        return checkIn;
    }

    public Task<CheckIn?> GetTodayAsync() => repository.GetByDayAsync(Today());

    /// <summary>Últimos <paramref name="days"/> dias locais terminando hoje, do mais recente ao mais antigo.</summary>
    public Task<IReadOnlyList<CheckIn>> GetLastDaysAsync(int days)
    {
        if (days < 1) throw new ArgumentOutOfRangeException(nameof(days));
        var today = Today();
        return repository.GetRangeAsync(today.AddDays(1 - days), today);
    }

    public Task<CheckIn?> GetByIdAsync(Guid id) => repository.GetByIdAsync(id);
    public Task<IReadOnlyList<CheckIn>> GetAllAsync() => repository.GetAllAsync();
}
