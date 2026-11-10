using System.Security.Cryptography;

namespace Social_Anxiety.Core.Auth;

/// <summary>PBKDF2 com SHA-256, salt aleatório de 16 bytes por usuário e 100.000 iterações.</summary>
public static class PasswordHasher
{
    public const int Iterations = 100_000;
    public const int SaltSize = 16;
    public const int HashSize = 32;

    public static (byte[] Hash, byte[] Salt) Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        return (Derive(password, salt), salt);
    }

    public static bool Verify(string password, byte[] hash, byte[] salt) =>
        CryptographicOperations.FixedTimeEquals(Derive(password, salt), hash);

    private static byte[] Derive(string password, byte[] salt) =>
        Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
}
