namespace Social_Anxiety.Core.CheckIns;

public interface ICheckInRepository
{
    /// <summary>Grava o check-in, substituindo o que já existir no mesmo dia local.</summary>
    Task SaveForDayAsync(CheckIn checkIn);
    Task<CheckIn?> GetByDayAsync(DateOnly localDay);
    /// <summary>Dias locais de <paramref name="from"/> a <paramref name="to"/>, inclusive, do mais recente ao mais antigo.</summary>
    Task<IReadOnlyList<CheckIn>> GetRangeAsync(DateOnly from, DateOnly to);
    Task<CheckIn?> GetByIdAsync(Guid id);
    /// <summary>Todos, do mais recente ao mais antigo.</summary>
    Task<IReadOnlyList<CheckIn>> GetAllAsync();
    /// <summary>Se existe ao menos um check-in, sem carregar os registros.</summary>
    Task<bool> HasAnyAsync();
}
