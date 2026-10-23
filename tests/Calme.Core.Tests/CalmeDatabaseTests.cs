using Social_Anxiety.Data;

namespace Calme.Core.Tests;

public class CalmeDatabaseTests
{
    [Fact]
    public async Task Inicializar_varias_vezes_cria_o_banco_uma_vez_sem_erro()
    {
        await using var temp = new TempDatabase();
        var first = await temp.Database.GetAsync();
        var second = await temp.Database.GetAsync();
        Assert.Same(first, second);
        Assert.True(File.Exists(temp.FilePath));

        var reopened = new CalmeDatabase(temp.FilePath);
        await reopened.GetAsync();
        await reopened.CloseAsync();
    }
}
