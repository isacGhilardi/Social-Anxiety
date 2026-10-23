using Social_Anxiety.Core.CheckIns;

namespace Social_Anxiety;

public partial class RecordPage : ContentPage, IQueryAttributable
{
    private readonly CheckInService checkIns;

    public RecordPage(CheckInService checkIns)
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
            RecordDay.Text = record?.Day ?? "Registro não encontrado";
            SavedScore.Text = record?.Summary ?? "";
        }
        catch (Exception error) when (error is IOException or SQLite.SQLiteException or UnauthorizedAccessException)
        { ErrorMessage.Text = "Não foi possível ler o registro. Seus dados foram preservados."; }
    }
    private async void OnHistoryClicked(object? sender, EventArgs e)
    {
        if (Navigation.NavigationStack.Count > 1 && Navigation.NavigationStack[^2] is HistoryPage)
            await Navigation.PopAsync(false);
        else await Shell.Current.GoToAsync("history");
    }
    private async void OnHelpClicked(object? sender, EventArgs e) => await App.OpenHelp();
    private async void OnSupportClicked(object? sender, EventArgs e) => await App.OpenSupport();
}
