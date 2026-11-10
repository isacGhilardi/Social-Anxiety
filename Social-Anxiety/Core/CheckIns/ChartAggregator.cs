namespace Social_Anxiety.Core.CheckIns;

/// <summary>Um ponto do gráfico. <see cref="Value"/> é nulo quando não há registro: ausência de dado nunca vira zero.</summary>
public sealed record ChartPoint(string Label, double? Value, int Count);

/// <summary>Pontos prontos para desenhar, na ordem do eixo X (do mais antigo ao mais recente).</summary>
public sealed record ChartSeries(IReadOnlyList<ChartPoint> Points)
{
    /// <summary>Nenhum registro no período inteiro: a tela deve mostrar o estado vazio.</summary>
    public bool IsEmpty => Points.All(p => p.Count == 0);
}

/// <summary>Agrega check-ins para o gráfico. Código puro: sem MAUI e sem banco; "hoje" vem do relógio injetado.</summary>
public sealed class ChartAggregator(TimeProvider clock)
{
    public DateOnly Today() => DateOnly.FromDateTime(clock.GetLocalNow().DateTime);

    /// <summary>Últimos 7 dias locais terminando hoje, um ponto por dia, rotulado com o dia do mês ("23").</summary>
    public ChartSeries Week(IEnumerable<CheckIn> checkIns)
    {
        var today = Today();
        var byDay = checkIns.ToLookup(c => c.LocalDay);
        var points = Enumerable.Range(0, 7)
            .Select(offset => today.AddDays(offset - 6))
            .Select(day => Point(day.ToString("dd"), byDay[day]))
            .ToArray();
        return new ChartSeries(points);
    }

    /// <summary>Mês de calendário em 4 blocos: dias 1–7, 8–14, 15–21 e 22 até o fim do mês ("Sem 1" a "Sem 4").</summary>
    public ChartSeries Month(IEnumerable<CheckIn> checkIns, int year, int month)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(month, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(month, 12);
        var inMonth = checkIns.Where(c => c.LocalDay.Year == year && c.LocalDay.Month == month).ToArray();
        var points = Enumerable.Range(0, 4)
            .Select(block => Point($"Sem {block + 1}", inMonth.Where(c => Math.Min((c.LocalDay.Day - 1) / 7, 3) == block)))
            .ToArray();
        return new ChartSeries(points);
    }

    /// <summary>Mês de calendário de hoje.</summary>
    public ChartSeries CurrentMonth(IEnumerable<CheckIn> checkIns)
    {
        var today = Today();
        return Month(checkIns, today.Year, today.Month);
    }

    private static ChartPoint Point(string label, IEnumerable<CheckIn> checkIns)
    {
        var scores = checkIns.Select(c => c.Score).ToArray();
        return new ChartPoint(label, scores.Length == 0 ? null : scores.Average(), scores.Length);
    }
}
