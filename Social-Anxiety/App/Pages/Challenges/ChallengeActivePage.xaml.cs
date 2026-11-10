using Social_Anxiety.Core.Challenges;

namespace Social_Anxiety;

// Desafio aberto a partir da trilha. Rota: challengeActive?level=2. Aceitar o desafio não grava nada.
public partial class ChallengeActivePage : ContentPage, IQueryAttributable
{
    private readonly ChallengeService service;
    private readonly CompleteChallengeAction action;
    private int? levelNumber;

    public ChallengeActivePage(ChallengeService service, CompleteChallengeAction action)
    {
        this.service = service;
        this.action = action;
        InitializeComponent();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
        => levelNumber = query.TryGetValue("level", out var value) && int.TryParse(value?.ToString(), out var number) ? number : null;

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        TrailItem? item;
        try { item = (await service.GetTrailAsync()).FirstOrDefault(l => l.Level.Number == levelNumber); }
        catch (Exception error) when (error is IOException or SQLite.SQLiteException or UnauthorizedAccessException)
        {
            ShowMessage("Não foi possível ler sua trilha. Seus dados foram preservados.");
            return;
        }

        // O nível 1 se conclui com o check-in; nível inexistente, bloqueado ou concluído só mostra um aviso.
        if (item is not { State: TrailState.Available } || item.Level.Number == 1)
        {
            ShowMessage(item switch
            {
                null => "Não encontramos esse desafio.",
                { State: TrailState.Locked } => "Este nível ainda está bloqueado. Conclua o nível anterior primeiro.",
                { State: TrailState.Completed } => "Você já concluiu este desafio.",
                _ => "Este nível se conclui com o seu primeiro check-in."
            });
            return;
        }

        var level = item.Level;
        ChallengeContent.IsVisible = true;
        StatusMessage.IsVisible = false;
        LevelTag.Text = $"NÍVEL {level.Number} · {level.Summary.ToUpper()}";
        LevelTitle.Text = level.Title;
        LevelDescription.Text = level.Description;
        BindableLayout.SetItemsSource(StepsList, level.Steps);
        SemanticProperties.SetDescription(StepsList, $"{level.Title} {level.Steps.Count} passos");
    }

    private void ShowMessage(string text)
    {
        ChallengeContent.IsVisible = false;
        StatusMessage.IsVisible = true;
        StatusMessage.Text = text;
    }

    private async void OnCompleteClicked(object? sender, EventArgs e)
    {
        if (levelNumber is not { } level) return;
        CompleteButton.IsEnabled = false;
        try { await action.RunAsync(level); }
        catch (Exception error) when (error is IOException or SQLite.SQLiteException or UnauthorizedAccessException)
        {
            await DisplayAlertAsync("Algo deu errado", "Não foi possível salvar agora. Seus dados foram preservados.", "OK");
        }
        finally { CompleteButton.IsEnabled = true; }
    }

    private async void OnBackClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("..");
    private async void OnSupportClicked(object? sender, EventArgs e) => await App.OpenSupport();
}
