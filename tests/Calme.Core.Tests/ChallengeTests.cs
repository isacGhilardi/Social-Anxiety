using Social_Anxiety.Core.Challenges;
using Social_Anxiety.Core.CheckIns;
using Social_Anxiety.Data;

namespace Calme.Core.Tests;

public class ChallengeServiceTests
{
    // 09:30 em Brasília = 12:30 UTC: prova que a gravação usa o instante UTC, não a hora local.
    private static readonly DateTimeOffset Now = new(2026, 11, 10, 9, 30, 0, TimeSpan.FromHours(-3));

    private readonly MemoryChallenges challenges = new();
    private readonly MemoryCheckIns checkIns = new();
    private readonly ChallengeService service;

    public ChallengeServiceTests() =>
        service = new ChallengeService(challenges, checkIns, new TestClock(Now, CheckInServiceTests.Brasilia));

    private void AddCheckIn() => checkIns.Items.Add(new CheckIn(Guid.NewGuid(), 3, Now.UtcDateTime, DateOnly.FromDateTime(Now.DateTime)));
    private void AddLevel(int level) => challenges.Levels.Add(new ChallengeProgress(Guid.NewGuid(), level, Now.UtcDateTime));

    private async Task<TrailState[]> StatesAsync() => (await service.GetTrailAsync()).Select(t => t.State).ToArray();

    [Fact]
    public async Task Trilha_sem_check_in_libera_so_o_nivel_1()
    {
        var trail = await service.GetTrailAsync();
        Assert.Equal([1, 2, 3], trail.Select(t => t.Level.Number));
        Assert.Equal([TrailState.Available, TrailState.Locked, TrailState.Locked], trail.Select(t => t.State));
    }

    [Fact]
    public async Task Trilha_com_check_in_conclui_o_nivel_1_e_libera_o_2()
    {
        AddCheckIn();
        Assert.Equal([TrailState.Completed, TrailState.Available, TrailState.Locked], await StatesAsync());
    }

    [Fact]
    public async Task Trilha_com_nivel_2_gravado_libera_o_3_e_com_2_e_3_conclui_todos()
    {
        AddCheckIn();
        AddLevel(2);
        Assert.Equal([TrailState.Completed, TrailState.Completed, TrailState.Available], await StatesAsync());
        AddLevel(3);
        Assert.Equal([TrailState.Completed, TrailState.Completed, TrailState.Completed], await StatesAsync());
    }

    [Fact]
    public async Task Trilha_faz_uma_leitura_de_cada_repositorio()
    {
        await service.GetTrailAsync();
        Assert.Equal(1, challenges.Reads);
        Assert.Equal(1, checkIns.HasAnyCalls);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(4)]
    public async Task Concluir_nivel_fora_de_2_e_3_e_invalido_sem_gravar(int level)
    {
        AddCheckIn();
        var result = await service.CompleteLevelAsync(level);
        Assert.Equal(new CompleteResult(CompleteStatus.Invalid, null), result);
        Assert.Empty(challenges.Levels);
        Assert.Empty(challenges.Achievements);
    }

    [Fact]
    public async Task Nivel_2_sem_check_in_fica_bloqueado_sem_gravar()
    {
        var result = await service.CompleteLevelAsync(2);
        Assert.Equal(CompleteStatus.Locked, result.Status);
        Assert.Null(result.Achievement);
        Assert.Empty(challenges.Levels);
        Assert.Empty(challenges.Achievements);
    }

    [Fact]
    public async Task Nivel_2_com_check_in_grava_nivel_e_pequeno_passo_no_mesmo_instante_utc()
    {
        AddCheckIn();
        var result = await service.CompleteLevelAsync(2);

        var expectedAt = new DateTime(2026, 11, 10, 12, 30, 0, DateTimeKind.Utc);
        Assert.Equal(CompleteStatus.CompletedNow, result.Status);
        Assert.Equal(new AchievementInfo("pequeno-passo", "Pequeno passo", true, expectedAt, false), result.Achievement);
        var level = Assert.Single(challenges.Levels);
        Assert.Equal((2, expectedAt, DateTimeKind.Utc), (level.Level, level.CompletedAtUtc, level.CompletedAtUtc.Kind));
        var achievement = Assert.Single(challenges.Achievements);
        Assert.Equal(("pequeno-passo", expectedAt), (achievement.Key, achievement.ObtainedAtUtc));
        // Assert.Equal de DateTime ignora o Kind: confere à parte.
        Assert.Equal(DateTimeKind.Utc, achievement.ObtainedAtUtc.Kind);
        Assert.Equal(DateTimeKind.Utc, result.Achievement!.ObtainedAtUtc!.Value.Kind);
    }

