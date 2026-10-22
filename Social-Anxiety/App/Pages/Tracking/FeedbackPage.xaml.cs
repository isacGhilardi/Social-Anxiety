namespace Social_Anxiety;

public partial class FeedbackPage : ContentPage, IQueryAttributable
{
    public FeedbackPage() => InitializeComponent();
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        try
        {
            var record = query.TryGetValue("id", out var value) && Guid.TryParse(value?.ToString(), out var id)
                ? App.Checkins.Read().SingleOrDefault(r => r.Id == id) : null;
            SavedScore.Text = record?.Summary ?? "Registro não encontrado.";
        }
        catch (Exception error) when (error is IOException or InvalidDataException or UnauthorizedAccessException)
        { ErrorMessage.Text = "Não foi possível consultar o registro. Seus dados foram preservados."; }
    }
    private async void OnHistoryClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("history");
    private async void OnHomeClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//home");
    private async void OnSupportClicked(object? sender, EventArgs e) => await App.OpenSupport();
}
