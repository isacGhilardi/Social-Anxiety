using System.Text.RegularExpressions;

namespace Social_Anxiety.Core.Auth;

public static partial class SignupValidator
{
    public const int MaxNameLength = 32;
    public const int MinPasswordLength = 6;

    public static bool IsValidName(string? name) =>
        !string.IsNullOrWhiteSpace(name) && name.Length <= MaxNameLength && name == name.Trim();

    // shortcut: formato simples (algo@algo.algo, sem espaços); não confirma que o e-mail existe, o que não importa numa conta local.
    public static bool IsValidEmail(string? email) => email is not null && EmailFormat().IsMatch(email);

    public static bool IsValidPassword(string? password) => password is not null && password.Length >= MinPasswordLength;

    public static bool IsValid(string? name, string? email, string? password) =>
        IsValidName(name) && IsValidEmail(email) && IsValidPassword(password);

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailFormat();
}
