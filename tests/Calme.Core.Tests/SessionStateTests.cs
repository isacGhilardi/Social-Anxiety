using Social_Anxiety.Core.Auth;

namespace Calme.Core.Tests;

public class SessionStateTests
{
    private sealed class MemoryStore : ISessionStore
    {
        public string? Value;
        public Task<string?> ReadAsync() => Task.FromResult(Value);
        public Task SaveAsync(string value) { Value = value; return Task.CompletedTask; }
        public Task ClearAsync() { Value = null; return Task.CompletedTask; }
    }

    private sealed class MemoryUsers(params User[] users) : IUserRepository
    {
        public Task<User?> GetByIdAsync(Guid id) => Task.FromResult(users.FirstOrDefault(u => u.Id == id));
    }

    private static readonly User Ana = new(Guid.NewGuid(), "Ana", "ana@exemplo.com", [1], [2], DateTime.UtcNow);

    [Fact]
    public async Task Sem_sessao_salva_nao_restaura_nada()
    {
        var session = new SessionState(new MemoryStore(), new MemoryUsers());
        Assert.False(await session.RestoreAsync());
        Assert.False(session.HasSession);
    }

    [Fact]
    public async Task Convidado_e_restaurado_ao_reabrir()
    {
        var store = new MemoryStore();
        await new SessionState(store, new MemoryUsers()).StartGuestAsync();

        var reopened = new SessionState(store, new MemoryUsers());
        Assert.True(await reopened.RestoreAsync());
        Assert.True(reopened.IsGuest);
        Assert.False(reopened.IsSignedIn);
    }

    [Fact]
    public async Task Usuario_e_restaurado_com_nome_e_so_o_id_fica_salvo()
    {
        var store = new MemoryStore();
        await new SessionState(store, new MemoryUsers(Ana)).SignInAsync(Ana);
        Assert.Equal(Ana.Id.ToString(), store.Value);

        var reopened = new SessionState(store, new MemoryUsers(Ana));
        Assert.True(await reopened.RestoreAsync());
        Assert.Equal(Ana.Id, reopened.UserId);
        Assert.Equal("Ana", reopened.DisplayName);
    }

    [Fact]
    public async Task Id_sem_usuario_correspondente_e_descartado()
    {
        var store = new MemoryStore { Value = Guid.NewGuid().ToString() };
        var session = new SessionState(store, new MemoryUsers());
        Assert.False(await session.RestoreAsync());
        Assert.Null(store.Value);
    }

    [Fact]
    public async Task Sair_apaga_a_sessao()
    {
        var store = new MemoryStore();
        var session = new SessionState(store, new MemoryUsers(Ana));
        await session.SignInAsync(Ana);
        await session.SignOutAsync();
        Assert.False(session.HasSession);
        Assert.Null(store.Value);
    }
}