    [Fact]
    public async Task Concluir_o_nivel_2_de_novo_nao_grava_nada()
    {
        AddCheckIn();
        await service.CompleteLevelAsync(2);
        var again = await service.CompleteLevelAsync(2);

        Assert.Equal(new CompleteResult(CompleteStatus.AlreadyCompleted, null), again);
        Assert.Equal(1, challenges.LevelWrites);
        Assert.Equal(1, challenges.AchievementWrites);
    }

    [Fact]
    public async Task Nivel_3_antes_do_2_fica_bloqueado_e_depois_do_2_libera_uma_conversa()
    {
        AddCheckIn();
        Assert.Equal(CompleteStatus.Locked, (await service.CompleteLevelAsync(3)).Status);
        Assert.Empty(challenges.Levels);

        await service.CompleteLevelAsync(2);
        var result = await service.CompleteLevelAsync(3);
        Assert.Equal(CompleteStatus.CompletedNow, result.Status);
        Assert.Equal("uma-conversa", result.Achievement?.Key);
        Assert.Equal([2, 3], challenges.Levels.Select(l => l.Level));
    }

    [Fact]
    public async Task Conquistas_vem_na_ordem_do_prototipo_com_em_breve_nas_tres_certas()
    {
        var achievements = await service.GetAchievementsAsync();
        Assert.Equal(["primeiro-cuidado", "tempo-para-mim", "pequeno-passo", "uma-conversa", "meu-ritmo", "minha-jornada"],
            achievements.Select(a => a.Key));
        Assert.Equal(["Primeiro cuidado", "Tempo para mim", "Pequeno passo", "Uma conversa", "Meu ritmo", "Minha jornada"],
            achievements.Select(a => a.Name));
        Assert.Equal(["tempo-para-mim", "meu-ritmo", "minha-jornada"], achievements.Where(a => a.ComingSoon).Select(a => a.Key));
        Assert.All(achievements, a => Assert.False(a.Obtained));
        Assert.Equal(1, challenges.Reads);
        Assert.Equal(1, checkIns.HasAnyCalls);
    }

    [Fact]
    public async Task Primeiro_cuidado_so_e_obtida_com_check_in_e_sem_data()
    {
        AddCheckIn();
        await service.CompleteLevelAsync(2);
        var achievements = await service.GetAchievementsAsync();

        Assert.Equal(new AchievementInfo("primeiro-cuidado", "Primeiro cuidado", true, null, false), achievements[0]);
        Assert.Equal(Now.UtcDateTime, achievements[2].ObtainedAtUtc);
        Assert.Equal(["primeiro-cuidado", "pequeno-passo"], achievements.Where(a => a.Obtained).Select(a => a.Key));
    }

    [Fact]
    public void GetLevel_delega_ao_catalogo() => Assert.Same(ChallengeCatalog.Find(2), service.GetLevel(2));
}

