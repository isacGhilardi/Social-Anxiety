using Social_Anxiety.Core.CheckIns;

namespace Calme.Core.Tests;

public class ChartAggregatorTests
{
    // "Hoje" é 23/10/2026 no fuso UTC-3 (meio-dia local).
    private static readonly ChartAggregator Aggregator =
        new(new TestClock(new DateTimeOffset(2026, 10, 23, 15, 0, 0, TimeSpan.Zero), CheckInServiceTests.Brasilia));

    private static CheckIn On(int year, int month, int day, int score) =>
        new(Guid.NewGuid(), score, DateTime.UtcNow, new DateOnly(year, month, day));

    [Fact]
    public void Semana_sem_registros_tem_7_pontos_nulos_e_esta_vazia()
    {
        var week = Aggregator.Week([]);
        Assert.Equal(["17", "18", "19", "20", "21", "22", "23"], week.Points.Select(p => p.Label));
        Assert.All(week.Points, p => { Assert.Null(p.Value); Assert.Equal(0, p.Count); });
        Assert.True(week.IsEmpty);
    }

    [Fact]
    public void Semana_com_um_registro_preenche_so_aquele_dia()
    {
        var week = Aggregator.Week([On(2026, 10, 23, 4)]);
        Assert.False(week.IsEmpty);
        Assert.Equal(new ChartPoint("23", 4, 1), week.Points[^1]);
        Assert.All(week.Points.Take(6), p => Assert.Null(p.Value));
    }

    [Fact]
    public void Semana_com_buracos_mantem_nulo_e_nunca_zero()
    {
        var week = Aggregator.Week([On(2026, 10, 17, 2), On(2026, 10, 20, 5), On(2026, 10, 23, 1)]);
        Assert.Equal([2, null, null, 5, null, null, 1], week.Points.Select(p => p.Value));
        Assert.DoesNotContain(week.Points, p => p.Value == 0);
    }

    [Fact]
    public void Semana_ignora_registros_fora_do_periodo()
    {
        var week = Aggregator.Week([On(2026, 10, 16, 3), On(2026, 10, 24, 3), On(2025, 10, 23, 3)]);
        Assert.True(week.IsEmpty);
    }

    [Fact]
    public void Mes_divide_em_4_blocos_e_o_dia_31_entra_no_ultimo()
    {
        var month = Aggregator.Month([
            On(2026, 10, 1, 2), On(2026, 10, 7, 4),
            On(2026, 10, 8, 3),
            On(2026, 10, 22, 1), On(2026, 10, 31, 5),
        ], 2026, 10);
        Assert.Equal(["Sem 1", "Sem 2", "Sem 3", "Sem 4"], month.Points.Select(p => p.Label));
        Assert.Equal(new ChartPoint("Sem 1", 3, 2), month.Points[0]);
        Assert.Equal(new ChartPoint("Sem 2", 3, 1), month.Points[1]);
        Assert.Equal(new ChartPoint("Sem 3", null, 0), month.Points[2]);
        Assert.Equal(new ChartPoint("Sem 4", 3, 2), month.Points[3]);
    }

    [Fact]
    public void Mes_curto_fecha_o_ultimo_bloco_no_fim_do_mes()
    {
        var month = Aggregator.Month([On(2026, 2, 28, 4), On(2026, 3, 1, 1)], 2026, 2);
        Assert.Equal(new ChartPoint("Sem 4", 4, 1), month.Points[3]);
        Assert.Equal(1, month.Points.Sum(p => p.Count));
    }

    [Fact]
    public void Mes_ignora_outros_meses_e_anos()
    {
        var month = Aggregator.Month([On(2026, 9, 30, 2), On(2026, 11, 1, 2), On(2025, 10, 10, 2)], 2026, 10);
        Assert.True(month.IsEmpty);
        Assert.All(month.Points, p => Assert.Null(p.Value));
    }

    [Fact]
    public void Mes_atual_usa_o_relogio()
    {
        var month = Aggregator.CurrentMonth([On(2026, 10, 23, 2)]);
        Assert.Equal(2, month.Points[3].Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void Mes_invalido_e_recusado(int month) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Aggregator.Month([], 2026, month));
}
