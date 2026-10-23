using Social_Anxiety.Data;

namespace Calme.Core.Tests;

/// <summary>Banco SQLite em arquivo temporário, apagado ao final do teste.</summary>
sealed class TempDatabase : IAsyncDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "calme-tests-" + Guid.NewGuid());
    public string FilePath => Path.Combine(directory, "calme.db3");
    public CalmeDatabase Database { get; }

    public TempDatabase() => Database = new CalmeDatabase(FilePath);

    public async ValueTask DisposeAsync()
    {
        await Database.CloseAsync();
        if (Directory.Exists(directory)) Directory.Delete(directory, true);
    }
}
