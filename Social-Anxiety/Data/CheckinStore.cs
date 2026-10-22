using System.Text.Json;

namespace Social_Anxiety;

public sealed class CheckinStore(string path)
{
    private readonly object gate = new();

    public IReadOnlyList<CheckinRecord> Read()
    {
        lock (gate)
        {
            if (!File.Exists(path)) return [];
            List<CheckinRecord>? records;
            try { records = JsonSerializer.Deserialize<List<CheckinRecord>>(File.ReadAllText(path)); }
            catch (JsonException error) { throw new InvalidDataException("O arquivo de registros não pôde ser lido. Os dados foram preservados.", error); }
            if (records is null || records.Any(r => r is null || r.Id == Guid.Empty || r.Score is < 1 or > 5 || r.Date == default)
                || records.Select(r => r.Date).Distinct().Count() != records.Count
                || records.Select(r => r.Id).Distinct().Count() != records.Count)
                throw new InvalidDataException("O arquivo contém registros inválidos. Os dados foram preservados.");
            return records.OrderByDescending(r => r.Date).ToArray();
        }
    }

    public IReadOnlyList<CheckinRecord> ReadPeriod(int days, DateOnly today)
    {
        if (days is not (7 or 30)) throw new ArgumentOutOfRangeException(nameof(days));
        return Read().Where(r => r.Date >= today.AddDays(1 - days) && r.Date <= today).ToArray();
    }

    public CheckinRecord Save(int score, DateOnly date)
    {
        if (score is < 1 or > 5) throw new ArgumentOutOfRangeException(nameof(score));
        if (date == default) throw new ArgumentOutOfRangeException(nameof(date));
        lock (gate)
        {
            var records = Read().ToList();
            var previous = records.SingleOrDefault(r => r.Date == date);
            var record = new CheckinRecord(previous?.Id ?? Guid.NewGuid(), date, score);
            records.RemoveAll(r => r.Date == date);
            records.Add(record);
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
            var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                File.WriteAllText(temporary, JsonSerializer.Serialize(records));
                File.Move(temporary, path, true);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
            return record;
        }
    }
}
