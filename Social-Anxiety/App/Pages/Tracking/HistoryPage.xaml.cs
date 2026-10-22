namespace Social_Anxiety;

public partial class HistoryPage : ContentPage
{
    private int days = 7;
    public HistoryPage() => InitializeComponent();
    protected override void OnAppearing() { base.OnAppearing(); Refresh(); }
    private void Refresh()
    {
        try
        {
            var records = App.Checkins.ReadPeriod(days, DateOnly.FromDateTime(DateTime.Now));
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
        catch (Exception error) when (error is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            HistoryGraph.IsVisible = EmptyMessage.IsVisible = false;
            RecordCount.Text = "";
            BindableLayout.SetItemsSource(RecordList, null);
            ErrorMessage.Text = "Não foi possível ler o histórico. Seus dados foram preservados.";
        }
    }
    private void OnPeriodClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && int.TryParse(button.CommandParameter?.ToString(), out var value) && value is 7 or 30)
        { days = value; Refresh(); }
    }
    private async void OnRecordClicked(object? sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: Guid id }) await Shell.Current.GoToAsync($"record?id={id}");
    }
    private async void OnHomeClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//home");
    private async void OnSupportClicked(object? sender, EventArgs e) => await App.OpenSupport();
}
