using Social_Anxiety.Core.CheckIns;

namespace Social_Anxiety;

public partial class HistoryPage : ContentPage
{
    private readonly CheckInService checkIns;
    private int days = 7;

    public HistoryPage(CheckInService checkIns)
    {
        this.checkIns = checkIns;
        InitializeComponent();
    }

    protected override async void OnAppearing() { base.OnAppearing(); await RefreshAsync(); }
    private async Task RefreshAsync()
    {
        try
        {
            var records = await checkIns.GetLastDaysAsync(days);
            PeriodLabel.Text = $"Últimos {days} dias";
            RecordCount.Text = records.Count == 1 ? "1 registro no período" : $"{records.Count} registros no período";
            EmptyMessage.IsVisible = records.Count == 0;
            HistoryGraph.IsVisible = records.Count > 0;
            HistoryGraph.Drawable = new ResumoGrafico(records.Reverse().ToArray());
            SemanticProperties.SetDescription(HistoryGraph, $"Últimos {days} dias. Ansiedade percebida: " + string.Join(", ", records.Reverse().Select(r => r.Score)));
            HistoryGraph.Invalidate();
            BindableLayout.SetItemsSource(RecordList, records);
            ErrorMessage.Text = "";
        }
        catch (Exception error) when (error is IOException or SQLite.SQLiteException or UnauthorizedAccessException)
        {
            HistoryGraph.IsVisible = EmptyMessage.IsVisible = false;
            RecordCount.Text = "";
            BindableLayout.SetItemsSource(RecordList, null);
            ErrorMessage.Text = "Não foi possível ler o histórico. Seus dados foram preservados.";
        }
    }
    private async void OnPeriodClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && int.TryParse(button.CommandParameter?.ToString(), out var value) && value is 7 or 30)
        { days = value; await RefreshAsync(); }
    }
    private async void OnRecordClicked(object? sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: Guid id }) await Shell.Current.GoToAsync($"record?id={id}");
    }
    private async void OnHomeClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//home");
    private async void OnSupportClicked(object? sender, EventArgs e) => await App.OpenSupport();
}
