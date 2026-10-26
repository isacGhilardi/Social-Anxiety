
namespace Social_Anxiety.Views;

public partial class MeditacaoGuiadaPage : ContentPage
{
    private const int DuracaoSegundos = 300;
    private int _tempoDecorrido;
    private IDispatcherTimer? _timer;
    private bool _reproduzindo;

    public MeditacaoGuiadaPage()
    {
        InitializeComponent();

        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.IsRepeating = true;
        _timer.Tick += OnTimerTick;

        AtualizarInterface();
    }

    private void OnReproduzirClicked(object sender, EventArgs e)
    {
        if (_tempoDecorrido >= DuracaoSegundos)
        {
            _tempoDecorrido = 0;
        }

        _reproduzindo = !_reproduzindo;

        if (_reproduzindo)
        {
            _timer?.Start();
        }
        else
        {
            _timer?.Stop();
        }

        AtualizarInterface();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (_tempoDecorrido < DuracaoSegundos)
        {
            _tempoDecorrido++;
        }

        if (_tempoDecorrido >= DuracaoSegundos)
        {
            _tempoDecorrido = DuracaoSegundos;
            _reproduzindo = false;
            _timer?.Stop();

            AtualizarInterface();

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert(
                    "Sessão concluída",
                    "Você concluiu os cinco minutos de meditação. Reserve um momento para perceber como está se sentindo.",
                    "Concluir");
            });

            return;
        }

        AtualizarInterface();
    }

    private void OnRecomecarClicked(object sender, EventArgs e)
    {
        _timer?.Stop();
        _reproduzindo = false;
        _tempoDecorrido = 0;

        AtualizarInterface();
    }

    private void AtualizarInterface()
    {
        TempoAtual.Text = TimeSpan
            .FromSeconds(_tempoDecorrido)
            .ToString(@"mm\:ss");

        Progresso.Progress =
            (double)_tempoDecorrido / DuracaoSegundos;

        BotaoReproduzir.Text = _reproduzindo
            ? "Ⅱ  Pausar meditação"
            : _tempoDecorrido >= DuracaoSegundos
                ? "▶  Repetir meditação"
                : "▶  Iniciar meditação";
    }

    private async void OnVoltarClicked(object sender, EventArgs e)
    {
        if (_reproduzindo)
        {
            _timer?.Stop();
            _reproduzindo = false;
        }

        await Shell.Current.GoToAsync("..");
    }

    protected override void OnDisappearing()
    {
        _timer?.Stop();
        _reproduzindo = false;

        base.OnDisappearing();
    }
}
