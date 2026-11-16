using Social_Anxiety.Core.Auth;
using Social_Anxiety.Core.CheckIns;

namespace Social_Anxiety;

public partial class MainPage : ContentPage
{
    private readonly SessionState session;
    private readonly CheckInService checkIns;

    public MainPage(SessionState session, CheckInService checkIns)
    {
        this.session = session;
        this.checkIns = checkIns;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Greeting.Text = session.IsSignedIn ? $"Olá, {session.DisplayName}." : "Olá, que bom ter você.";
        try
        {
            var records = await checkIns.GetLastDaysAsync(7);
            EmptyMessage.IsVisible = records.Count == 0;
            SummaryGraph.IsVisible = records.Count > 0;
            LatestScore.Text = records.FirstOrDefault()?.Summary ?? "";
            SummaryGraph.Drawable = new ResumoGrafico(records.Reverse().ToArray());
            SemanticProperties.SetDescription(SummaryGraph, "Ansiedade percebida nos últimos sete dias: " + string.Join(", ", records.Reverse().Select(r => r.Score)));
            SummaryGraph.Invalidate();
            ErrorMessage.Text = "";
        }
        catch (Exception error) when (error is IOException or SQLite.SQLiteException or UnauthorizedAccessException)
        {
            SummaryGraph.IsVisible = EmptyMessage.IsVisible = false;
            LatestScore.Text = "";
            ErrorMessage.Text = "Não foi possível ler os registros. Seus dados foram preservados.";
        }
    }
    private async void OnCheckinClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("checkin");
    private async void OnHistoryClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("history");
    private async void OnBreatheClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("breathe");
    private async void OnTechniquesClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("library");
    private async void OnMeditacaoGuiadaClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("meditacao-guiada");
    private async void OnChallengesClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//challenges");
    private async void OnProfileClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//profile");
    private async void OnSupportClicked(object? sender, EventArgs e) => await App.OpenSupport();
}
