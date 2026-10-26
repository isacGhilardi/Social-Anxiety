using System.Globalization;
using Social_Anxiety.Core.CheckIns;

namespace Social_Anxiety;

public partial class HistoryPage : ContentPage
{
    private readonly CheckInService checkIns;
    private readonly ChartAggregator aggregator;
    private bool mensal;
    private bool carregando;

    public HistoryPage(CheckInService checkIns, ChartAggregator aggregator)
    {
        this.checkIns = checkIns;
        this.aggregator = aggregator;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Application.Current!.RequestedThemeChanged += OnThemeChanged;
        await RefreshAsync();
    }

    protected override void OnDisappearing()
    {
        Application.Current!.RequestedThemeChanged -= OnThemeChanged;
        base.OnDisappearing();
    }

    private void OnThemeChanged(object? sender, AppThemeChangedEventArgs e) => HistoryGraph.Invalidate();

    private async Task RefreshAsync()
    {
        if (carregando) return;
        carregando = true;
        WeekButton.IsEnabled = MonthButton.IsEnabled = false;
        LoadingIndicator.IsVisible = LoadingIndicator.IsRunning = true;
        ErrorMessage.Text = "";

        try
        {
            var hoje = aggregator.Today();
            var inicio = mensal ? new DateOnly(hoje.Year, hoje.Month, 1) : hoje.AddDays(-6);
            var fim = mensal ? inicio.AddMonths(1).AddDays(-1) : hoje;
            var encontrados = await checkIns.GetAllAsync();
            var registros = encontrados
                .Where(r => r.LocalDay >= inicio && r.LocalDay <= fim)
                .OrderByDescending(r => r.LocalDay)
                .ToArray();
            var serie = mensal
                ? aggregator.Month(registros, hoje.Year, hoje.Month)
                : aggregator.Week(registros);

            PeriodLabel.Text = mensal
                ? inicio.ToDateTime(TimeOnly.MinValue).ToString("MMMM 'de' yyyy", CultureInfo.GetCultureInfo("pt-BR"))
                : $"Últimos 7 dias · {inicio:dd/MM}–{fim:dd/MM}";
            ChartLegend.Text = mensal
                ? "Média de ansiedade por bloco: dias 1–7, 8–14, 15–21 e 22 até o fim do mês. Escala de 1 a 5."
                : "Ansiedade percebida: 1 = pouca; 5 = muita. Dias sem registro ficam sem ponto.";
            RecordCount.Text = registros.Length == 1 ? "1 registro no período" : $"{registros.Length} registros no período";
            EmptyMessage.IsVisible = serie.IsEmpty;
            HistoryGraph.IsVisible = ChartLegend.IsVisible = !serie.IsEmpty;
            HistoryGraph.Drawable = new GraficoHistorico(serie);
            var descricao = string.Join("; ", serie.Points.Select(p =>
                $"{p.Label}: {(p.Value is double valor ? valor.ToString("0.##", CultureInfo.GetCultureInfo("pt-BR")) : "sem registro")}"));
            SemanticProperties.SetDescription(HistoryGraph, $"{PeriodLabel.Text}. {ChartLegend.Text} {descricao}");
            HistoryGraph.Invalidate();
            BindableLayout.SetItemsSource(RecordList, registros);
        }
        catch (Exception error) when (error is IOException or SQLite.SQLiteException or UnauthorizedAccessException)
        {
            HistoryGraph.IsVisible = ChartLegend.IsVisible = EmptyMessage.IsVisible = false;
            RecordCount.Text = "";
            BindableLayout.SetItemsSource(RecordList, null);
            ErrorMessage.Text = "Não foi possível ler o histórico. Seus dados foram preservados.";
        }
        finally
        {
            LoadingIndicator.IsVisible = LoadingIndicator.IsRunning = false;
            WeekButton.IsEnabled = MonthButton.IsEnabled = true;
            WeekButton.Style = (Style)Application.Current!.Resources[mensal ? "CalmeTextButton" : "CalmeButton"];
            MonthButton.Style = (Style)Application.Current.Resources[mensal ? "CalmeButton" : "CalmeTextButton"];
            SemanticProperties.SetDescription(WeekButton, mensal ? "Semana" : "Semana, selecionada");
            SemanticProperties.SetDescription(MonthButton, mensal ? "Mês, selecionado" : "Mês");
            carregando = false;
        }
    }

    private async void OnPeriodClicked(object? sender, EventArgs e)
    {
        if (carregando || sender is not Button button) return;
        if (button.CommandParameter?.ToString() is not ("7" or "30")) return;
        mensal = button.CommandParameter.ToString() == "30";
        await RefreshAsync();
    }

    private async void OnRecordClicked(object? sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: Guid id })
            await Shell.Current.GoToAsync($"record?id={id}");
    }

    private async void OnHomeClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//home");
    private async void OnSupportClicked(object? sender, EventArgs e) => await App.OpenSupport();
}
