namespace Social_Anxiety.Core.Auth;

/// <summary>Cadastro e login da conta local simulada. Em caso de sucesso, abre a sessão.</summary>
public sealed class AuthService(IUserRepository users, SessionState session, TimeProvider clock)
{
    // Usado quando o e-mail não existe, para o login levar o mesmo tempo e não revelar quais e-mails têm conta.
    private static readonly (byte[] Hash, byte[] Salt) Decoy = PasswordHasher.Hash(Guid.NewGuid().ToString());

    public async Task<AuthResult> SignUpAsync(string? name, string? email, string? password)
    {
        var displayName = name?.Trim();
        var normalizedEmail = EmailNormalizer.Normalize(email);
        if (!SignupValidator.IsValid(displayName, normalizedEmail, password))
            return AuthResult.Fail(AuthResult.InvalidSignup);
        if (await users.GetByEmailAsync(normalizedEmail) is not null)
            return AuthResult.Fail(AuthResult.EmailTaken);

        var (hash, salt) = PasswordHasher.Hash(password!);
        var user = new User(Guid.NewGuid(), displayName!, normalizedEmail, hash, salt, clock.GetUtcNow().UtcDateTime);
        // O índice único do banco é a garantia final se dois cadastros com o mesmo e-mail correrem juntos.
        if (!await users.TryAddAsync(user))
            return AuthResult.Fail(AuthResult.EmailTaken);

        await session.SignInAsync(user);
        return AuthResult.Success;
    }

    public async Task<AuthResult> SignInAsync(string? email, string? password)
    {
        var user = await users.GetByEmailAsync(EmailNormalizer.Normalize(email));
        var (hash, salt) = user is null ? Decoy : (user.PasswordHash, user.Salt);
        if (!PasswordHasher.Verify(password ?? "", hash, salt) || user is null)
            return AuthResult.Fail(AuthResult.InvalidLogin);

        await session.SignInAsync(user);
        return AuthResult.Success;
    }
}
