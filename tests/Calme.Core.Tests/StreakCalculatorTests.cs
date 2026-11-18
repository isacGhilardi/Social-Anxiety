using Social_Anxiety.Core.Profile;

namespace Calme.Core.Tests;

public class StreakCalculatorTests
{
    private static readonly DateOnly Today = new(2026, 11, 12);

    private static DateOnly DaysAgo(int n) => Today.AddDays(-n);

    [Fact]
    public void Sem_check_ins_a_sequencia_e_zero()
    {
        Assert.Equal(0, StreakCalculator.Calculate([], Today));
    }

    [Fact]
    public void Check_in_so_de_hoje_conta_um_dia()
    {
        Assert.Equal(1, StreakCalculator.Calculate([Today], Today));
    }

    [Fact]
    public void Dias_seguidos_ate_hoje_somam_a_sequencia()
    {
        Assert.Equal(3, StreakCalculator.Calculate([Today, DaysAgo(1), DaysAgo(2)], Today));
    }

    [Fact]
    public void Hoje_sem_check_in_mantem_a_sequencia_de_ontem()
    {
        Assert.Equal(2, StreakCalculator.Calculate([DaysAgo(1), DaysAgo(2)], Today));
    }

    [Fact]
    public void Um_dia_inteiro_sem_check_in_zera_a_sequencia()
    {
        Assert.Equal(0, StreakCalculator.Calculate([DaysAgo(2), DaysAgo(3)], Today));
    }

    [Fact]
    public void Buraco_no_meio_corta_a_contagem()
    {
        Assert.Equal(2, StreakCalculator.Calculate([Today, DaysAgo(1), DaysAgo(3)], Today));
    }

    [Fact]
    public void Dias_repetidos_e_fora_de_ordem_nao_atrapalham()
    {
        Assert.Equal(2, StreakCalculator.Calculate([DaysAgo(1), Today, Today], Today));
    }
}
