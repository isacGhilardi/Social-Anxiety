using SQLite;
using Social_Anxiety.Core.Challenges;

namespace Social_Anxiety.Data;

// Usa as tabelas challenge_progress e achievements do cartão #16; os índices únicos garantem uma gravação por nível e por conquista.
public sealed class ChallengeRepository(CalmeDatabase database) : IChallengeRepository
{
    public async Task<IReadOnlyList<ChallengeProgress>> GetCompletedLevelsAsync()
    {
        var db = await database.GetAsync();
        var rows = await db.QueryAsync<ChallengeProgressRow>("select * from challenge_progress order by Level");
        return rows.Select(r => r.ToProgress()).ToArray();
    }

    public async Task<bool> TryCompleteLevelAsync(int level, DateTime completedAtUtc) =>
        await TryInsertAsync(new ChallengeProgressRow { Id = Guid.NewGuid(), Level = level, CompletedAtUtc = completedAtUtc });

    public async Task<IReadOnlyList<Achievement>> GetAchievementsAsync()
    {
        var db = await database.GetAsync();
        var rows = await db.QueryAsync<AchievementRow>("select * from achievements");
        return rows.Select(r => r.ToAchievement()).ToArray();
    }

    public async Task<bool> TryAddAchievementAsync(string key, DateTime obtainedAtUtc) =>
        await TryInsertAsync(new AchievementRow { Id = Guid.NewGuid(), Key = key, ObtainedAtUtc = obtainedAtUtc });

    public async Task<bool> TryCompleteLevelWithAchievementAsync(int level, string achievementKey, DateTime completedAtUtc)
    {
        var db = await database.GetAsync();
        try
        {
            // Erro dentro da transação desfaz tudo: o índice único do nível barra a segunda conclusão.
            await db.RunInTransactionAsync(connection =>
            {
                connection.Insert(new ChallengeProgressRow { Id = Guid.NewGuid(), Level = level, CompletedAtUtc = completedAtUtc });
                if (connection.Table<AchievementRow>().Where(a => a.Key == achievementKey).Count() == 0)
                    connection.Insert(new AchievementRow { Id = Guid.NewGuid(), Key = achievementKey, ObtainedAtUtc = completedAtUtc });
            });
            return true;
        }
        catch (SQLiteException error) when (error.Result == SQLite3.Result.Constraint)
        {
            return false;
        }
    }

    private async Task<bool> TryInsertAsync(object row)
    {
        var db = await database.GetAsync();
        try
        {
            await db.InsertAsync(row);
            return true;
        }
        catch (SQLiteException error) when (error.Result == SQLite3.Result.Constraint)
        {
            return false;
        }
    }
}
