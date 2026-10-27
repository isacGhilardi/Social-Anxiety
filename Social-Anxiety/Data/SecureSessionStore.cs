using Social_Anxiety.Core.Auth;

namespace Social_Anxiety.Data;

public sealed class SecureSessionStore : ISessionStore
{
    private const string Key = "calme.session";

    public async Task<string?> ReadAsync()
    {
        try { return await SecureStorage.Default.GetAsync(Key); }
        catch (Exception)
        {
            // Valor ilegível (chave de criptografia trocada ou arquivo corrompido): trata como sem sessão.
            SecureStorage.Default.Remove(Key);
            return null;
        }
    }

    public Task SaveAsync(string value) => SecureStorage.Default.SetAsync(Key, value);

    public Task ClearAsync()
    {
        SecureStorage.Default.Remove(Key);
        return Task.CompletedTask;
    }
}
