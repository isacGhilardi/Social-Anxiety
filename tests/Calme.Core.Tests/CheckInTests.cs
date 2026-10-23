using Social_Anxiety.Core.CheckIns;
using Social_Anxiety.Data;

namespace Calme.Core.Tests;

sealed class MemoryCheckIns : ICheckInRepository
{
    public readonly List<CheckIn> Items = [];
    public Task SaveForDayAsync(CheckIn checkIn)
    {
        Items.RemoveAll(c => c.LocalDay == checkIn.LocalDay || c.Id == checkIn.Id);
        Items.Add(checkIn);
        return Task.CompletedTask;
    }
    public Task<CheckIn?> GetByDayAsync(DateOnly localDay) => Task.FromResult(Items.FirstOrDefault(c => c.LocalDay == localDay));
    public Task<IReadOnlyList<CheckIn>> GetRangeAsync(DateOnly from, DateOnly to) =>
        Task.FromResult<IReadOnlyList<CheckIn>>(Items.Where(c => c.LocalDay >= from && c.LocalDay <= to).OrderByDescending(c => c.LocalDay).ToArray());
    public Task<CheckIn?> GetByIdAsync(Guid id) => Task.FromResult(Items.FirstOrDefault(c => c.Id == id));
    public Task<IReadOnlyList<CheckIn>> GetAllAsync() => Task.FromResult<IReadOnlyList<CheckIn>>(Items.OrderByDescending(c => c.LocalDay).ToArray());
}

public class CheckInServiceTests
{
    internal static readonly TimeZoneInfo Brasilia = TimeZoneInfo.CreateCustomTimeZone("UTC-3", TimeSpan.FromHours(-3), "UTC-3", "UTC-3");

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public async Task Nota_fora_de_1_a_5_e_recusada_sem_gravar(int score)
    {
        var repository = new MemoryCheckIns();
        var service = new CheckInService(repository, new TestClock());
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.SaveTodayAsync(score));
        Assert.Empty(repository.Items);
    }

    [Theory]
    [InlineData(1, "Pouca")]
    [InlineData(2, "Leve")]
    [InlineData(3, "Moderada")]
    [InlineData(4, "Intensa")]
    [InlineData(5, "Muito intensa")]
    public async Task Rotulo_e_calculado_a_partir_da_nota(int score, string label)
    {
        var service = new CheckInService(new MemoryCheckIns(), new TestClock());
        var checkIn = await service.SaveTodayAsync(score);
        Assert.Equal(label, checkIn.ScoreLabel);
        Assert.Equal($"{score} de 5 · {label}", checkIn.Summary);
    }

    [Fact]
    public async Task Salvar_de_novo_no_mesmo_dia_substitui_e_mantem_o_id()
    {
        var repository = new MemoryCheckIns();
        var clock = new TestClock(new DateTimeOffset(2026, 10, 23, 9, 0, 0, TimeSpan.Zero));
        var service = new CheckInService(repository, clock);
        var first = await service.SaveTodayAsync(2);
        clock.Now = clock.Now.AddHours(8);
        var second = await service.SaveTodayAsync(4);

        var only = Assert.Single(repository.Items);
        Assert.Equal(4, only.Score);
        Assert.Equal(first.Id, second.Id);
        Assert.Equal(new DateTime(2026, 10, 23, 17, 0, 0, DateTimeKind.Utc), only.RecordedAtUtc);
    }

    [Fact]
    public async Task Check_in_as_22h_em_UTC_menos_3_cai_no_dia_local()
    {
        // 22h de 23/10 em UTC-3 já é 01h de 24/10 em UTC.
        var clock = new TestClock(new DateTimeOffset(2026, 10, 24, 1, 0, 0, TimeSpan.Zero), Brasilia);
        var service = new CheckInService(new MemoryCheckIns(), clock);
        var checkIn = await service.SaveTodayAsync(3);
        Assert.Equal(new DateOnly(2026, 10, 23), checkIn.LocalDay);
        Assert.Equal(new DateTime(2026, 10, 24, 1, 0, 0, DateTimeKind.Utc), checkIn.RecordedAtUtc);
        Assert.Equal(DateTimeKind.Utc, checkIn.RecordedAtUtc.Kind);
        Assert.Equal(new DateOnly(2026, 10, 23), service.Today());
    }

    [Fact]
    public async Task Dia_seguinte_gera_um_novo_registro()
    {
        var repository = new MemoryCheckIns();
        var clock = new TestClock(new DateTimeOffset(2026, 10, 23, 12, 0, 0, TimeSpan.Zero));
        var service = new CheckInService(repository, clock);
        await service.SaveTodayAsync(2);
        clock.Now = clock.Now.AddDays(1);
        await service.SaveTodayAsync(3);
        Assert.Equal(2, repository.Items.Count);
    }

    [Fact]
    public async Task Ultimos_dias_incluem_hoje_e_excluem_o_dia_fora_do_intervalo()
    {
        var repository = new MemoryCheckIns();
        var today = new DateOnly(2026, 10, 23);
        foreach (var daysAgo in new[] { 0, 6, 7, 29, 30 })
            repository.Items.Add(new CheckIn(Guid.NewGuid(), 2, DateTime.UtcNow, today.AddDays(-daysAgo)));
        var service = new CheckInService(repository, new TestClock(new DateTimeOffset(2026, 10, 23, 12, 0, 0, TimeSpan.Zero)));

        Assert.Equal(2, (await service.GetLastDaysAsync(7)).Count);
        Assert.Equal(4, (await service.GetLastDaysAsync(30)).Count);
    }
}

