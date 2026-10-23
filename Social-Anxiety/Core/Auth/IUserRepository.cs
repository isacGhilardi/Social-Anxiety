namespace Social_Anxiety.Core.Auth;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);

    /// <summary>Busca pelo e-mail já normalizado.</summary>
    Task<User?> GetByEmailAsync(string normalizedEmail);

    /// <summary>Retorna false se o e-mail já estiver cadastrado.</summary>
    Task<bool> TryAddAsync(User user);
}
