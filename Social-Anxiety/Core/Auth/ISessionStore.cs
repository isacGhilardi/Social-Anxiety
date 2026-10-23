namespace Social_Anxiety.Core.Auth;

/// <summary>Guarda apenas o id do usuário ou a marca de convidado. Nunca a senha.</summary>
public interface ISessionStore
{
    Task<string?> ReadAsync();
    Task SaveAsync(string value);
    Task ClearAsync();
}
