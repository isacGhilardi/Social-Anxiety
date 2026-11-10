namespace Social_Anxiety.Core.Challenges;

public interface IChallengeRepository
{
    Task<IReadOnlyList<ChallengeProgress>> GetCompletedLevelsAsync();
    /// <summary>Grava a conclusão do nível. Retorna falso, sem alterar nada, se o nível já estava concluído.</summary>
    Task<bool> TryCompleteLevelAsync(int level, DateTime completedAtUtc);
    Task<IReadOnlyList<Achievement>> GetAchievementsAsync();
    /// <summary>Grava a conquista. Retorna falso, sem alterar nada, se ela já existia.</summary>
    Task<bool> TryAddAchievementAsync(string key, DateTime obtainedAtUtc);
}