public class CheckInRepositoryTests
{
    private static CheckIn At(DateOnly day, int score, Guid? id = null) =>
        new(id ?? Guid.NewGuid(), score, new DateTime(2026, 10, 23, 15, 30, 0, DateTimeKind.Utc), day);

    [Fact]
    public async Task Mesmo_dia_com_outro_id_substitui_o_registro_anterior()
    {
        await using var temp = new TempDatabase();
        var repository = new CheckInRepository(temp.Database);
        var day = new DateOnly(2026, 10, 23);
        await repository.SaveForDayAsync(At(day, 2));
        await repository.SaveForDayAsync(At(day, 5));

        var all = await repository.GetAllAsync();
        Assert.Equal(5, Assert.Single(all).Score);
    }

    [Fact]
    public async Task Indice_unico_impede_dois_registros_no_mesmo_dia()
    {
        await using var temp = new TempDatabase();
        var db = await temp.Database.GetAsync();
        await db.ExecuteAsync("insert into checkins (Id, Score, RecordedAtUtc, LocalDay) values (?, 1, 0, '2026-10-23')", Guid.NewGuid());
        await Assert.ThrowsAsync<SQLite.SQLiteException>(() =>
            db.ExecuteAsync("insert into checkins (Id, Score, RecordedAtUtc, LocalDay) values (?, 2, 0, '2026-10-23')", Guid.NewGuid()));
    }

    [Fact]
    public async Task Busca_por_dia_id_e_intervalo_preservando_utc_e_ordem()
    {
        await using var temp = new TempDatabase();
        var repository = new CheckInRepository(temp.Database);
        var today = new DateOnly(2026, 10, 23);
        var saved = At(today, 3);
        await repository.SaveForDayAsync(saved);
        await repository.SaveForDayAsync(At(today.AddDays(-6), 1));
        await repository.SaveForDayAsync(At(today.AddDays(-7), 4));

        var byId = await repository.GetByIdAsync(saved.Id);
        Assert.Equal(saved, byId);
        Assert.Equal(DateTimeKind.Utc, byId!.RecordedAtUtc.Kind);
        Assert.Equal(saved.Id, (await repository.GetByDayAsync(today))?.Id);
        Assert.Null(await repository.GetByDayAsync(today.AddDays(-1)));

        var week = await repository.GetRangeAsync(today.AddDays(-6), today);
        Assert.Equal([today, today.AddDays(-6)], week.Select(c => c.LocalDay));
        var all = await repository.GetAllAsync();
        Assert.Equal([today, today.AddDays(-6), today.AddDays(-7)], all.Select(c => c.LocalDay));
    }

    [Fact]
    public async Task Registros_continuam_ao_reabrir_o_banco()
    {
        await using var temp = new TempDatabase();
        var day = new DateOnly(2026, 10, 23);
        await new CheckInRepository(temp.Database).SaveForDayAsync(At(day, 4));
        await temp.Database.CloseAsync();

        var reopened = new CalmeDatabase(temp.FilePath);
        var found = await new CheckInRepository(reopened).GetByDayAsync(day);
        await reopened.CloseAsync();
        Assert.Equal(4, found?.Score);
    }
}
