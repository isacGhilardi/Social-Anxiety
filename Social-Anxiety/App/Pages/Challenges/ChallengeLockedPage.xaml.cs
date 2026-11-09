using Social_Anxiety.Core.Challenges;

namespace Social_Anxiety;

// Tela exibida ao tocar em um nível bloqueado da trilha. Rota: challengeLocked?level=3
public partial class ChallengeLockedPage : ContentPage, IQueryAttributable
{
    // Dados falsos por enquanto; trocar pela fonte real quando o contrato de dados existir.
    private readonly ITrailSource source = new FakeTrailSource();
    private int? levelNumber;

    public ChallengeLockedPage() => InitializeComponent();

    public void ApplyQueryAttributes(IDictionary<string, object> query)
        => levelNumber = query.TryGetValue("level", out var value) && int.TryParse(value?.ToString(), out var number) ? number : null;

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var levels = await source.GetTrailAsync();
        var level = levels.FirstOrDefault(l => l.Level == levelNumber);
        var previous = levels.FirstOrDefault(l => l.Level == levelNumber - 1);

        // Nível inexistente ou já liberado: mostra só um aviso e o botão de voltar.
        if (level is not { State: TrailState.Locked })
        {
            LockedContent.IsVisible = false;
            StatusMessage.IsVisible = true;
            StatusMessage.Text = level is null
                ? "Não encontramos esse nível."
                : "Este nível já está liberado. Volte para a trilha para começar.";
            return;
        }

        LockedContent.IsVisible = true;
        StatusMessage.IsVisible = false;
        LevelTag.Text = $"NÍVEL {level.Level} · {level.Title.ToUpper()}";
        MissingTitle.Text = previous is null ? "Concluir o nível anterior" : $"Concluir o nível {previous.Level}: {previous.Title}";
        MissingDescription.Text = previous?.Description ?? "";
    }

    private async void OnBackClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("..");
    private async void OnSupportClicked(object? sender, EventArgs e) => await App.OpenSupport();
}
