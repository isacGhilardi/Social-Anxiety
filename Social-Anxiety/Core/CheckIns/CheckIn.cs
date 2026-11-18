using System.Globalization;

namespace Social_Anxiety.Core.CheckIns;

/// <summary>
/// Um check-in por dia local. O momento fica em UTC; o dia local é o do fuso do aparelho no momento do salvamento.
/// O rótulo é calculado a partir da nota, nunca guardado.
/// </summary>
public sealed record CheckIn(Guid Id, int Score, DateTime RecordedAtUtc, DateOnly LocalDay)
{
    public const int MinScore = 1;
    public const int MaxScore = 5;
    private static readonly string[] Labels = ["Pouca", "Leve", "Moderada", "Intensa", "Muito intensa"];

    public static bool IsValidScore(int score) => score is >= MinScore and <= MaxScore;
    public static string LabelFor(int score) => IsValidScore(score) ? Labels[score - 1] : "";

    public string ScoreLabel => LabelFor(Score);
    public string Summary => $"{Score} de 5 · {ScoreLabel}";
    public string Day => LocalDay.ToString("dd 'de' MMMM 'de' yyyy", CultureInfo.GetCultureInfo("pt-BR"));
}
