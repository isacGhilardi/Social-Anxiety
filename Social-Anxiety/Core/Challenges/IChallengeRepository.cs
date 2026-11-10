namespace Social_Anxiety.Core.Challenges;

public interface IChallengeRepository
{
    Task<IReadOnlyList<ChallengeProgress>> GetCompletedLevelsAsync();
    Task<IReadOnlyList<Achievement>> GetAchievementsAsync();
    /// <summary>
    /// Grava o nível e a conquista dele numa única transação, com o mesmo instante.
    /// Retorna falso, sem alterar nada, se o nível já estava concluído; a conquista já existente é mantida.
    /// </summary>
    Task<bool> TryCompleteLevelWithAchievementAsync(int level, string achievementKey, DateTime completedAtUtc);
}
