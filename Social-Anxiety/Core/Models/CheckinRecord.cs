namespace Social_Anxiety;
public sealed record CheckinRecord(Guid Id, DateOnly Date, int Score)
{
    static readonly string[] Labels = { "Pouca", "Leve", "Moderada", "Intensa", "Muito intensa" };

    [System.Text.Json.Serialization.JsonIgnore]
    public string ScoreLabel => Score is >= 1 and <= 5 ? Labels[Score - 1] : "";

    [System.Text.Json.Serialization.JsonIgnore]
    public string Day => Date.ToString("dd 'de' MMMM 'de' yyyy", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
    [System.Text.Json.Serialization.JsonIgnore]
    public string Summary => $"{Score} de 5 · {ScoreLabel}";
}
