namespace Social_Anxiety;
public record CheckinRecord(string Day, int Score)
{
    static readonly string[] Labels = { "Pouca", "Leve", "Moderada", "Intensa", "Muito intensa" };

    public string ScoreLabel => Score is >= 1 and <= 5 ? Labels[Score - 1] : "";

    public static readonly IReadOnlyList<CheckinRecord> Sample = new List<CheckinRecord>
    {
        new("29 de setembro", 2),
        new("28 de setembro", 3),
        new("27 de setembro", 2),
        new("26 de setembro", 3),
        new("25 de setembro", 4),
        new("24 de setembro", 2),
        new("23 de setembro", 3),
    };
}