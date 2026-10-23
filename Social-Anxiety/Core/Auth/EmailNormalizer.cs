namespace Social_Anxiety.Core.Auth;

public static class EmailNormalizer
{
    /// <summary>Remove espaços das pontas e passa para minúsculas. Usado antes de gravar e antes de buscar.</summary>
    public static string Normalize(string? email) => (email ?? "").Trim().ToLowerInvariant();
}
