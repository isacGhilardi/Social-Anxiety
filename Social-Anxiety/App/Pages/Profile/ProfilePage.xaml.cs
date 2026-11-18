using Microsoft.Maui.Controls.Shapes;
using Social_Anxiety.Core.Auth;
using Social_Anxiety.Core.Challenges;
using Social_Anxiety.Core.Profile;

namespace Social_Anxiety;

public partial class ProfilePage : ContentPage
{
    private const int Columns = 3;

    // Ícones que já existem em Resources/Images, um por conquista.
    private static readonly Dictionary<string, string> Icons = new()
    {
        ["primeiro-cuidado"] = "leaf.png",
        ["tempo-para-mim"] = "wind.png",
        ["pequeno-passo"] = "heart.png",
        ["uma-conversa"] = "path.png",
        ["meu-ritmo"] = "heart.png",
        ["minha-jornada"] = "path.png"
    };

    private readonly SessionState session;
    private readonly ProfileStatsService stats;
    private readonly ChallengeService challenges;

    public ProfilePage(SessionState session, ProfileStatsService stats, ChallengeService challenges)
    {
        this.session = session;
        this.stats = stats;
        this.challenges = challenges;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Convidado não tem nome: a tela mostra "Meu espaço", como no protótipo.
        var name = session.IsSignedIn ? session.DisplayName : "";
        ProfileName.Text = name.Length > 0 ? name : "Meu espaço";
        Initial.Text = name.Length > 0 ? name[..1].ToUpper() : "C";

        try
        {
            var profile = await stats.GetAsync();
            var achievements = await challenges.GetAchievementsAsync();

            StreakNumber.Text = profile.StreakDays.ToString();
            ChallengesNumber.Text = profile.CompletedChallenges.ToString();
            SemanticProperties.SetDescription(StreakCard, $"{profile.StreakDays} dias de cuidado, sequência atual");
            SemanticProperties.SetDescription(ChallengesCard, $"{profile.CompletedChallenges} desafios experimentados");

            AchievementGrid.Children.Clear();
            AchievementGrid.RowDefinitions.Clear();
            for (int row = 0; row < (achievements.Count + Columns - 1) / Columns; row++)
                AchievementGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            for (int i = 0; i < achievements.Count; i++)
            {
                var badge = BuildBadge(achievements[i]);
                Grid.SetRow(badge, i / Columns);
                Grid.SetColumn(badge, i % Columns);
                AchievementGrid.Children.Add(badge);
            }
            ErrorMessage.Text = "";
        }
        catch (Exception error) when (error is IOException or SQLite.SQLiteException or UnauthorizedAccessException)
        {
            StreakNumber.Text = ChallengesNumber.Text = "";
            AchievementGrid.Children.Clear();
            ErrorMessage.Text = "Não foi possível ler seu progresso. Seus dados foram preservados.";
            SemanticScreenReader.Default.Announce(ErrorMessage.Text);
        }
    }

    // Uma conquista: medalha com ícone, nome e "Conquistada" ou "Em breve".
    private static Border BuildBadge(AchievementInfo achievement)
    {
        var resources = Application.Current!.Resources;
        var status = achievement.Obtained ? "Conquistada" : "Em breve";

        var medal = new Border
        {
            WidthRequest = 52,
            HeightRequest = 52,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(26) },
            HorizontalOptions = LayoutOptions.Center,
            Content = new Image
            {
                Source = Icons.GetValueOrDefault(achievement.Key, "leaf.png"),
                WidthRequest = 28,
                HeightRequest = 28,
                Opacity = achievement.Obtained ? 1 : 0.35
            }
        };
        var (light, dark) = achievement.Obtained ? ("CalmeLilac", "CalmeLilacDark") : ("CalmeSubtle", "CalmeSubtleDark");
        medal.SetAppThemeColor(BackgroundColorProperty, (Color)resources[light], (Color)resources[dark]);

        var name = new Label
        {
            Text = achievement.Name,
            Style = (Style)resources["FlowSmall"],
            FontFamily = "NunitoBold",
            HorizontalTextAlignment = TextAlignment.Center
        };
        name.SetAppThemeColor(Label.TextColorProperty, (Color)resources["CalmeText"], (Color)resources["CalmeTextDark"]);

        var caption = new Label { Text = status, Style = (Style)resources["FlowSmall"], HorizontalTextAlignment = TextAlignment.Center };
        if (achievement.Obtained)
            caption.SetAppThemeColor(Label.TextColorProperty, (Color)resources["CalmePurple"], (Color)resources["CalmePurpleDark"]);

        var card = new Border
        {
            Style = (Style)resources["FlowCard"],
            Padding = new Thickness(8, 12),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(16) },
            AutomationId = $"profile-achievement-{achievement.Key}",
            Content = new VerticalStackLayout { Spacing = 8, Children = { medal, name, caption } }
        };
        SemanticProperties.SetDescription(card, $"Conquista {achievement.Name}, {status.ToLower()}");
        return card;
    }

    private async void OnHomeClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//home");
    private async void OnSupportClicked(object? sender, EventArgs e) => await App.OpenSupport();
}
