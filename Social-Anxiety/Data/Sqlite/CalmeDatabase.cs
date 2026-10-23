using SQLite;

namespace Social_Anxiety.Data;

/// <summary>
/// Conexão única com o banco local. As tabelas são criadas no primeiro acesso;
/// CreateTablesAsync só cria o que falta, então chamar de novo não altera nada.
/// </summary>
public sealed class CalmeDatabase(string path)
{
    internal static readonly Type[] Tables = [];

    private readonly SQLiteAsyncConnection connection = new(path);
    private readonly SemaphoreSlim gate = new(1, 1);
    private bool ready;

    public async Task<SQLiteAsyncConnection> GetAsync()
    {
        if (ready) return connection;
        await gate.WaitAsync();
        try
        {
            if (!ready)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
                await connection.EnableWriteAheadLoggingAsync();
                if (Tables.Length > 0) await connection.CreateTablesAsync(CreateFlags.None, Tables);
                ready = true;
            }
        }
        finally { gate.Release(); }
        return connection;
    }

    public Task CloseAsync() => connection.CloseAsync();
}
