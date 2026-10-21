namespace Social_Anxiety;

public partial class MainPage : ContentPage
{
    private readonly int[] registrosExemplo = { 2, 3, 1, 4, 2, 3, 2 };

    public MainPage()
    {
        InitializeComponent();

        AtualizarResumo(registrosExemplo, dadosDeExemplo: true);
    }

    private void AtualizarResumo(int[] registros, bool dadosDeExemplo)
    {
        bool temRegistros = registros.Length > 0;

        MensagemSemRegistros.IsVisible = !temRegistros;
        GraficoResumo.IsVisible = temRegistros;
        LegendaGrafico.IsVisible = temRegistros;
        AvisoExemplo.IsVisible = temRegistros && dadosDeExemplo;

        if (temRegistros)
        {
            GraficoResumo.Drawable = new ResumoGrafico(registros);
            GraficoResumo.Invalidate();
        }
    }
}