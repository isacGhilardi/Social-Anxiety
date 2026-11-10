namespace Social_Anxiety.Core.Challenges;

public interface IChallengeRepository
{
    Task<IReadOnlyList<ChallengeProgress>> GetCompletedLevelsAsync();
    /// <summary>Grava a conclusão do nível. Retorna falso, sem alterar nada, se o nível já estava concluído.</summary>
    Task<bool> TryCompleteLevelAsync(int level, DateTime completedAtUtc);
    Task<IReadOnlyList<Achievement>> GetAchievementsAsync();
    /// <summary>Grava a conquista. Retorna falso, sem alterar nada, se ela já existia.</summary>
    Task<bool> TryAddAchievementAsync(string key, DateTime obtainedAtUtc);
    /// <summary>
    /// Grava o nível e a conquista dele numa única transação, com o mesmo instante.
    /// Retorna falso, sem alterar nada, se o nível já estava concluído; a conquista já existente é mantida.
    /// </summary>
    Task<bool> TryCompleteLevelWithAchievementAsync(int level, string achievementKey, DateTime completedAtUtc);
}
