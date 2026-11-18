using Social_Anxiety.Core.Challenges;

namespace Social_Anxiety;

public partial class ChallengesPage : ContentPage
{
    private readonly ChallengeService service;
    private bool navigating;

    public ChallengesPage(ChallengeService service)
    {
        this.service = service;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            var levels = await service.GetTrailAsync();
            var achievements = await service.GetAchievementsAsync();
            LevelCards.Children.Clear();
            foreach (var item in levels)
                LevelCards.Children.Add(BuildCard(item));
            ProgressText.Text = $"{levels.Count(l => l.State == TrailState.Completed)} de {levels.Count} níveis concluídos";
            AchievementCards.Children.Clear();
            foreach (var achievement in achievements)
                AchievementCards.Children.Add(BuildAchievement(achievement));
            ErrorMessage.Text = "";
        }
        catch (Exception error) when (error is IOException or SQLite.SQLiteException or UnauthorizedAccessException)
        {
            LevelCards.Children.Clear();
            AchievementCards.Children.Clear();
            ProgressText.Text = "";
            ErrorMessage.Text = "Não foi possível ler sua trilha. Seus dados foram preservados.";
            SemanticScreenReader.Default.Announce(ErrorMessage.Text);
        }
    }

    // Um cartão por nível: a cor e o texto mudam conforme o estado (concluído, disponível ou bloqueado).
    private Border BuildCard(TrailItem item)
    {
        var level = item.Level;
        var (status, light, dark) = item.State switch
        {
            TrailState.Completed => ("Concluído", "CalmeMint", "CalmeMintDark"),
            TrailState.Available => ("Disponível", "CalmeBlue", "CalmeBlueDark"),
            _ => ("Bloqueado", "CalmeSubtle", "CalmeSubtleDark")
        };
        var resources = Application.Current!.Resources;

        var texts = new VerticalStackLayout { Spacing = 6 };
        texts.Children.Add(new Label { Text = $"{level.Number}. {level.Name}", Style = (Style)resources["FlowCardTitle"] });
        texts.Children.Add(new Label { Text = level.Summary, Style = (Style)resources["FlowCopy"] });
        texts.Children.Add(new Label { Text = status, Style = (Style)resources["FlowSmall"] });
        if (item.State == TrailState.Locked)
            texts.Children.Add(new Label { Text = "Conclua o nível anterior para liberar.", Style = (Style)resources["FlowSmall"] });

        var card = new Border
        {
            Style = (Style)resources["FlowCard"],
            AutomationId = $"challenge-level-{level.Number}",
            Content = texts
        };
        card.SetAppThemeColor(BackgroundColorProperty, (Color)resources[light], (Color)resources[dark]);
        var summary = $"Nível {level.Number}, {level.Name}, {level.Summary}, {status.ToLower()}";

        // Nível 1 se conclui com o check-in; os demais abrem o desafio ou explicam o bloqueio. Concluído não abre nada.
        var route = item.State switch
        {
            TrailState.Locked => $"challengeLocked?level={level.Number}",
            TrailState.Available when level.Number == 1 => "checkin",
            TrailState.Available => $"challengeActive?level={level.Number}",
            _ => null
        };
        // Botão, e não toque no cartão, para funcionar com teclado e leitor de tela.
        if (route is not null)
        {
            var open = new Button
            {
                Text = item.State == TrailState.Locked ? "Ver o que falta" : level.Number == 1 ? "Fazer check-in" : "Ver desafio",
                Style = (Style)resources["CalmeTextButton"],
                AutomationId = $"challenge-open-{level.Number}",
                HorizontalOptions = LayoutOptions.Start
            };
            open.Clicked += async (_, _) => await OpenAsync(route);
            SemanticProperties.SetDescription(open, $"{open.Text}, nível {level.Number}");
            texts.Children.Add(open);
        }
        else
            SemanticProperties.SetDescription(card, summary); // sem botão: o cartão inteiro é lido de uma vez
        return card;
    }

    private static Border BuildAchievement(AchievementInfo achievement)
    {
        var resources = Application.Current!.Resources;
        var status = achievement.ComingSoon ? "Em breve" : achievement.Obtained ? "Conquistada" : "Ainda não";
        var texts = new VerticalStackLayout { Spacing = 4 };
        texts.Children.Add(new Label { Text = achievement.Name, Style = (Style)resources["FlowCardTitle"] });
        texts.Children.Add(new Label { Text = status, Style = (Style)resources["FlowSmall"] });

        var card = new Border
        {
            Style = (Style)resources["FlowCard"],
            AutomationId = $"achievement-{achievement.Key}",
            Content = texts
        };
        var (light, dark) = achievement.Obtained ? ("CalmeLilac", "CalmeLilacDark") : ("CalmeSubtle", "CalmeSubtleDark");
        card.SetAppThemeColor(BackgroundColorProperty, (Color)resources[light], (Color)resources[dark]);
        SemanticProperties.SetDescription(card, $"Conquista {achievement.Name}, {status.ToLower()}");
        return card;
    }

    private async Task OpenAsync(string route)
    {
        if (navigating) return; // evita abrir duas telas com toques seguidos
        navigating = true;
        try { await Shell.Current.GoToAsync(route); }
        finally { navigating = false; }
    }

    private async void OnHomeClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//home");
    private async void OnSupportClicked(object? sender, EventArgs e) => await App.OpenSupport();
}
