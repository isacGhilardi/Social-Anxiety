using Social_Anxiety.Core.Challenges;

namespace Calme.Core.Tests;

sealed class MemoryChallenges : IChallengeRepository
{
    public readonly List<ChallengeProgress> Levels = [];
    public readonly List<Achievement> Achievements = [];
    public int Reads;
    public int LevelWrites;
    public int AchievementWrites;

    public Task<IReadOnlyList<ChallengeProgress>> GetCompletedLevelsAsync()
    {
        Reads++;
        return Task.FromResult<IReadOnlyList<ChallengeProgress>>(Levels.ToArray());
    }

    public Task<bool> TryCompleteLevelAsync(int level, DateTime completedAtUtc)
    {
        if (Levels.Any(l => l.Level == level)) return Task.FromResult(false);
        LevelWrites++;
        Levels.Add(new ChallengeProgress(Guid.NewGuid(), level, completedAtUtc));
        return Task.FromResult(true);
    }

    public Task<IReadOnlyList<Achievement>> GetAchievementsAsync()
    {
        Reads++;
        return Task.FromResult<IReadOnlyList<Achievement>>(Achievements.ToArray());
    }

    public Task<bool> TryAddAchievementAsync(string key, DateTime obtainedAtUtc)
    {
        if (Achievements.Any(a => a.Key == key)) return Task.FromResult(false);
        AchievementWrites++;
        Achievements.Add(new Achievement(Guid.NewGuid(), key, obtainedAtUtc));
        return Task.FromResult(true);
    }
}