public class ChallengeCatalogTests
{
    [Fact]
    public void Catalogo_tem_3_niveis_com_chaves_unicas()
    {
        Assert.Equal([1, 2, 3], ChallengeCatalog.All.Select(l => l.Number));
        Assert.Equal(["perceber", "aproximar", "conversar"], ChallengeCatalog.All.Select(l => l.Key));
        Assert.Equal(["Perceber", "Aproximar", "Conversar"], ChallengeCatalog.All.Select(l => l.Name));
        Assert.Empty(ChallengeCatalog.Find(1)!.Steps);
        Assert.All(ChallengeCatalog.All.Skip(1), l => Assert.Equal([1, 2, 3], l.Steps.Select(s => s.Number)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public void Find_fora_de_1_a_3_retorna_nulo(int level) => Assert.Null(ChallengeCatalog.Find(level));
}

public class ChallengeSqliteTests
{
    [Fact]
    public async Task HasAnyAsync_falso_com_banco_vazio_e_verdadeiro_com_um_registro()
    {
        await using var temp = new TempDatabase();
        var repository = new CheckInRepository(temp.Database);
        Assert.False(await repository.HasAnyAsync());

        await repository.SaveForDayAsync(new CheckIn(Guid.NewGuid(), 2, DateTime.UtcNow, new DateOnly(2026, 11, 10)));
        Assert.True(await repository.HasAnyAsync());
    }

    [Fact]
    public async Task Repositorio_grava_cada_nivel_e_conquista_uma_vez_em_utc()
    {
        await using var temp = new TempDatabase();
        var repository = new ChallengeRepository(temp.Database);
        var at = new DateTime(2026, 11, 10, 12, 30, 0, DateTimeKind.Utc);

        Assert.True(await repository.TryCompleteLevelAsync(2, at));
        Assert.False(await repository.TryCompleteLevelAsync(2, at.AddHours(1)));
        Assert.True(await repository.TryAddAchievementAsync("pequeno-passo", at));
        Assert.False(await repository.TryAddAchievementAsync("pequeno-passo", at.AddHours(1)));

        var level = Assert.Single(await repository.GetCompletedLevelsAsync());
        Assert.Equal((2, at, DateTimeKind.Utc), (level.Level, level.CompletedAtUtc, level.CompletedAtUtc.Kind));
        var achievement = Assert.Single(await repository.GetAchievementsAsync());
        Assert.Equal(("pequeno-passo", at, DateTimeKind.Utc), (achievement.Key, achievement.ObtainedAtUtc, achievement.ObtainedAtUtc.Kind));
    }

    [Fact]
    public async Task Nivel_e_conquista_sao_gravados_juntos_uma_unica_vez()
    {
        await using var temp = new TempDatabase();
        var repository = new ChallengeRepository(temp.Database);
        var at = new DateTime(2026, 11, 10, 12, 30, 0, DateTimeKind.Utc);
        var earlier = at.AddDays(-1);
        await repository.TryAddAchievementAsync("uma-conversa", earlier);

        Assert.True(await repository.TryCompleteLevelWithAchievementAsync(2, "pequeno-passo", at));
        Assert.False(await repository.TryCompleteLevelWithAchievementAsync(2, "pequeno-passo", at.AddHours(1)));
        // Conquista que já existia é mantida com a data original.
        Assert.True(await repository.TryCompleteLevelWithAchievementAsync(3, "uma-conversa", at));

        Assert.Equal([2, 3], (await repository.GetCompletedLevelsAsync()).Select(l => l.Level));
        var achievements = (await repository.GetAchievementsAsync()).ToDictionary(a => a.Key, a => a.ObtainedAtUtc);
        Assert.Equal(2, achievements.Count);
        Assert.Equal(at, achievements["pequeno-passo"]);
        Assert.Equal(earlier, achievements["uma-conversa"]);
    }

    [Fact]
    public async Task Nivel_ja_gravado_nao_grava_a_conquista()
    {
        await using var temp = new TempDatabase();
        var repository = new ChallengeRepository(temp.Database);
        var at = new DateTime(2026, 11, 10, 12, 30, 0, DateTimeKind.Utc);
        await repository.TryCompleteLevelAsync(2, at);

        Assert.False(await repository.TryCompleteLevelWithAchievementAsync(2, "pequeno-passo", at));
        Assert.Empty(await repository.GetAchievementsAsync());
    }

    [Fact]
    public async Task Niveis_voltam_em_ordem_e_sobrevivem_a_reabrir_o_banco()
    {
        await using var temp = new TempDatabase();
        var at = new DateTime(2026, 11, 10, 12, 30, 0, DateTimeKind.Utc);
        var repository = new ChallengeRepository(temp.Database);
        await repository.TryCompleteLevelAsync(3, at);
        await repository.TryCompleteLevelAsync(2, at);
        await repository.TryAddAchievementAsync("uma-conversa", at);
        Assert.Equal([2, 3], (await repository.GetCompletedLevelsAsync()).Select(l => l.Level));

        var reopened = new CalmeDatabase(temp.FilePath);
        try
        {
            var again = new ChallengeRepository(reopened);
            Assert.Equal([2, 3], (await again.GetCompletedLevelsAsync()).Select(l => l.Level));
            Assert.Equal("uma-conversa", Assert.Single(await again.GetAchievementsAsync()).Key);
            Assert.False(await again.TryCompleteLevelAsync(2, at));
        }
        finally { await reopened.CloseAsync(); }
    }
}
