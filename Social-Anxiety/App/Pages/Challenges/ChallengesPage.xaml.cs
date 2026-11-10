using Social_Anxiety.Core.Challenges;

namespace Social_Anxiety;

public partial class ChallengesPage : ContentPage
{
    // Dados falsos por enquanto; trocar pela fonte real quando o contrato de dados existir.
    private readonly ITrailSource source = new FakeTrailSource();

    public ChallengesPage() => InitializeComponent();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var levels = await source.GetTrailAsync();
        LevelCards.Children.Clear();
        foreach (var level in levels.OrderBy(l => l.Level))
            LevelCards.Children.Add(BuildCard(level));
        ProgressText.Text = $"{levels.Count(l => l.State == TrailState.Completed)} de {levels.Count} níveis concluídos";
    }

    // Um cartão por nível: a cor e o texto mudam conforme o estado (concluído, disponível ou bloqueado).
    private static Border BuildCard(TrailLevel level)
    {
        var (status, light, dark) = level.State switch
        {
            TrailState.Completed => ("Concluído", "CalmeMint", "CalmeMintDark"),
            TrailState.Available => ("Disponível", "CalmeBlue", "CalmeBlueDark"),
            _ => ("Bloqueado", "CalmeSubtle", "CalmeSubtleDark")
        };
        var resources = Application.Current!.Resources;

        var texts = new VerticalStackLayout { Spacing = 6 };
        texts.Children.Add(new Label { Text = $"NÍVEL {level.Level} · {status.ToUpper()}", Style = (Style)resources["FlowSmall"] });
        texts.Children.Add(new Label { Text = level.Title, Style = (Style)resources["FlowCardTitle"] });
        texts.Children.Add(new Label { Text = level.Description, Style = (Style)resources["FlowCopy"] });
        if (level.State == TrailState.Locked)
            texts.Children.Add(new Label { Text = "Conclua o nível anterior para liberar.", Style = (Style)resources["FlowSmall"] });

        var card = new Border
        {
            Style = (Style)resources["FlowCard"],
            AutomationId = $"challenge-level-{level.Level}",
            Content = texts
        };
        card.SetAppThemeColor(BackgroundColorProperty, (Color)resources[light], (Color)resources[dark]);
        SemanticProperties.SetDescription(card, $"Nível {level.Level}, {level.Title}, {status.ToLower()}. {level.Description}");
        return card;
    }

    private async void OnHomeClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//home");
    private async void OnSupportClicked(object? sender, EventArgs e) => await App.OpenSupport();
}
