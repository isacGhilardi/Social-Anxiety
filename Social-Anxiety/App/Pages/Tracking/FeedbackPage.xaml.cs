using Social_Anxiety.Core.CheckIns;

namespace Social_Anxiety;

public partial class FeedbackPage : ContentPage, IQueryAttributable
{
    private readonly CheckInService checkIns;

    public FeedbackPage(CheckInService checkIns)
    {
        this.checkIns = checkIns;
        InitializeComponent();
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        try
        {
            var record = query.TryGetValue("id", out var value) && Guid.TryParse(value?.ToString(), out var id)
                ? await checkIns.GetByIdAsync(id) : null;
            SavedScore.Text = record?.Summary ?? "Registro não encontrado.";
        }
        catch (Exception error) when (error is IOException or SQLite.SQLiteException or UnauthorizedAccessException)
        { ErrorMessage.Text = "Não foi possível consultar o registro. Seus dados foram preservados."; }
    }
    private async void OnHistoryClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("history");
    private async void OnHomeClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//home");
    private async void OnSupportClicked(object? sender, EventArgs e) => await App.OpenSupport();
}
