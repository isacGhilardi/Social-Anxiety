using Social_Anxiety.Core.Challenges;

namespace Social_Anxiety;

// Parabéns depois de "Concluí meu desafio". Rota: challengeDone?level=2&achievement=pequeno-passo
public partial class ChallengeDonePage : ContentPage, IQueryAttributable
{
    private readonly ChallengeService service;
    private string? achievementKey;

    public ChallengeDonePage(ChallengeService service)
    {
        this.service = service;
        InitializeComponent();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var level = query.TryGetValue("level", out var value) && int.TryParse(value?.ToString(), out var number) ? number : 0;
        NextLevelNotice.IsVisible = service.GetLevel(level + 1) is not null;
        achievementKey = query.TryGetValue("achievement", out var key) ? key?.ToString() : null;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        AchievementInfo? achievement = null;
        try { achievement = (await service.GetAchievementsAsync()).FirstOrDefault(a => a.Key == achievementKey && a.Obtained); }
        catch (Exception error) when (error is IOException or SQLite.SQLiteException or UnauthorizedAccessException) { }

        // Sem conquista legível, a tela continua válida: só não mostra o cartão.
        AchievementCard.IsVisible = achievement is not null;
        AchievementName.Text = achievement is null ? "" : $"{achievement.Name} · por experimentar um desafio social.";
        SemanticScreenReader.Default.Announce(achievement is null
            ? "Seu esforço merece ser reconhecido. Desafio concluído."
            : $"Seu esforço merece ser reconhecido. Conquista desbloqueada: {achievement.Name}.");
    }

    private async void OnTrailClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//challenges");
    private async void OnHomeClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//home");
    private async void OnSupportClicked(object? sender, EventArgs e) => await App.OpenSupport();
}
