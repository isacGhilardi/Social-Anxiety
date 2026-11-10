using Social_Anxiety.Core.CheckIns;

namespace Social_Anxiety.Core.Challenges;

/// <summary>
/// Regras da trilha e das conquistas. O nível 1 e "Primeiro cuidado" são calculados (existe ao menos um check-in);
/// só os níveis 2 e 3 e suas conquistas são gravados. Aceitar um desafio não grava nada; cada conclusão conta uma vez.
/// </summary>
public sealed class ChallengeService(IChallengeRepository challenges, ICheckInRepository checkIns, TimeProvider clock)
{
    public const string FirstCareKey = "primeiro-cuidado";

    // Ordem do protótipo. Level: nível que libera a conquista (null = calculada ou em breve).
    private static readonly (string Key, string Name, int? Level, bool ComingSoon)[] Achievements =
    [
        (FirstCareKey, "Primeiro cuidado", null, false),
        ("tempo-para-mim", "Tempo para mim", null, true),
        ("pequeno-passo", "Pequeno passo", 2, false),
        ("uma-conversa", "Uma conversa", 3, false),
        ("meu-ritmo", "Meu ritmo", null, true),
        ("minha-jornada", "Minha jornada", null, true),
    ];

    /// <summary>Os 3 níveis, em ordem, com o estado calculado a partir de uma leitura dos check-ins e uma do progresso.</summary>
    public async Task<IReadOnlyList<TrailItem>> GetTrailAsync()
    {
        var hasCheckIn = await checkIns.HasAnyAsync();
        var completed = (await challenges.GetCompletedLevelsAsync()).Select(p => p.Level).ToHashSet();

        var trail = new List<TrailItem>(ChallengeCatalog.All.Count);
        var previousDone = true;
        foreach (var level in ChallengeCatalog.All)
        {
            var done = level.Number == 1 ? hasCheckIn : completed.Contains(level.Number);
            var state = done ? TrailState.Completed : previousDone ? TrailState.Available : TrailState.Locked;
            trail.Add(new TrailItem(level, state));
            previousDone = done;
        }
        return trail;
    }

    public ChallengeLevel? GetLevel(int level) => ChallengeCatalog.Find(level);

    /// <summary>Conclui o nível 2 ou 3 se o anterior estiver concluído. Grava nível e conquista com o mesmo instante UTC.</summary>
    public async Task<CompleteResult> CompleteLevelAsync(int level)
    {
        if (level is not (2 or 3)) return new(CompleteStatus.Invalid, null); // o nível 1 conclui sozinho

        var previousDone = level == 2
            ? await checkIns.HasAnyAsync()
            : (await challenges.GetCompletedLevelsAsync()).Any(p => p.Level == level - 1);
        if (!previousDone) return new(CompleteStatus.Locked, null);

        var now = clock.GetUtcNow().UtcDateTime;
        if (!await challenges.TryCompleteLevelAsync(level, now)) return new(CompleteStatus.AlreadyCompleted, null);

        var achievement = Achievements.First(a => a.Level == level);
        await challenges.TryAddAchievementAsync(achievement.Key, now);
        return new(CompleteStatus.CompletedNow, new AchievementInfo(achievement.Key, achievement.Name, true, now, false));
    }

    /// <summary>As 6 conquistas na ordem do protótipo.</summary>
    public async Task<IReadOnlyList<AchievementInfo>> GetAchievementsAsync()
    {
        var hasCheckIn = await checkIns.HasAnyAsync();
        var saved = (await challenges.GetAchievementsAsync()).ToDictionary(a => a.Key, a => (DateTime?)a.ObtainedAtUtc);

        return Achievements.Select(a =>
        {
            if (a.Key == FirstCareKey) return new AchievementInfo(a.Key, a.Name, hasCheckIn, null, false);
            var obtainedAt = saved.GetValueOrDefault(a.Key);
            return new AchievementInfo(a.Key, a.Name, obtainedAt is not null, obtainedAt, a.ComingSoon);
        }).ToArray();
    }
}
