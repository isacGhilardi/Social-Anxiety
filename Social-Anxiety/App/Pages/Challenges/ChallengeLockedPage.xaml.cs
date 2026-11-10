using Social_Anxiety.Core.Challenges;

namespace Social_Anxiety;

// Tela exibida ao tocar em um nível bloqueado da trilha. Rota: challengeLocked?level=3
public partial class ChallengeLockedPage : ContentPage, IQueryAttributable
{
    private readonly ChallengeService service;
    private int? levelNumber;

    public ChallengeLockedPage(ChallengeService service)
    {
        this.service = service;
        InitializeComponent();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
        => levelNumber = query.TryGetValue("level", out var value) && int.TryParse(value?.ToString(), out var number) ? number : null;

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        IReadOnlyList<TrailItem> levels;
        try { levels = await service.GetTrailAsync(); }
        catch (Exception error) when (error is IOException or SQLite.SQLiteException or UnauthorizedAccessException)
        {
            LockedContent.IsVisible = false;
            StatusMessage.IsVisible = true;
            StatusMessage.Text = "Não foi possível ler sua trilha. Seus dados foram preservados.";
            return;
        }
        var level = levels.FirstOrDefault(l => l.Level.Number == levelNumber);
        var previous = levels.FirstOrDefault(l => l.Level.Number == levelNumber - 1)?.Level;

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
        LevelTag.Text = $"NÍVEL {level.Level.Number} · {level.Level.Title.ToUpper()}";
        MissingTitle.Text = previous is null ? "Concluir o nível anterior" : $"Concluir o nível {previous.Number}: {previous.Title}";
        MissingDescription.Text = previous?.Description ?? "";
    }

    private async void OnBackClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("..");
    private async void OnSupportClicked(object? sender, EventArgs e) => await App.OpenSupport();
}
