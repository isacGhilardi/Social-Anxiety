namespace Social_Anxiety.Core.Auth;

/// <summary>Conta local simulada: não há backend; os dados ficam só neste aparelho.</summary>
public sealed record User(Guid Id, string DisplayName, string Email, byte[] PasswordHash, byte[] Salt, DateTime CreatedAtUtc);
