using Social_Anxiety.Core.CheckIns;
using Social_Anxiety.Core.Challenges;

namespace Social_Anxiety.Core.Profile;

public sealed class ProfileStatsService(CheckInService checkIns, ChallengeService challenges)
{
    public async Task<ProfileStats> GetAsync()
    {
        var all = await checkIns.GetAllAsync();
        var streak = StreakCalculator.Calculate(all.Select(c => c.LocalDay), checkIns.Today());

        // Nível 1 já vem como concluído do próprio serviço (calculado a partir dos check-ins).
        var trail = await challenges.GetTrailAsync();
        var completed = trail.Count(level => level.State == TrailState.Completed);

        return new ProfileStats(streak, completed);
    }
}
